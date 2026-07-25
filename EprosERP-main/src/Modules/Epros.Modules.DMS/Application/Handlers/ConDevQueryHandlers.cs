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
    public class ObterRedeNosQueryHandler : IQueryHandler<ObterRedeNosQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterRedeNosQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterRedeNosQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.RedeNos
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }

    public class ObterContratosRedeQueryHandler : IQueryHandler<ObterContratosRedeQuery, CommandResult>
    {
        private readonly ContextDMS _context;

        public ObterContratosRedeQueryHandler(ContextDMS context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterContratosRedeQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.ContratosRede
                .OrderByDescending(x => x.CriadoEm)
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("Listagem realizada com sucesso!", itens);
        }
    }
}
