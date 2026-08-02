using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.ESG.Application.Queries;
using Epros.Modules.ESG.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.ESG.Application.Handlers
{
    public class ListarRegistrosTsuQueryHandler : IQueryHandler<ListarRegistrosTsuQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarRegistrosTsuQueryHandler(ContextESG c) => _context = c;
        public async Task<CommandResult> Handle(ListarRegistrosTsuQuery request, CancellationToken ct)
        {
            var dados = await _context.RegistrosTsu.OrderBy(r => r.Codigo).ToListAsync(ct);
            return CommandResult.Ok("Registros de transporte listados.", dados);
        }
    }

    public class ListarCalculosTsuQueryHandler : IQueryHandler<ListarCalculosTsuQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarCalculosTsuQueryHandler(ContextESG c) => _context = c;
        public async Task<CommandResult> Handle(ListarCalculosTsuQuery request, CancellationToken ct)
        {
            var dados = await _context.CalculosTsu.Where(c => c.TrechoId == request.TrechoId).ToListAsync(ct);
            return CommandResult.Ok("Calculos de transporte listados.", dados);
        }
    }

    public class ListarMetasModalTsuQueryHandler : IQueryHandler<ListarMetasModalTsuQuery, CommandResult>
    {
        private readonly ContextESG _context;
        public ListarMetasModalTsuQueryHandler(ContextESG c) => _context = c;
        public async Task<CommandResult> Handle(ListarMetasModalTsuQuery request, CancellationToken ct)
        {
            var dados = await _context.MetasModalTsu.Where(m => m.RegistroTsuId == request.RegistroTsuId).ToListAsync(ct);
            return CommandResult.Ok("Metas modais listadas.", dados);
        }
    }
}
