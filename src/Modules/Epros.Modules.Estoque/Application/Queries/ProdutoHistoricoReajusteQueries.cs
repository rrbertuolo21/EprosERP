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
    /// <summary>
    /// Listagem paginada do histórico de reajuste de preço de produtos.
    /// Porte fiel do legado <c>ProdutoHistoricoReajusteController</c> (GET base, filtros
    /// localizar / dataAlteracao / ativo). Ordena por SequenciaExibicao (SequenciaTenantId no legado).
    /// </summary>
    public record ListarProdutosHistoricosReajustesQuery(
        string? Localizar = null,
        DateTime? DataAlteracao = null,
        bool Ativo = true,
        int Pagina = 1,
        int TamanhoPagina = 25
    ) : IQuery<CommandResult>;

    public class ListarProdutosHistoricosReajustesQueryHandler : IRequestHandler<ListarProdutosHistoricosReajustesQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarProdutosHistoricosReajustesQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarProdutosHistoricosReajustesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ProdutoHistoricoReajustes
                .AsNoTracking()
                .Include(h => h.Produto)
                .Where(h => h.DeletadoEm == null)
                .AsQueryable();

            if (request.Ativo)
            {
                query = query.Where(h => h.Produto != null && h.Produto.Ativo);
            }

            if (!string.IsNullOrWhiteSpace(request.Localizar))
            {
                var termo = request.Localizar;
                query = query.Where(h => h.Produto != null && (
                    h.Produto.Codigo == termo ||
                    h.Produto.Descricao.Contains(termo) ||
                    h.Produto.Ean == termo));
            }

            if (request.DataAlteracao.HasValue)
            {
                var data = request.DataAlteracao.Value;
                query = query.Where(h => h.AlteradoEm >= data || h.CriadoEm >= data);
            }

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(h => h.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(h => new
                {
                    h.Id,
                    h.ProdutoId,
                    h.CodigoProduto,
                    ProdutoDescricao = h.Produto != null ? h.Produto.Descricao : null,
                    h.ValorAntigo,
                    Tipo = (int)h.Tipo,
                    h.Fator,
                    h.ValorFixo,
                    h.ValorNovo,
                    h.Motivo,
                    Data = h.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    /// <summary>
    /// Histórico de reajuste de um produto específico (legado <c>localizar-por-id-produto/{idProduto}</c>).
    /// </summary>
    public record ListarProdutosHistoricosReajustesPorProdutoQuery(Guid ProdutoId) : IQuery<CommandResult>;

    public class ListarProdutosHistoricosReajustesPorProdutoQueryHandler : IRequestHandler<ListarProdutosHistoricosReajustesPorProdutoQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ListarProdutosHistoricosReajustesPorProdutoQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarProdutosHistoricosReajustesPorProdutoQuery request, CancellationToken cancellationToken)
        {
            var itens = await _context.ProdutoHistoricoReajustes
                .AsNoTracking()
                .Where(h => h.DeletadoEm == null && h.ProdutoId == request.ProdutoId)
                .OrderBy(h => h.CriadoEm)
                .Select(h => new
                {
                    h.Id,
                    h.ProdutoId,
                    h.CodigoProduto,
                    h.ValorAntigo,
                    Tipo = (int)h.Tipo,
                    h.Fator,
                    h.ValorFixo,
                    h.ValorNovo,
                    h.Motivo,
                    Data = h.CriadoEm
                })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = itens.Count, Itens = itens });
        }
    }

    /// <summary>Obtém um registro de histórico de reajuste por Id.</summary>
    public record ObterProdutoHistoricoReajustePorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ObterProdutoHistoricoReajustePorIdQueryHandler : IRequestHandler<ObterProdutoHistoricoReajustePorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;

        public ObterProdutoHistoricoReajustePorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterProdutoHistoricoReajustePorIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _context.ProdutoHistoricoReajustes
                .AsNoTracking()
                .Where(h => h.DeletadoEm == null && h.Id == request.Id)
                .Select(h => new
                {
                    h.Id,
                    h.ProdutoId,
                    h.CodigoProduto,
                    h.ValorAntigo,
                    Tipo = (int)h.Tipo,
                    h.Fator,
                    h.ValorFixo,
                    h.ValorNovo,
                    h.Motivo,
                    Data = h.CriadoEm
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (item == null) return CommandResult.Falha("Histórico de reajuste não encontrado.");
            return CommandResult.Ok("OK", item);
        }
    }
}
