using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    public record ListarProdutoGruposQuery(string? Localizar = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterProdutoGrupoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarProdutoGruposQueryHandler : IRequestHandler<ListarProdutoGruposQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarProdutoGruposQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarProdutoGruposQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ProdutoGrupos.AsNoTracking().Where(g => g.DeletadoEm == null).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Localizar))
                query = query.Where(g => g.Descricao.Contains(request.Localizar));

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(g => g.Descricao)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(g => new { g.Id, g.Descricao })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterProdutoGrupoPorIdQueryHandler : IRequestHandler<ObterProdutoGrupoPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterProdutoGrupoPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterProdutoGrupoPorIdQuery request, CancellationToken cancellationToken)
        {
            var g = await _context.ProdutoGrupos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (g == null)
                return CommandResult.Falha("Grupo de produtos não encontrado.");

            return CommandResult.Ok("OK", new { g.Id, g.Descricao });
        }
    }
}
