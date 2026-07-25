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
    public class ObterEstoqueVeiculosQueryHandler : IQueryHandler<ObterEstoqueVeiculosQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterEstoqueVeiculosQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterEstoqueVeiculosQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.EstoqueVeiculos
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }

    public class ObterReservasVeiculoQueryHandler : IQueryHandler<ObterReservasVeiculoQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterReservasVeiculoQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterReservasVeiculoQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.ReservasVeiculo
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }

    public class ObterPropostasVendaQueryHandler : IQueryHandler<ObterPropostasVendaQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterPropostasVendaQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterPropostasVendaQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.PropostasVenda
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }
}
