using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Epros.Modules.Fiscal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>
    /// Transmite a devolução fiscal à SEFAZ (EF_DEVOLUCAO_FISCAL 8.4). Monta um <see cref="DocumentoFiscal"/>
    /// transitório de finalidade DEVOLUÇÃO (finNFe=4, referenciando a chave da NF de entrada), reusa o
    /// cálculo (<see cref="CalculadoraImpostosDocumentoFiscal"/>) e o motor via
    /// <see cref="IHerculesFiscalService.EmitirAsync"/>. Atualiza o estado da devolução conforme o retorno.
    /// </summary>
    public class TransmitirDevolucaoFiscalCommandHandler : ICommandHandler<TransmitirDevolucaoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IHerculesFiscalService _fiscalService;
        private readonly CalculadoraImpostosDocumentoFiscal _calculadora;

        public TransmitirDevolucaoFiscalCommandHandler(
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

        public async Task<CommandResult> Handle(TransmitirDevolucaoFiscalCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var devolucao = await _context.DevolucoesFiscais
                .Include(d => d.Itens)
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (devolucao is null)
                return CommandResult.Falha("Devolução fiscal não localizada.");

            // REG-DEV-012/013/018: valida estado, referência fiscal e presença de itens.
            if (!devolucao.PodeTransmitir(out var motivoBloqueio))
                return CommandResult.Falha(motivoBloqueio);

            // Número gerado: participa da sequência fiscal compartilhada (REG-DEV-010). Próximo número =
            // maior número já usado (NF-e/NFC-e emitidas + devoluções aprovadas) para o modelo/série + 1.
            var numeroGerado = await ProximoNumeroAsync(devolucao.Modelo, devolucao.Serie, cancellationToken);

            // Monta um DocumentoFiscal transitório (NÃO persistido) apenas para alimentar o motor.
            var documento = new DocumentoFiscal(
                devolucao.Modelo,
                devolucao.Ambiente,
                devolucao.Serie,
                numeroGerado,
                devolucao.Total,
                devolucao.DestinatarioCnpjCpf,
                devolucao.DestinatarioNome,
                tenantId,
                usuario);

            if (devolucao.EmpresaId is not null && devolucao.EmpresaId != Guid.Empty)
                documento.VincularEmpresaEmitente(devolucao.EmpresaId.Value);

            documento.DefinirDevolucao(devolucao.ChaveNfEntrada);

            foreach (var item in devolucao.Itens)
                documento.AdicionarItem(item.Sku, item.NomeProduto, item.Cst, item.Cfop, item.Ncm,
                    item.Quantidade, item.ValorUnitario, item.AliquotaIcms, usuario);

            if (!documento.IsValid)
                return CommandResult.Falha(documento.Notifications.Select(n => n.Message), "Erro ao montar o documento de devolução.");

            _calculadora.CalcularEAplicar(documento);
            devolucao.RegistrarTransmissao();

            var resultado = await _fiscalService.EmitirAsync(documento);

            if (resultado.Sucesso && resultado.StatusSefaz == 100)
            {
                // REG-DEV-004/008/009: grava APROVADO + chave gerada + número gerado + protocolo.
                devolucao.Aprovar(resultado.ChaveAcesso, numeroGerado, resultado.Protocolo, resultado.XmlRetorno, usuario);
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Ok("Devolução fiscal transmitida e APROVADA.", new
                {
                    devolucao.Id,
                    Estado = devolucao.Estado.ToString(),
                    devolucao.ChaveGerada,
                    devolucao.NumeroGerado,
                    devolucao.Protocolo
                });
            }

            // REG-DEV-005: grava REJEITADO (permite correção/retransmissão).
            devolucao.Rejeitar($"[{resultado.StatusSefaz}] {resultado.Motivo}", resultado.XmlRetorno, usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Falha(new[] { resultado.Motivo }, $"Devolução REJEITADA pela SEFAZ (Status: {resultado.StatusSefaz}).");
        }

        private async Task<long> ProximoNumeroAsync(string modelo, int serie, CancellationToken ct)
        {
            var maxDoc = await _context.DocumentosFiscais
                .Where(d => d.Modelo == modelo && d.Serie == serie)
                .Select(d => (long?)d.Numero)
                .MaxAsync(ct) ?? 0;

            var maxDev = await _context.DevolucoesFiscais
                .Where(d => d.Modelo == modelo && d.Serie == serie && d.NumeroGerado != null)
                .Select(d => d.NumeroGerado)
                .MaxAsync(ct) ?? 0;

            return Math.Max(maxDoc, maxDev) + 1;
        }
    }
}
