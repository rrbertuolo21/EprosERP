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
    public record ListarProdutosEspecificosQuery(Guid? ProdutoId = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    public record ObterProdutoEspecificoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarProdutosEspecificosQueryHandler : IRequestHandler<ListarProdutosEspecificosQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarProdutosEspecificosQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarProdutosEspecificosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ProdutosEspecificos.AsNoTracking().Where(p => p.DeletadoEm == null).AsQueryable();

            if (request.ProdutoId.HasValue)
                query = query.Where(p => p.ProdutoId == request.ProdutoId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(p => p.ProdutoId)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(p => new
                {
                    p.Id,
                    p.ProdutoId,
                    p.ValorPercentualGlpDerivadoPetroleo,
                    p.ValorPercentualGasNaturalNacional,
                    p.ValorPercentualGasNaturalImportado,
                    p.ValorPartida,
                    p.UfConsumo
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterProdutoEspecificoPorIdQueryHandler : IRequestHandler<ObterProdutoEspecificoPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterProdutoEspecificoPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterProdutoEspecificoPorIdQuery request, CancellationToken cancellationToken)
        {
            var p = await _context.ProdutosEspecificos
                .AsNoTracking()
                .Include(x => x.Origens)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);

            if (p == null)
                return CommandResult.Falha("Produto específico não encontrado.");

            return CommandResult.Ok("OK", new
            {
                p.Id,
                p.ProdutoId,
                p.ValorPercentualGlpDerivadoPetroleo,
                p.ValorPercentualGasNaturalNacional,
                p.ValorPercentualGasNaturalImportado,
                p.ValorPartida,
                p.UfConsumo,
                Origens = p.Origens.Where(o => o.DeletadoEm == null).Select(o => new
                {
                    o.Id,
                    o.IndicadorImportacao,
                    o.UfOrigem,
                    o.ValorPercentualUf
                })
            });
        }
    }
}
