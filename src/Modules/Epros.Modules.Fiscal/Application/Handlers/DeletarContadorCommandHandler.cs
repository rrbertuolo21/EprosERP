using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Application.Commands;
using Epros.Modules.Fiscal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Handlers
{
    public class DeletarContadorCommandHandler : ICommandHandler<DeletarContadorCommand>
    {
        private readonly ContextFiscal _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public DeletarContadorCommandHandler(
            ContextFiscal context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(DeletarContadorCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var usuario = _currentUser.GetUserId() ?? "system";

            var contador = await _context.Contadores
                .FirstOrDefaultAsync(c => c.Id == request.Id && c.TenantId == tenantId && c.DeletadoEm == null, cancellationToken);

            if (contador == null)
            {
                return CommandResult.Falha("Contador não encontrado.");
            }

            contador.Deletar(usuario);
            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Contador deletado com sucesso!");
        }
    }
}
