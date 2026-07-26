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
    public record ListarProdutosQuery(
        string? Localizar = null,
        bool Ativo = true,
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterProdutoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarProdutosQueryHandler : IRequestHandler<ListarProdutosQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarProdutosQueryHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ListarProdutosQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Produtos.AsNoTracking().Where(p => p.DeletadoEm == null).AsQueryable();

            if (request.Ativo)
            {
                query = query.Where(p => p.Ativo);
            }

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                query = query.Where(p => p.Nome.Contains(request.Localizar) || p.Sku.Contains(request.Localizar));
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(p => p.Nome)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(p => new
                {
                    p.Id,
                    p.Sku,
                    p.Nome,
                    p.PrecoVenda,
                    p.SaldoEstoque,
                    p.CustoMedio,
                    p.CategoriaId,
                    p.MarcaProdutoId,
                    p.UnidadeMedidaComercialId,
                    p.Ean,
                    p.PesoLiquido,
                    p.PesoBruto,
                    p.ValorVendaPrazo,
                    p.ValorCompra,
                    p.TipoProduto,
                    p.Ativo,
                    p.Imagem,
                    p.CestId,
                    p.CodigoAnpId,
                    p.BalancaId,
                    p.UtilizaBalanca,
                    p.CodigoProdutoBalanca,
                    p.NcmId
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterProdutoPorIdQueryHandler : IRequestHandler<ObterProdutoPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterProdutoPorIdQueryHandler(ContextEstoque context)
        {
            _context = context;
        }

        public async Task<CommandResult> Handle(ObterProdutoPorIdQuery request, CancellationToken cancellationToken)
        {
            var p = await _context.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.Id && p.DeletadoEm == null, cancellationToken);
            if (p == null)
            {
                return CommandResult.Falha("Produto não encontrado.");
            }

            var dto = new
            {
                p.Id,
                p.Sku,
                p.Nome,
                p.PrecoVenda,
                p.SaldoEstoque,
                p.CustoMedio,
                p.CategoriaId,
                p.MarcaProdutoId,
                p.UnidadeMedidaComercialId,
                p.Ean,
                p.PesoLiquido,
                p.PesoBruto,
                p.ValorVendaPrazo,
                p.ValorCompra,
                p.TipoProduto,
                p.Ativo,
                p.Imagem,
                p.CestId,
                p.CodigoAnpId,
                p.BalancaId,
                p.UtilizaBalanca,
                p.CodigoProdutoBalanca,
                p.NcmId
            };

            return CommandResult.Ok("OK", dto);
        }
    }
}
