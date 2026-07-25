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
    public class CriarControleCommandHandler : ICommandHandler<CriarControleCommand>
    {
        private readonly ContextGRC _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarControleCommandHandler(
            ContextGRC context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarControleCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var controle = new ControleInterno(
                request.Codigo,
                request.Nome,
                request.Descricao,
                request.Frequencia,
                tenantId,
                usuario
            );

            if (!controle.IsValid)
            {
                return CommandResult.Falha(controle.Notifications.Select(n => n.Message));
            }

            _context.ControlesInternos.Add(controle);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Controle interno criado com sucesso!", new { ControleId = controle.Id });
        }
    }
}
