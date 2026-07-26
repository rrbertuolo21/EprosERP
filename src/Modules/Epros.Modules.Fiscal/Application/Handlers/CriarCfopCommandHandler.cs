using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Domain.Entities;
using Epros.Modules.Fiscal.Infrastructure.Data;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class CriarCfopCommandHandler : ICommandHandler<CriarCfopCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarCfopCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarCfopCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var cfop = new Cfop(
                request.CfopCodigo,
                request.Descricao,
                request.NaturezaOperacao,
                request.CfopCorrelacao,
                request.IntegraFaturamento,
                request.IndicadorNfe,
                request.IndicadorComunicacao,
                request.IndicadorTransporte,
                request.IndicadorDevolucao,
                request.IndicadorRetorno,
                request.IndicadorAnulacao,
                request.IndicadorRemessa,
                request.IndicadorCombustivel,
                request.IndicadorTransferencia,
                request.IndicadorNfce,
                request.IndicadorCiap,
                request.IndicadorUsoConsumo,
                request.IndicadorUsoSemOperacao,
                request.IndicadorSt,
                request.IndicadorMei,
                request.IncidenciaSimples,
                request.CfopDevolucao,
                tenantId,
                usuario
            );

            if (!cfop.IsValid)
            {
                return CommandResult.Falha(cfop.Notifications.Select(n => n.Message), "Erro de validação de domínio do CFOP.");
            }

            _context.Cfops.Add(cfop);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("CFOP criado com sucesso!", new { Id = cfop.Id });
        }
    }
}
