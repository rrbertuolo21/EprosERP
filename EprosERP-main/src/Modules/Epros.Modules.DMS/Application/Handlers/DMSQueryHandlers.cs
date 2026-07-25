using System.Collections.Generic;
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
    public class ObterVendasVeiculosQueryHandler : IQueryHandler<ObterVendasVeiculosQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterVendasVeiculosQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterVendasVeiculosQuery request, CancellationToken cancellationToken)
        {
            var vendas = await _context.VendasVeiculos
                .OrderByDescending(v => v.PrecoVenda)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Vendas de veículos listadas com sucesso!", vendas);
        }
    }

    public class ObterOrdensServicoDmsQueryHandler : IQueryHandler<ObterOrdensServicoDmsQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterOrdensServicoDmsQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterOrdensServicoDmsQuery request, CancellationToken cancellationToken)
        {
            var ordens = await _context.OrdensServicoDms
                .OrderByDescending(o => o.NumeroOs)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Ordens de serviço da oficina listadas com sucesso!", ordens);
        }
    }

    public class ObterGarantiasMontadoraQueryHandler : IQueryHandler<ObterGarantiasMontadoraQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterGarantiasMontadoraQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterGarantiasMontadoraQuery request, CancellationToken cancellationToken)
        {
            var garantias = await _context.GarantiasMontadora
                .OrderByDescending(g => g.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Garantias da montadora listadas com sucesso!", garantias);
        }
    }
}
