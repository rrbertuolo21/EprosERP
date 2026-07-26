using System;
using System.Linq;
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
    /// Cancela a devolução fiscal (EF_DEVOLUCAO_FISCAL 8.5). Se já APROVADA, envia o evento de cancelamento
    /// à SEFAZ (via <see cref="IHerculesFiscalService.CancelarAsync"/>) usando a chave/protocolo gerados;
    /// se ainda NOVO (não transmitida), cancela localmente. Preserva as chaves rastreáveis (REG-DEV-019).
    /// </summary>
    public class CancelarDevolucaoFiscalCommandHandler : ICommandHandler<CancelarDevolucaoFiscalCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IHerculesFiscalService _fiscalService;

        public CancelarDevolucaoFiscalCommandHandler(
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

        public async Task<CommandResult> Handle(CancelarDevolucaoFiscalCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var devolucao = await _context.DevolucoesFiscais
                .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken);

            if (devolucao is null)
                return CommandResult.Falha("Devolução fiscal não localizada.");

            if (!devolucao.PodeCancelar())
                return CommandResult.Falha($"Devolução não elegível para cancelamento (estado atual: {devolucao.Estado}).");

            // NOVO (nunca transmitida): cancelamento apenas local.
            if (devolucao.Estado == EEstadoDevolucaoFiscal.Novo)
            {
                devolucao.Cancelar("Cancelamento local (devolução não transmitida).", null, usuario);
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok("Devolução cancelada (estado NOVO, sem transmissão prévia).");
            }

            // APROVADA: envia evento de cancelamento à SEFAZ. Exige chave gerada (REG-DEV-009/MSG-DEV-009).
            if (string.IsNullOrWhiteSpace(devolucao.ChaveGerada) || string.IsNullOrWhiteSpace(devolucao.Protocolo))
                return CommandResult.Falha("Chave gerada/protocolo da devolução não localizados — não é possível cancelar na SEFAZ.");

            var documento = ConstruirDocumentoParaEvento(devolucao, usuario);
            var retorno = await _fiscalService.CancelarAsync(documento, request.Justificativa);

            // 135 = homologado; 155 = homologado fora de prazo; 101 = cancelamento já homologado.
            if (retorno.Sucesso && (retorno.StatusSefaz == 135 || retorno.StatusSefaz == 155 || retorno.StatusSefaz == 101))
            {
                devolucao.Cancelar(retorno.Motivo, retorno.XmlRetorno, usuario);
                await _context.SaveChangesAsync(cancellationToken);
                return CommandResult.Ok("Devolução cancelada na SEFAZ com sucesso!");
            }

            return CommandResult.Falha(new[] { retorno.Motivo }, $"Falha ao cancelar a devolução na SEFAZ (Status: {retorno.StatusSefaz}).");
        }

        /// <summary>Constrói um DocumentoFiscal transitório com chave/protocolo/empresa para o evento SEFAZ.</summary>
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

            // Preenche chave/protocolo (usados por CancelarAsync); demais campos são irrelevantes ao evento.
            documento.Autorizar(devolucao.ChaveGerada!, devolucao.Protocolo!, 100, string.Empty, devolucao.XmlRetorno ?? string.Empty, string.Empty, string.Empty);
            return documento;
        }
    }
}
