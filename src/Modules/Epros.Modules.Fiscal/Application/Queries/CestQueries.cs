using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Fiscal.Infrastructure.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Fiscal.Application.Queries
{
    public record ListarCestsQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterCestPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarCestsQueryHandler : IRequestHandler<ListarCestsQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarCestsQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarCestsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Cests.AsNoTracking().Where(c => c.DeletadoEm == null).AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(c => c.Codigo.Contains(request.Localizar) || c.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderBy(c => c.Codigo)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.Codigo, c.Descricao })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterCestPorIdQueryHandler : IRequestHandler<ObterCestPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterCestPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterCestPorIdQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.Cests.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (c == null) return CommandResult.Falha("CEST não encontrado.");
            return CommandResult.Ok("OK", new { c.Id, c.Codigo, c.Descricao });
        }
    }
}
