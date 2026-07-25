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
    public class RegistrarRiscoCommandHandler : ICommandHandler<RegistrarRiscoCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RegistrarRiscoCommandHandler(
            ContextGRC context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(RegistrarRiscoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var risco = new RiscoCorporativo(
                request.Titulo,
                request.Descricao,
                request.Categoria,
                request.Probabilidade,
                request.Impacto,
                tenantId,
                usuario
            );

            if (!risco.IsValid)
            {
                return CommandResult.Falha(risco.Notifications.Select(n => n.Message));
            }

            _context.RiscosCorporativos.Add(risco);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Risco corporativo registrado com sucesso!", new { RiscoId = risco.Id, NivelRisco = risco.NivelRisco, Status = risco.Status });
        }
    }
}
