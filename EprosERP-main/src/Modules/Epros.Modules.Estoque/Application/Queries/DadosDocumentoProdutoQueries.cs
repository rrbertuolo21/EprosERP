using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Dados auxiliares de produtos para as telas de Venda (<c>venda-dados</c>) e Compra
    /// (<c>compras-dados</c>): retorna os produtos por lista de Ids com os campos usados na tela.
    /// Read-model do módulo dono (Estoque).
    /// </summary>
    public record ObterDadosProdutosPorIdsQuery(IReadOnlyCollection<Guid> IdsProdutos) : IQuery<CommandResult>;

    public class ObterDadosProdutosPorIdsQueryHandler : IRequestHandler<ObterDadosProdutosPorIdsQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterDadosProdutosPorIdsQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterDadosProdutosPorIdsQuery request, CancellationToken cancellationToken)
        {
            if (request.IdsProdutos == null || request.IdsProdutos.Count == 0)
                return CommandResult.Falha("Informe ao menos um produto.");

            var ids = request.IdsProdutos.Distinct().ToList();

            var itens = await _context.Produtos
                .AsNoTracking()
                .Where(p => p.DeletadoEm == null && ids.Contains(p.Id))
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    p.Descricao,
                    p.Ean,
                    p.UnidadeMedidaComercialId,
                    ValorUnitario = p.ValorVenda,
                    p.ValorCompra,
                    QuantidadeSaldoEstoque = p.SaldoEstoque,
                    p.NcmId,
                    p.CestId,
                    p.CodigoAnpId
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = itens.Count, Itens = itens });
        }
    }
}
