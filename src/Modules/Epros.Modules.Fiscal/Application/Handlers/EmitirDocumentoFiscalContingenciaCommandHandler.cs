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
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>
    /// Emissão em CONTINGÊNCIA. Espelha <see cref="EmitirDocumentoFiscalCommandHandler"/> (mesmo agregado,
    /// cálculo e motor), acrescentando a transição para o modo de contingência:
    /// - SVC-AN/SVC-RS/EPEC (online): transmite imediatamente ao webservice de contingência.
    /// - Offline-NFCe (tpEmis=9): NÃO transmite agora (SEFAZ indisponível); grava o documento com Status
    ///   "PendenteContingencia" para reenvio posterior (<see cref="ReprocessarContingenciaCommandHandler"/>).
    /// O caminho normal (EmitirDocumentoFiscalCommand) não é tocado.
    /// </summary>
    public class EmitirDocumentoFiscalContingenciaCommandHandler : ICommandHandler<EmitirDocumentoFiscalContingenciaCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IHerculesFiscalService _fiscalService;
        private readonly CalculadoraImpostosDocumentoFiscal _calculadora;

        public EmitirDocumentoFiscalContingenciaCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IHerculesFiscalService fiscalService,
            CalculadoraImpostosDocumentoFiscal calculadora)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _fiscalService = fiscalService;
            _calculadora = calculadora;
        }

        public async Task<CommandResult> Handle(EmitirDocumentoFiscalContingenciaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var existe = await _context.DocumentosFiscais.AnyAsync(d =>
                d.Modelo == request.Modelo &&
                d.Serie == request.Serie &&
                d.Numero == request.Numero, cancellationToken);

            if (existe)
                return CommandResult.Falha("Já existe um documento fiscal emitido com esta mesma série e número.");

            var documento = new DocumentoFiscal(
                request.Modelo,
                request.Ambiente,
                request.Serie,
                request.Numero,
                request.Total,
                request.DestinatarioCnpjCpf,
                request.DestinatarioNome,
                tenantId,
                usuario);

            if (!documento.IsValid)
                return CommandResult.Falha(documento.Notifications.Select(n => n.Message), "Dados do documento fiscal são inválidos.");

            if (request.EmpresaId is not null && request.EmpresaId != Guid.Empty)
                documento.VincularEmpresaEmitente(request.EmpresaId.Value);

            foreach (var itemInput in request.Itens)
            {
                documento.AdicionarItem(
                    itemInput.Sku, itemInput.NomeProduto, itemInput.Cst, itemInput.Cfop, itemInput.Ncm,
                    itemInput.Quantidade, itemInput.ValorUnitario, itemInput.AliquotaIcms, usuario);
            }

            if (!documento.IsValid)
                return CommandResult.Falha(documento.Notifications.Select(n => n.Message), "Erro ao validar itens do documento fiscal.");

            // Aplica a contingência (tpEmis/xJust/dhCont) — o mapper monta o XML com esses dados.
            var tipoEmissao = (ETipoEmissaoFiscal)request.TipoEmissao;
            documento.EntrarContingencia(tipoEmissao, request.Justificativa);

            _calculadora.CalcularEAplicar(documento);
            documento.Submeter();
            _context.DocumentosFiscais.Add(documento);

            // Contingência OFFLINE (NFC-e): não transmite agora — fica pendente para reenvio quando a SEFAZ voltar.
            if (tipoEmissao == ETipoEmissaoFiscal.ContingenciaOffline)
            {
                documento.MarcarPendenteContingencia();
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Ok(
                    "Documento emitido em contingência offline. Aguardando reenvio à SEFAZ (transmissão diferida).",
                    new { DocumentoFiscalId = documento.Id, documento.Status, TipoEmissao = tipoEmissao.ToString() });
            }

            // Contingências online (SVC-AN/SVC-RS/EPEC): transmite já, roteado ao WS de contingência.
            var resultado = await _fiscalService.EmitirAsync(documento);

            if (resultado.Sucesso && resultado.StatusSefaz == 100)
            {
                documento.Autorizar(resultado.ChaveAcesso, resultado.Protocolo, resultado.StatusSefaz,
                    resultado.XmlEnvio, resultado.XmlRetorno, resultado.PdfCaminho, resultado.XmlCaminho);

                var payload = JsonSerializer.Serialize(new
                {
                    DocumentoFiscalId = documento.Id,
                    TenantId = tenantId,
                    documento.ChaveAcesso,
                    documento.Protocolo,
                    documento.Modelo,
                    documento.Numero,
                    documento.Total,
                    TipoEmissao = tipoEmissao.ToString()
                });
                _context.OutboxMessages.Add(new OutboxMessage(tenantId, "DocumentoFiscalAutorizado", payload));

                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Ok("Documento fiscal autorizado em contingência com sucesso!", new
                {
                    DocumentoFiscalId = documento.Id,
                    documento.ChaveAcesso,
                    documento.Protocolo,
                    documento.Status,
                    TipoEmissao = tipoEmissao.ToString()
                });
            }

            documento.Rejeitar(resultado.StatusSefaz, resultado.Motivo, resultado.XmlRetorno);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Falha(new[] { resultado.Motivo }, $"O documento em contingência foi rejeitado pela SEFAZ (Status: {resultado.StatusSefaz}).");
        }
    }
}
