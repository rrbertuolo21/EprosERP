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
    // ============ REQUISIÇÃO ============

    public record ListarScRequisicoesQuery(Guid? ColaboradorId = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterScRequisicaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarScRequisicoesQueryHandler : IRequestHandler<ListarScRequisicoesQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarScRequisicoesQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarScRequisicoesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ScRequisicoes.AsNoTracking().Where(r => r.DeletadoEm == null);
            if (request.ColaboradorId.HasValue)
                query = query.Where(r => r.ColaboradorId == request.ColaboradorId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(r => r.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(r => new { r.Id, r.TipoRequisicaoId, r.ColaboradorId, r.DataRequisicao, r.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class ObterScRequisicaoPorIdQueryHandler : IRequestHandler<ObterScRequisicaoPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterScRequisicaoPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterScRequisicaoPorIdQuery request, CancellationToken cancellationToken)
        {
            // SC-058: seleção de requisição carrega seus itens.
            var r = await _context.ScRequisicoes.AsNoTracking()
                .Include(x => x.Itens)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (r == null)
                return CommandResult.Falha("Requisição não encontrada.");

            return CommandResult.Ok("OK", new
            {
                r.Id,
                r.TipoRequisicaoId,
                r.ColaboradorId,
                r.DataRequisicao,
                Itens = r.Itens.Where(i => i.DeletadoEm == null).Select(i => new { i.Id, i.ProdutoId, i.Quantidade, i.QuantidadeCotada, i.ItemCotado })
            });
        }
    }

    // ============ COTAÇÃO ============

    public record ListarScCotacoesQuery(int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterScCotacaoPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarScCotacoesQueryHandler : IRequestHandler<ListarScCotacoesQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarScCotacoesQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarScCotacoesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ScCotacoes.AsNoTracking().Where(c => c.DeletadoEm == null);
            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(c => c.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(c => new { c.Id, c.Descricao, c.Situacao, c.DataCotacao, c.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class ObterScCotacaoPorIdQueryHandler : IRequestHandler<ObterScCotacaoPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterScCotacaoPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterScCotacaoPorIdQuery request, CancellationToken cancellationToken)
        {
            var c = await _context.ScCotacoes.AsNoTracking()
                .Include(x => x.Fornecedores)
                .Include(x => x.Itens)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (c == null)
                return CommandResult.Falha("Cotação não encontrada.");

            return CommandResult.Ok("OK", new
            {
                c.Id,
                c.Descricao,
                c.Situacao,
                c.DataCotacao,
                Fornecedores = c.Fornecedores.Where(f => f.DeletadoEm == null).Select(f => new { f.Id, f.FornecedorId, f.PrazoEntrega, f.CondicoesPagamento, f.Subtotal, f.Desconto, f.Total }),
                Itens = c.Itens.Where(i => i.DeletadoEm == null).Select(i => new { i.Id, i.CotacaoFornecedorId, i.ProdutoId, i.Quantidade, i.ValorUnitario, i.ValorTotal })
            });
        }
    }

    // ============ PEDIDO DE COMPRA ============

    public record ListarScPedidosCompraQuery(Guid? FornecedorId = null, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
    public record ObterScPedidoCompraPorIdQuery(Guid Id) : IQuery<CommandResult>;

    public class ListarScPedidosCompraQueryHandler : IRequestHandler<ListarScPedidosCompraQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarScPedidosCompraQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarScPedidosCompraQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ScPedidosCompra.AsNoTracking().Where(p => p.DeletadoEm == null);
            if (request.FornecedorId.HasValue)
                query = query.Where(p => p.FornecedorId == request.FornecedorId.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(p => p.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(p => new { p.Id, p.TipoPedidoId, p.FornecedorId, p.CotacaoId, p.DataPedido, p.DataPrevistaEntrega, p.CriadoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, request.Pagina, Itens = itens });
        }
    }

    public class ObterScPedidoCompraPorIdQueryHandler : IRequestHandler<ObterScPedidoCompraPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterScPedidoCompraPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterScPedidoCompraPorIdQuery request, CancellationToken cancellationToken)
        {
            var p = await _context.ScPedidosCompra.AsNoTracking()
                .Include(x => x.Itens)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (p == null)
                return CommandResult.Falha("Pedido de compra não encontrado.");

            return CommandResult.Ok("OK", new
            {
                p.Id,
                p.TipoPedidoId,
                p.FornecedorId,
                p.CotacaoId,
                p.DataPedido,
                p.DataPrevistaEntrega,
                p.DataPrevisaoPagamento,
                p.LocalEntrega,
                p.LocalCobranca,
                p.Contato,
                p.FormaPagamento,
                Itens = p.Itens.Where(i => i.DeletadoEm == null).Select(i => new { i.Id, i.ProdutoId, i.Quantidade, i.ValorUnitario, i.ValorTotal })
            });
        }
    }
}
