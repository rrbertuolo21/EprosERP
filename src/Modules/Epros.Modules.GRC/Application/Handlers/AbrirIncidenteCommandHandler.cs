using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.GRC.Application.Commands;
using Epros.Modules.GRC.Domain.Entities;
using Epros.Modules.GRC.Infrastructure.Data;

namespace Epros.Modules.GRC.Application.Handlers
{
    public class AbrirIncidenteCommandHandler : ICommandHandler<AbrirIncidenteCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public AbrirIncidenteCommandHandler(
            ContextGRC context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AbrirIncidenteCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var incidente = new IncidenteCompliance(
                request.Titulo,
                request.Descricao,
                request.Origem,
                request.Gravidade,
                tenantId,
                usuario
            );

            if (!incidente.IsValid)
            {
                return CommandResult.Falha(incidente.Notifications.Select(n => n.Message));
            }

            _context.IncidentesCompliance.Add(incidente);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Incidente de compliance aberto com sucesso!", new { IncidenteId = incidente.Id, Status = incidente.Status });
        }
    }
}
