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
    public class CriarRedeNoCommandHandler : ICommandHandler<CriarRedeNoCommand>
    {
        private readonly ContextDMS _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public CriarRedeNoCommandHandler(
            ContextDMS context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(CriarRedeNoCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var entidade = new RedeNo(
                request.Codigo,
                request.TipoNo,
                request.PaiId,
                request.PessoaEmpresaId,
                request.LocalId,
                request.InicioVigencia,
                request.FimVigencia,
                tenantId,
                usuario);

            if (!entidade.IsValid)
            {
                return CommandResult.Falha(entidade.Notifications.Select(n => n.Message));
            }

            _context.RedeNos.Add(entidade);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Registro criado com sucesso!", new { entidade.Id });
        }
    }
}
