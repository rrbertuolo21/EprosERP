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
    public record ListarEstoqueProdutosQuery(Guid? EmpresaId = null, Guid? ProdutoId = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterEstoqueProdutoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarEstoqueProdutosQueryHandler : IRequestHandler<ListarEstoqueProdutosQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarEstoqueProdutosQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarEstoqueProdutosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.EstoqueProdutos.AsNoTracking().Where(e => e.DeletadoEm == null).AsQueryable();

            if (request.EmpresaId.HasValue)
                query = query.Where(e => e.EmpresaId == request.EmpresaId.Value);

            if (request.ProdutoId.HasValue)
                query = query.Where(e => e.ProdutoId == request.ProdutoId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(e => e.ProdutoId)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(e => new
                {
                    e.Id,
                    e.EmpresaId,
                    e.ProdutoId,
                    e.QuantidadeSaldoEstoque,
                    e.QuantidadeEstoqueMinimo,
                    e.QuantidadeEstoqueMaximo,
                    e.QuantidadeEstoqueReservado,
                    e.ValorSaldo,
                    e.ValorCustoMedio,
                    e.TipoCusteioEstoque
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterEstoqueProdutoPorIdQueryHandler : IRequestHandler<ObterEstoqueProdutoPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterEstoqueProdutoPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterEstoqueProdutoPorIdQuery request, CancellationToken cancellationToken)
        {
            var e = await _context.EstoqueProdutos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (e == null)
                return CommandResult.Falha("Estoque do produto não encontrado.");

            return CommandResult.Ok("OK", new
            {
                e.Id,
                e.EmpresaId,
                e.ProdutoId,
                e.QuantidadeSaldoEstoque,
                e.QuantidadeEstoqueMinimo,
                e.QuantidadeEstoqueMaximo,
                e.QuantidadeEstoqueReservado,
                e.ValorSaldo,
                e.ValorCustoMedio,
                e.TipoCusteioEstoque
            });
        }
    }
}
