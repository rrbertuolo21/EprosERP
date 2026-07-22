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
    public class AlocarRecursoCommandHandler : ICommandHandler<AlocarRecursoCommand>
    {
        private readonly ContextProjetos _context;
        private readonly ICurrentUser _currentUser;

        public AlocarRecursoCommandHandler(
            ContextProjetos context,
            ICurrentUser currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<CommandResult> Handle(AlocarRecursoCommand request, CancellationToken cancellationToken)
        {
            var usuario = _currentUser.GetUserId() ?? "system";

            var projeto = await _context.Projetos
                .Include(p => p.Alocacoes)
                .FirstOrDefaultAsync(p => p.Id == request.ProjetoId, cancellationToken);

            if (projeto == null)
            {
                return CommandResult.Falha("Projeto não encontrado.");
            }

            projeto.AlocarRecurso(
                request.ColaboradorId,
                request.Funcao,
                request.CustoHora,
                request.HorasPlanejadas,
                usuario
            );

            if (!projeto.IsValid)
            {
                return CommandResult.Falha(projeto.Notifications.Select(n => n.Message));
            }

            var novaAlocacao = projeto.Alocacoes.Last();
            _context.Entry(novaAlocacao).State = EntityState.Added;

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResult.Ok("Recurso alocado com sucesso!", new { AlocacaoId = novaAlocacao.Id });
        }
    }
}
