using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.DMS.Application.Queries;
using Epros.Modules.DMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.DMS.Application.Handlers
{
    public class ObterPecasReposicaoQueryHandler : IQueryHandler<ObterPecasReposicaoQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterPecasReposicaoQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterPecasReposicaoQuery request, CancellationToken cancellationToken)
        {
            var pecas = await _context.PecasReposicao
                .OrderByDescending(p => p.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Peças de reposição listadas com sucesso!", pecas);
        }
    }

    public class ObterDemandasPecaQueryHandler : IQueryHandler<ObterDemandasPecaQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterDemandasPecaQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterDemandasPecaQuery request, CancellationToken cancellationToken)
        {
            var demandas = await _context.DemandasPeca
                .OrderByDescending(d => d.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Demandas de peça listadas com sucesso!", demandas);
        }
    }

    public class ObterReservasPecaQueryHandler : IQueryHandler<ObterReservasPecaQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterReservasPecaQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterReservasPecaQuery request, CancellationToken cancellationToken)
        {
            var reservas = await _context.ReservasPeca
                .OrderByDescending(r => r.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Reservas de peça listadas com sucesso!", reservas);
        }
    }
}
