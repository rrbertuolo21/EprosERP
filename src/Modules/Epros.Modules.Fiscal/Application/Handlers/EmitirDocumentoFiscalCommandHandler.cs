using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class EmitirDocumentoFiscalCommandHandler : ICommandHandler<EmitirDocumentoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IHerculesFiscalService _fiscalService;
        private readonly CalculadoraImpostosDocumentoFiscal _calculadora;
        private readonly IDanfeService _danfeService;
        private readonly IArmazenamentoArquivoFiscal _armazenamento;

        public EmitirDocumentoFiscalCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IHerculesFiscalService fiscalService,
            CalculadoraImpostosDocumentoFiscal calculadora,
            IDanfeService danfeService,
            IArmazenamentoArquivoFiscal armazenamento)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _fiscalService = fiscalService;
            _calculadora = calculadora;
            _danfeService = danfeService;
            _armazenamento = armazenamento;
        }

        public async Task<CommandResult> Handle(EmitirDocumentoFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            // 1. Evitar lançamento em duplicidade
            var existe = await _context.DocumentosFiscais.AnyAsync(d =>
                d.Modelo == request.Modelo &&
                d.Serie == request.Serie &&
                d.Numero == request.Numero, cancellationToken);

            if (existe)
            {
                return CommandResult.Falha("Já existe um documento fiscal emitido com esta mesma série e número.");
            }

            // 2. Instanciar agregado raiz
            var documento = new DocumentoFiscal(
                request.Modelo,
                request.Ambiente,
                request.Serie,
                request.Numero,
                request.Total,
                request.DestinatarioCnpjCpf,
                request.DestinatarioNome,
                tenantId,
                usuario
            );

            if (!documento.IsValid)
            {
                return CommandResult.Falha(documento.Notifications.Select(n => n.Message), "Dados do documento fiscal são inválidos.");
            }

            // Vincula a empresa emitente (resolve certificado/params fiscais para cálculo e transmissão).
            if (request.EmpresaId is not null && request.EmpresaId != Guid.Empty)
                documento.VincularEmpresaEmitente(request.EmpresaId.Value);

            // 3. Adicionar itens
            foreach (var itemInput in request.Itens)
            {
                documento.AdicionarItem(
                    itemInput.Sku,
                    itemInput.NomeProduto,
                    itemInput.Cst,
                    itemInput.Cfop,
                    itemInput.Ncm,
                    itemInput.Quantidade,
                    itemInput.ValorUnitario,
                    itemInput.AliquotaIcms,
                    usuario
                );
            }

            if (!documento.IsValid)
            {
                return CommandResult.Falha(documento.Notifications.Select(n => n.Message), "Erro ao validar itens do documento fiscal.");
            }

            // 3.1. Calcular impostos reais de cada item (ICMS/ST/FCP/IPI/PIS/COFINS) antes de transmitir.
            // Resolve o emitente (UF de origem + regime tributário) via provider; se ausente, o cálculo
            // usa defaults neutros e o item mantém apenas o ICMS informado (comportamento degradado honesto).
            _calculadora.CalcularEAplicar(documento);

            documento.Submeter();
            _context.DocumentosFiscais.Add(documento);

            // 4. Executar emissão/transmissão fiscal com Hercules.NET
            var resultadoEmissao = await _fiscalService.EmitirAsync(documento);

            if (resultadoEmissao.Sucesso && resultadoEmissao.StatusSefaz == 100)
            {
                documento.Autorizar(
                    resultadoEmissao.ChaveAcesso,
                    resultadoEmissao.Protocolo,
                    resultadoEmissao.StatusSefaz,
                    resultadoEmissao.XmlEnvio,
                    resultadoEmissao.XmlRetorno,
                    resultadoEmissao.PdfCaminho,
                    resultadoEmissao.XmlCaminho
                );

                // Persistir XML autorizado + PDF do DANFE/cupom no armazenamento (local hoje; MinIO futuro)
                // e gravar os caminhos no documento. Falha aqui não invalida a autorização já obtida.
                await PersistirArtefatosAsync(documento, resultadoEmissao.XmlRetorno, cancellationToken);

                // Enfileirar evento no Outbox de forma transacional
                var fiscalEvent = new
                {
                    DocumentoFiscalId = documento.Id,
                    TenantId = tenantId,
                    ChaveAcesso = documento.ChaveAcesso,
                    Protocolo = documento.Protocolo,
                    Modelo = documento.Modelo,
                    Numero = documento.Numero,
                    Total = documento.Total,
                    DestinatarioCnpjCpf = documento.DestinatarioCnpjCpf
                };

                var payloadJson = JsonSerializer.Serialize(fiscalEvent);
                var outboxMessage = new OutboxMessage(tenantId, "DocumentoFiscalAutorizado", payloadJson);
                _context.OutboxMessages.Add(outboxMessage);

                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Ok("Documento fiscal emitido e autorizado com sucesso!", new
                {
                    DocumentoFiscalId = documento.Id,
                    ChaveAcesso = documento.ChaveAcesso,
                    Protocolo = documento.Protocolo,
                    Status = documento.Status
                });
            }
            else
            {
                documento.Rejeitar(
                    resultadoEmissao.StatusSefaz,
                    resultadoEmissao.Motivo,
                    resultadoEmissao.XmlRetorno
                );

                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Falha(new[] { resultadoEmissao.Motivo }, $"O documento foi rejeitado pela SEFAZ (Status: {resultadoEmissao.StatusSefaz}).");
            }
        }

        /// <summary>
        /// Gera o PDF do DANFE/cupom e salva XML autorizado + PDF no armazenamento, gravando os caminhos
        /// no documento. Best-effort: qualquer falha aqui é engolida (a autorização SEFAZ já ocorreu; os
        /// artefatos podem ser regerados sob demanda pelo endpoint de DANFE).
        /// </summary>
        private async Task PersistirArtefatosAsync(DocumentoFiscal documento, string xmlRetorno, CancellationToken ct)
        {
            try
            {
                var chaveLogica = string.IsNullOrWhiteSpace(documento.ChaveAcesso)
                    ? documento.Id.ToString()
                    : documento.ChaveAcesso;

                string? xmlCaminho = null;
                if (!string.IsNullOrWhiteSpace(xmlRetorno))
                {
                    var xmlBytes = System.Text.Encoding.UTF8.GetBytes(xmlRetorno);
                    xmlCaminho = await _armazenamento.SalvarAsync(chaveLogica, $"{chaveLogica}.xml", xmlBytes, "application/xml", ct);
                }

                var pdfBytes = _danfeService.GerarPdf(documento);
                var pdfCaminho = await _armazenamento.SalvarAsync(chaveLogica, $"{chaveLogica}.pdf", pdfBytes, "application/pdf", ct);

                documento.DefinirCaminhosArquivos(xmlCaminho, pdfCaminho);
            }
            catch
            {
                // Não interrompe: o DANFE pode ser regerado pelo endpoint sob demanda.
            }
        }
    }
}
