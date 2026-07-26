using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.RH.Application.Queries;
using Epros.Modules.RH.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.RH.Application.Handlers
{
    public class ObterColaboradoresQueryHandler : IQueryHandler<ObterColaboradoresQuery, CommandResult>
    {
        private readonly ContextRH _context;

        public ObterColaboradoresQueryHandler(ContextRH context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterColaboradoresQuery request, CancellationToken cancellationToken)
        {
            var colaboradores = await _context.Colaboradores
                .OrderBy(c => c.Nome)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Colaboradores listados com sucesso!", colaboradores);
        }
    }

    public class ObterFolhasPagamentoQueryHandler : IQueryHandler<ObterFolhasPagamentoQuery, CommandResult>
    {
        private readonly ContextRH _context;

        public ObterFolhasPagamentoQueryHandler(ContextRH context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterFolhasPagamentoQuery request, CancellationToken cancellationToken)
        {
            var folhas = await _context.FolhasPagamento
                .Include(f => f.Verbas)
                .OrderByDescending(f => f.AnoCompetencia)
                .ThenByDescending(f => f.MesCompetencia)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Folhas de pagamento listadas com sucesso!", folhas);
        }
    }
}
