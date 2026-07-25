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
    public record ListarCentrosCustoQuery(int Pagina = 1, int TamanhoPagina = 20) : IRequest<CommandResult>;
    public record ObterCentroCustoPorIdQuery(Guid Id) : IRequest<CommandResult>;
    public record ListarAlocacoesTituloQuery(Guid TituloId) : IRequest<CommandResult>;
    public record ListarDimensoesAnaliticasQuery(string? Tipo) : IRequest<CommandResult>;
    public record ConsultaGerencialPorCentroCustoQuery(Guid CentroCustoId) : IRequest<CommandResult>;

    public class ContabilidadeGerencialQueryHandlers :
        IRequestHandler<ListarCentrosCustoQuery, CommandResult>,
        IRequestHandler<ObterCentroCustoPorIdQuery, CommandResult>,
        IRequestHandler<ListarAlocacoesTituloQuery, CommandResult>,
        IRequestHandler<ListarDimensoesAnaliticasQuery, CommandResult>,
        IRequestHandler<ConsultaGerencialPorCentroCustoQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ContabilidadeGerencialQueryHandlers(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ListarCentrosCustoQuery request, CancellationToken ct)
        {
            var tamanho = request.TamanhoPagina is <= 0 or > 100 ? 20 : request.TamanhoPagina;
            var pagina = request.Pagina <= 0 ? 1 : request.Pagina;
            var query = _context.CentrosCusto.AsNoTracking().OrderBy(c => c.Codigo);
            var total = await query.CountAsync(ct);
            var itens = await query.Skip((pagina - 1) * tamanho).Take(tamanho)
                .Select(c => new { c.Id, c.Codigo, c.Descricao, c.PaiId, c.Estado }).ToListAsync(ct);
            return CommandResult.Ok("Centros de custo listados.", new { total, pagina, tamanho, itens });
        }

        public async Task<CommandResult> Handle(ObterCentroCustoPorIdQuery request, CancellationToken ct)
        {
            var cc = await _context.CentrosCusto.AsNoTracking().FirstOrDefaultAsync(c => c.Id == request.Id, ct);
            return cc == null ? CommandResult.Falha("Centro de custo não encontrado.") : CommandResult.Ok("Centro de custo encontrado.", cc);
        }

        public async Task<CommandResult> Handle(ListarAlocacoesTituloQuery request, CancellationToken ct)
        {
            var itens = await _context.AlocacoesCentroCusto.AsNoTracking().Where(a => a.TituloId == request.TituloId)
                .Select(a => new { a.Id, a.TituloId, a.TipoTitulo, a.CentroCustoId, a.Percentual, a.ValorRateado }).ToListAsync(ct);
            return CommandResult.Ok("Alocações do título listadas.", itens);
        }

        public async Task<CommandResult> Handle(ListarDimensoesAnaliticasQuery request, CancellationToken ct)
        {
            var query = _context.DimensoesAnaliticas.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(request.Tipo)) query = query.Where(d => d.Tipo == request.Tipo);
            var itens = await query.OrderBy(d => d.Tipo).ThenBy(d => d.Valor)
                .Select(d => new { d.Id, d.Tipo, d.Valor }).ToListAsync(ct);
            return CommandResult.Ok("Dimensões analíticas listadas.", itens);
        }

        public async Task<CommandResult> Handle(ConsultaGerencialPorCentroCustoQuery request, CancellationToken ct)
        {
            var total = await _context.AlocacoesCentroCusto.AsNoTracking()
                .Where(a => a.CentroCustoId == request.CentroCustoId)
                .SumAsync(a => (decimal?)a.ValorRateado, ct) ?? 0m;
            var quantidade = await _context.AlocacoesCentroCusto.AsNoTracking()
                .CountAsync(a => a.CentroCustoId == request.CentroCustoId, ct);
            return CommandResult.Ok("Consulta gerencial consolidada.", new { centroCustoId = request.CentroCustoId, totalRateado = total, quantidadeAlocacoes = quantidade });
        }
    }
}
