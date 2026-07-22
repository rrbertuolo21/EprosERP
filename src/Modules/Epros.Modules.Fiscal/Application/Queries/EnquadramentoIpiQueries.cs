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
    public record ListarEnquadramentosIpiQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterEnquadramentoIpiPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarEnquadramentosIpiQueryHandler : IRequestHandler<ListarEnquadramentosIpiQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ListarEnquadramentosIpiQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ListarEnquadramentosIpiQuery request, CancellationToken cancellationToken)
        {
            var query = _context.EnquadramentosIpi.AsNoTracking().Where(e => e.DeletadoEm == null).AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(e => e.Codigo.Contains(request.Localizar) || e.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query.OrderBy(e => e.Codigo)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(e => new { e.Id, e.Codigo, e.Descricao, e.TipoOperacao })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterEnquadramentoIpiPorIdQueryHandler : IRequestHandler<ObterEnquadramentoIpiPorIdQuery, CommandResult>
    {
        private readonly ContextFiscal _context;
        public ObterEnquadramentoIpiPorIdQueryHandler(ContextFiscal context) => _context = context;

        public async Task<CommandResult> Handle(ObterEnquadramentoIpiPorIdQuery request, CancellationToken cancellationToken)
        {
            var e = await _context.EnquadramentosIpi.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (e == null) return CommandResult.Falha("Enquadramento IPI não encontrado.");
            return CommandResult.Ok("OK", new { e.Id, e.Codigo, e.Descricao, e.TipoOperacao });
        }
    }
}
