using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    public record ListarMoedasQuery(bool? ApenasAtivas, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarTaxasCambioQuery(Guid? MoedaId, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarExposicoesCambiaisQuery(Guid? MoedaId, int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ListarReavaliacoesTituloQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterReavaliacaoTituloPorIdQuery(Guid Id) : IRequest<CommandResult>;

    public class CambioRiscoQueryHandlers :
        IRequestHandler<ListarMoedasQuery, CommandResult>,
        IRequestHandler<ListarTaxasCambioQuery, CommandResult>,
        IRequestHandler<ListarExposicoesCambiaisQuery, CommandResult>,
        IRequestHandler<ListarReavaliacoesTituloQuery, CommandResult>,
        IRequestHandler<ObterReavaliacaoTituloPorIdQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public CambioRiscoQueryHandlers(ContextFinanceiro context) => _context = context;

        private static (int pagina, int tamanho) Normaliza(int pagina, int tamanho)
            => (pagina <= 0 ? 1 : pagina, tamanho is <= 0 or > 100 ? 20 : tamanho);

        public async Task<CommandResult> Handle(ListarMoedasQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.Moedas.AsNoTracking().AsQueryable();
            if (request.ApenasAtivas == true) query = query.Where(m => m.Ativo);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderBy(m => m.CodigoIso).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(m => new { m.Id, m.CodigoIso, m.Simbolo, m.Nome, m.Ativo }).ToListAsync(ct);
            return CommandResult.Ok("Moedas listadas.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarTaxasCambioQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.TaxasCambio.AsNoTracking().AsQueryable();
            if (request.MoedaId.HasValue) query = query.Where(t => t.MoedaId == request.MoedaId.Value);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(t => t.DataTaxa).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(t => new { t.Id, t.MoedaId, t.DataTaxa, t.TaxaCompra, t.TaxaVenda, t.OrigemTaxa }).ToListAsync(ct);
            return CommandResult.Ok("Taxas de câmbio listadas.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarExposicoesCambiaisQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.ExposicoesCambiais.AsNoTracking().AsQueryable();
            if (request.MoedaId.HasValue) query = query.Where(e => e.MoedaId == request.MoedaId.Value);
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(e => e.DataReferencia).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(e => new { e.Id, e.MoedaId, e.ValorExposto, e.Status, e.DataReferencia, e.ValorMoedaBase }).ToListAsync(ct);
            return CommandResult.Ok("Exposições cambiais listadas.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ListarReavaliacoesTituloQuery request, CancellationToken ct)
        {
            var (pagina, tamanho) = Normaliza(request.Pagina, request.TamanhoPagina);
            var query = _context.ReavaliacoesTitulo.AsNoTracking();
            var total = await query.CountAsync(ct);
            var itens = await query.OrderByDescending(r => r.DataReavaliacao).Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(r => new { r.Id, r.DataReavaliacao, r.Status, r.TotalValorOriginal, r.TotalValorReavaliado, r.TotalVariacao }).ToListAsync(ct);
            return CommandResult.Ok("Reavaliações cambiais listadas.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterReavaliacaoTituloPorIdQuery request, CancellationToken ct)
        {
            var reav = await _context.ReavaliacoesTitulo.AsNoTracking().Include(r => r.Itens)
                .FirstOrDefaultAsync(r => r.Id == request.Id, ct);
            return reav == null ? CommandResult.Falha("Reavaliação não encontrada.") : CommandResult.Ok("Reavaliação encontrada.", reav);
        }
    }
}
