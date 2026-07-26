using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Projetos.Application.Commands;
using Epros.Modules.Projetos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    public class AdicionarWbsItemCommandHandler : ICommandHandler<AdicionarWbsItemCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AdicionarWbsItemCommandHandler(
            ContextProjetos context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AdicionarWbsItemCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var projeto = await _context.Projetos
                .Include(p => p.ItensWbs)
                .FirstOrDefaultAsync(p => p.Id == request.ProjetoId, cancellationToken);

            if (projeto == null)
            {
                return CommandResult.Falha("Projeto não encontrado.");
            }

            projeto.AdicionarItemWbs(
                request.Nome,
                request.Descricao,
                request.DataInicio,
                request.DataTermino,
                request.PesoPonderado,
                usuario
            );

            if (!projeto.IsValid)
            {
                return CommandResult.Falha(projeto.Notifications.Select(n => n.Message));
            }

            // Precisamos marcar as entidades filhas explicitly se tiver ID instanciado nos construtores
            var novoItem = projeto.ItensWbs.Last();
            _context.Entry(novoItem).State = EntityState.Added;

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Tarefa WBS adicionada com sucesso!", new { WbsItemId = novoItem.Id });
        }
    }
}
