using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Projetos.Application.Queries;
using Epros.Modules.Projetos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Projetos.Application.Handlers
{
    public class ObterProjetosQueryHandler : IQueryHandler<ObterProjetosQuery, CommandResult>
    {
        private readonly ContextProjetos _context;

        public ObterProjetosQueryHandler(ContextProjetos context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterProjetosQuery request, CancellationToken cancellationToken)
        {
            var projetos = await _context.Projetos
                .OrderByDescending(p => p.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Projetos listados com sucesso!", projetos);
        }
    }

    public class ObterProjetoPorIdQueryHandler : IQueryHandler<ObterProjetoPorIdQuery, CommandResult>
    {
        private readonly ContextProjetos _context;

        public ObterProjetoPorIdQueryHandler(ContextProjetos context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterProjetoPorIdQuery request, CancellationToken cancellationToken)
        {
            var projeto = await _context.Projetos
                .Include(p => p.ItensWbs)
                .Include(p => p.Alocacoes)
                .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

            if (projeto == null)
            {
                return CommandResult.Falha("Projeto não encontrado.");
            }

            return CommandResult.Ok("Projeto carregado com sucesso!", projeto);
        }
    }
}
