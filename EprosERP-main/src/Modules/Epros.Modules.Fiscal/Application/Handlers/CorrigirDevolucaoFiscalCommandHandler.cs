using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Application.Services;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    /// <summary>
    /// Registra correção da devolução (EF_DEVOLUCAO_FISCAL 8.6). Se APROVADA, envia Carta de Correção
    /// Eletrônica (CC-e) à SEFAZ via <see cref="IHerculesFiscalService.CartaCorrecaoAsync"/> e registra o
    /// evento; caso contrário, registra a correção localmente. Mantém a rastreabilidade (REG-DEV-016).
    /// </summary>
    public class CorrigirDevolucaoFiscalCommandHandler : ICommandHandler<CorrigirDevolucaoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IHerculesFiscalService _fiscalService;

        public CorrigirDevolucaoFiscalCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IHerculesFiscalService fiscalService)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _fiscalService = fiscalService;
        }

        public async Task<CommandResult> Handle(CorrigirDevolucaoFiscalCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var devolucao = await _context.DevolucoesFiscais
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (devolucao is null)
                return CommandResult.Falha("Devolução fiscal não localizada.");

            if (devolucao.Estado == EEstadoDevolucaoFiscal.Cancelado)
                return CommandResult.Falha("Devolução cancelada não é elegível para correção.");

            // Não aprovada ainda: correção apenas registrada localmente (será considerada na retransmissão).
            if (devolucao.Estado != EEstadoDevolucaoFiscal.Aprovado)
            {
                devolucao.RegistrarCorrecao(request.TextoCorrecao, null, usuario);
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok("Correção registrada na devolução (documento ainda não aprovado).");
            }

            // Aprovada: exige chave gerada para a CC-e (MSG-DEV-009).
            if (string.IsNullOrWhiteSpace(devolucao.ChaveGerada))
                return CommandResult.Falha("Chave gerada da devolução não localizada — não é possível enviar carta de correção.");

            var documento = ConstruirDocumentoParaEvento(devolucao, usuario);

            // Sequência do evento: próxima após os eventos já registrados para esta chave.
            var sequencia = await ProximaSequenciaCorrecaoAsync(devolucao.Id, cancellationToken);
            var retorno = await _fiscalService.CartaCorrecaoAsync(documento, request.TextoCorrecao, sequencia);

            // 128 = lote de evento processado; 135 = evento registrado e vinculado.
            if (retorno.Sucesso && (retorno.StatusSefaz == 128 || retorno.StatusSefaz == 135))
            {
                var evento = new EventoDocumentoFiscal(
                    devolucao.Id, "CartaCorrecao", retorno.StatusSefaz, retorno.Motivo, retorno.Protocolo,
                    sequencia, request.TextoCorrecao, retorno.XmlRetorno, devolucao.TenantId, usuario);
                _context.EventosDocumentosFiscais.Add(evento);

                devolucao.RegistrarCorrecao(request.TextoCorrecao, retorno.XmlRetorno, usuario);
                await _context.SaveChangesAsync(cancellationToken);

                return CommandResult.Ok("Carta de correção da devolução registrada na SEFAZ com sucesso!");
            }

            return CommandResult.Falha(new[] { retorno.Motivo }, $"Falha ao registrar a carta de correção da devolução (Status: {retorno.StatusSefaz}).");
        }

        private async Task<int> ProximaSequenciaCorrecaoAsync(Guid devolucaoId, CancellationToken ct)
        {
            var max = await _context.EventosDocumentosFiscais
                .Where(e => e.DocumentoFiscalId == devolucaoId && e.TipoEvento == "CartaCorrecao")
                .Select(e => (int?)e.SequenciaEvento)
                .MaxAsync(ct) ?? 0;
            return max + 1;
        }

        private static DocumentoFiscal ConstruirDocumentoParaEvento(DevolucaoFiscal devolucao, string usuario)
        {
            var documento = new DocumentoFiscal(
                devolucao.Modelo,
                devolucao.Ambiente,
                devolucao.Serie,
                devolucao.NumeroGerado ?? 1,
                devolucao.Total,
                devolucao.DestinatarioCnpjCpf,
                devolucao.DestinatarioNome,
                devolucao.TenantId,
                usuario);

            if (devolucao.EmpresaId is not null && devolucao.EmpresaId != Guid.Empty)
                documento.VincularEmpresaEmitente(devolucao.EmpresaId.Value);

            documento.Autorizar(devolucao.ChaveGerada!, devolucao.Protocolo ?? string.Empty, 100, string.Empty, devolucao.XmlRetorno ?? string.Empty, string.Empty, string.Empty);
            return documento;
        }
    }
}
