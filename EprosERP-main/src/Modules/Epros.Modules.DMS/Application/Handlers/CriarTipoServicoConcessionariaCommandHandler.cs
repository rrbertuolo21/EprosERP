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
    public class CriarTipoServicoConcessionariaCommandHandler : ICommandHandler<CriarTipoServicoConcessionariaCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarTipoServicoConcessionariaCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarTipoServicoConcessionariaCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var tipoServico = new TipoServicoConcessionaria(
                request.Codigo,
                request.Nome,
                request.Descricao,
                tenantId,
                usuario
            );

            if (!tipoServico.IsValid)
            {
                return CommandResult.Falha(tipoServico.Notifications.Select(n => n.Message));
            }

            _context.TiposServicoConcessionaria.Add(tipoServico);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Tipo de serviço da concessionária criado com sucesso!", new { tipoServico.Id });
        }
    }
}
