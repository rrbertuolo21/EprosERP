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
    public record ListarCodigosAnpQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterCodigoAnpPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarCodigosAnpQueryHandler : IRequestHandler<ListarCodigosAnpQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarCodigosAnpQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarCodigosAnpQuery request, CancellationToken cancellationToken)
        {
            var query = _context.CodigosAnp.AsNoTracking().Where(a => a.DeletadoEm == null).AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(a => a.Codigo.Contains(request.Localizar) || a.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderBy(a => a.Codigo)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(a => new { a.Id, a.Codigo, a.Descricao, a.DataInicioVigencia, a.DataFinalVigencia })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterCodigoAnpPorIdQueryHandler : IRequestHandler<ObterCodigoAnpPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterCodigoAnpPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterCodigoAnpPorIdQuery request, CancellationToken cancellationToken)
        {
            var a = await _context.CodigosAnp.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (a == null) return CommandResult.Falha("Código ANP não encontrado.");
            return CommandResult.Ok("OK", new { a.Id, a.Codigo, a.Descricao, a.DataInicioVigencia, a.DataFinalVigencia });
        }
    }
}
