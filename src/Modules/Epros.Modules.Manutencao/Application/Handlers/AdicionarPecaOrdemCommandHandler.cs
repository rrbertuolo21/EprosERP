using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Handlers
{
    public class AdicionarPecaOrdemCommandHandler : ICommandHandler<AdicionarPecaOrdemCommand>
    {
        private readonly ContextManutencao _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarPecaOrdemCommandHandler(
            ContextManutencao context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarPecaOrdemCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var ordem = await _context.OrdensManutencao
                .Include(o => o.Pecas)
                .FirstOrDefaultAsync(o => o.Id == request.OrdemManutencaoId, cancellationToken);

            if (ordem == null)
            {
                return CommandResult.Falha("Ordem de manutenção não encontrada.");
            }

            ordem.AdicionarPeca(request.ProdutoId, request.Quantidade, usuario);

            if (!ordem.IsValid)
            {
                return CommandResult.Falha(ordem.Notifications.Select(n => n.Message));
            }

            var novaPeca = ordem.Pecas.Last();
            _context.Entry(novaPeca).State = EntityState.Added;

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Peça adicionada à ordem com sucesso!", new { PecaId = novaPeca.Id });
        }
    }
}
