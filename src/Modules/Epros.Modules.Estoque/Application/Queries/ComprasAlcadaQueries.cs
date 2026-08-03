using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MediatR;
using Epros.Shared.Application.Models;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Modules.Estoque.Application.Queries
{
    /// <summary>
    /// Consultas da alçada de aprovação de compras (CD3). Tenant aplicado pelo filtro global.
    /// </summary>
    public record ListarComprasAlcadaRegrasQuery(int? Nivel = null, bool? Ativo = null, int Pagina = 1, int TamanhoPagina = 50) : IRequest<CommandResult>;

    public record ListarPedidosAprovacaoCompraQuery(EStatusPedidoAprovacaoCompra? Status = null, EOrigemAprovacaoCompra? OrigemTipo = null, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;

    public record ObterPedidoAprovacaoCompraPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class ListarComprasAlcadaRegrasQueryHandler : IRequestHandler<ListarComprasAlcadaRegrasQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarComprasAlcadaRegrasQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarComprasAlcadaRegrasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ComprasAlcadaRegras.AsNoTracking().Where(r => r.DeletadoEm == null).AsQueryable();
            if (request.Nivel.HasValue)
                query = query.Where(r => r.Nivel == request.Nivel.Value);
            if (request.Ativo.HasValue)
                query = query.Where(r => r.Ativo == request.Ativo.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderBy(r => r.Nivel).ThenBy(r => r.ValorMinimo)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(r => new { r.Id, r.Nivel, r.ValorMinimo, r.ValorMaximo, r.CompradorId, r.CategoriaCompra, r.AprovadorId, r.PapelAprovador, r.Ativo })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ListarPedidosAprovacaoCompraQueryHandler : IRequestHandler<ListarPedidosAprovacaoCompraQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ListarPedidosAprovacaoCompraQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ListarPedidosAprovacaoCompraQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ComprasPedidosAprovacao.AsNoTracking().Where(p => p.DeletadoEm == null).AsQueryable();
            if (request.Status.HasValue)
                query = query.Where(p => p.Status == request.Status.Value);
            if (request.OrigemTipo.HasValue)
                query = query.Where(p => p.OrigemTipo == request.OrigemTipo.Value);

            var total = await query.CountAsync(cancellationToken);
            var itens = await query
                .OrderByDescending(p => p.CriadoEm)
                .Skip((request.Pagina - 1) * request.TamanhoPagina)
                .Take(request.TamanhoPagina)
                .Select(p => new { p.Id, p.OrigemTipo, p.OrigemId, p.ValorTotal, p.CompradorId, p.CategoriaCompra, p.Status, p.NivelAtual, p.QuantidadeNiveis, p.CriadoEm, p.DecididoEm })
                .ToListAsync(cancellationToken);

            return CommandResult.Ok("OK", new { Total = total, Pagina = request.Pagina, Itens = itens });
        }
    }

    public class ObterPedidoAprovacaoCompraPorIdQueryHandler : IRequestHandler<ObterPedidoAprovacaoCompraPorIdQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public ObterPedidoAprovacaoCompraPorIdQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(ObterPedidoAprovacaoCompraPorIdQuery request, CancellationToken cancellationToken)
        {
            var p = await _context.ComprasPedidosAprovacao.AsNoTracking()
                .Include(x => x.Niveis)
                .FirstOrDefaultAsync(x => x.Id == request.Id && x.DeletadoEm == null, cancellationToken);
            if (p == null)
                return CommandResult.Falha("Pedido de aprovação não encontrado.");

            return CommandResult.Ok("OK", new
            {
                p.Id,
                p.OrigemTipo,
                p.OrigemId,
                p.ValorTotal,
                p.CompradorId,
                p.CategoriaCompra,
                p.Status,
                p.NivelAtual,
                p.QuantidadeNiveis,
                p.CriadoEm,
                p.DecididoEm,
                Niveis = p.Niveis.OrderBy(n => n.Nivel).Select(n => new { n.Id, n.Nivel, n.AprovadorId, n.PapelAprovador, n.ValorMinimo, n.ValorMaximo, n.Status, n.DecididoPor, n.DecididoEm, n.Justificativa })
            });
        }
    }
}
