using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Manutencao.Application.Queries;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Manutencao.Application.Handlers
{
    public class ObterEquipamentosQueryHandler : IQueryHandler<ObterEquipamentosQuery, CommandResult>
    {
        private readonly ContextManutencao _context;

        public ObterEquipamentosQueryHandler(ContextManutencao context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterEquipamentosQuery request, CancellationToken cancellationToken)
        {
            var equipamentos = await _context.Equipamentos
                .OrderBy(e => e.Codigo)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Equipamentos listados com sucesso!", equipamentos);
        }
    }

    public class ObterOrdensManutencaoQueryHandler : IQueryHandler<ObterOrdensManutencaoQuery, CommandResult>
    {
        private readonly ContextManutencao _context;

        public ObterOrdensManutencaoQueryHandler(ContextManutencao context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterOrdensManutencaoQuery request, CancellationToken cancellationToken)
        {
            var ordens = await _context.OrdensManutencao
                .Include(o => o.Pecas)
                .OrderByDescending(o => o.DataAbertura)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Ordens de serviço de manutenção listadas com sucesso!", ordens);
        }
    }
}
