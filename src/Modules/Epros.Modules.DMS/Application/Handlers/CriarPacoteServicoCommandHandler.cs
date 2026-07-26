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
    public class CriarPacoteServicoCommandHandler : ICommandHandler<CriarPacoteServicoCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarPacoteServicoCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarPacoteServicoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var pacote = new PacoteServico(
                request.Codigo,
                request.Nome,
                tenantId,
                usuario
            );

            if (!pacote.IsValid)
            {
                return CommandResult.Falha(pacote.Notifications.Select(n => n.Message));
            }

            _context.PacotesServico.Add(pacote);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Pacote de serviço criado com sucesso!", new { pacote.Id });
        }
    }
}
