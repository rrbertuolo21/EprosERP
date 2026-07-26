using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.DMS.Application.Commands;
using Epros.Modules.DMS.Domain.Entities;
using Epros.Modules.DMS.Infrastructure.Data;

namespace Epros.Modules.DMS.Application.Handlers
{
    public class CriarOrcamentoManutencaoCommandHandler : ICommandHandler<CriarOrcamentoManutencaoCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarOrcamentoManutencaoCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarOrcamentoManutencaoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var orcamento = new OrcamentoManutencao(
                request.OrdemServicoId,
                request.Validade,
                request.ValorTotal,
                tenantId,
                usuario
            );

            if (!orcamento.IsValid)
            {
                return CommandResult.Falha(orcamento.Notifications.Select(n => n.Message));
            }

            _context.OrcamentosManutencao.Add(orcamento);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Orçamento de manutenção criado com sucesso!", new { orcamento.Id });
        }
    }
}
