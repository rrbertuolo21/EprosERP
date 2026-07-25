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
    public class ObterJornadasFinQueryHandler : IQueryHandler<ObterJornadasFinQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterJornadasFinQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterJornadasFinQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.JornadasFin
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }

    public class ObterSimulacoesFinQueryHandler : IQueryHandler<ObterSimulacoesFinQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterSimulacoesFinQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterSimulacoesFinQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.SimulacoesFin
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }

    public class ObterContratosFinQueryHandler : IQueryHandler<ObterContratosFinQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterContratosFinQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterContratosFinQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.ContratosFin
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }
}
