using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Producao.Application.Commands;
using Epros.Modules.Producao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Producao.Application.Handlers
{
    public class IniciarProducaoCommandHandler : ICommandHandler<IniciarProducaoCommand>
    {
        private readonly ContextProducao _context;
        private readonly ICurrentUser _currentUser;

        public IniciarProducaoCommandHandler(
            ContextProducao context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(IniciarProducaoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.OrdensProducao
                .FirstOrDefaultAsync(o => o.Id == request.OrdemProducaoId, cancellationToken);

            if (ordem == null)
            {
                return CommandResult.Falha("Ordem de Produção não encontrada.");
            }

            ordem.IniciarProducao(usuario);

            if (!ordem.IsValid)
            {
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));
            }

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Produção iniciada com sucesso!", new { OrdemProducaoId = ordem.Id, Status = ordem.Status });
        }
    }
}
