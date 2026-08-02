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
    /// Relatórios de COMPRAS (CD7 — pacote completo): curva ABC de fornecedor, savings de cotação, lead time
    /// de pedido e aderência de alçada. Queries read-only, tenant/soft-delete pelo filtro global. Materializam
    /// e agregam em memória (aritmética estrutural). Base para dashboards/BI.
    /// </summary>

    // ---- CD7.1 Curva ABC de fornecedor ----
    public record CurvaAbcFornecedorQuery(DateTime? DataInicio = null, DateTime? DataFim = null) : IRequest<CommandResult>;

    public class CurvaAbcFornecedorQueryHandler : IRequestHandler<CurvaAbcFornecedorQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public CurvaAbcFornecedorQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(CurvaAbcFornecedorQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Compras.AsNoTracking().Where(c => !c.Cancelada);
            if (request.DataInicio.HasValue) query = query.Where(c => c.DataCompra >= request.DataInicio.Value);
            if (request.DataFim.HasValue) query = query.Where(c => c.DataCompra <= request.DataFim.Value);

            var compras = await query.Select(c => new { c.FornecedorCnpj, c.FornecedorNome, c.ValorTotal }).ToListAsync(cancellationToken);

            var porFornecedor = compras
                .GroupBy(c => new { c.FornecedorCnpj, c.FornecedorNome })
                .Select(g => new { g.Key.FornecedorCnpj, g.Key.FornecedorNome, Total = g.Sum(x => x.ValorTotal), Compras = g.Count() })
                .OrderByDescending(x => x.Total)
                .ToList();

            var totalGeral = porFornecedor.Sum(x => x.Total);
            decimal acumulado = 0m;
            var linhas = porFornecedor.Select(x =>
            {
                acumulado += x.Total;
                var percAcum = totalGeral <= 0m ? 0m : Math.Round(acumulado / totalGeral * 100m, 2, MidpointRounding.AwayFromZero);
                var classe = percAcum <= 80m ? "A" : percAcum <= 95m ? "B" : "C";
                return new
                {
                    x.FornecedorCnpj, x.FornecedorNome, x.Total, x.Compras,
                    ParticipacaoPercent = totalGeral <= 0m ? 0m : Math.Round(x.Total / totalGeral * 100m, 2, MidpointRounding.AwayFromZero),
                    PercentualAcumulado = percAcum,
                    Classe = classe
                };
            }).ToList();

            return CommandResult.Ok("OK", new { TotalGeral = totalGeral, Fornecedores = linhas.Count, Linhas = linhas });
        }
    }

    // ---- CD7.2 Savings de cotação ----
    public record SavingsCotacaoQuery() : IRequest<CommandResult>;

    public class SavingsCotacaoQueryHandler : IRequestHandler<SavingsCotacaoQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public SavingsCotacaoQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(SavingsCotacaoQuery request, CancellationToken cancellationToken)
        {
            var cotacoes = await _context.ScCotacoes.AsNoTracking()
                .Include(c => c.Fornecedores)
                .Where(c => c.FornecedorVencedorId != null)
                .ToListAsync(cancellationToken);

            var linhas = cotacoes.Select(c =>
            {
                var props = c.Fornecedores.Where(f => f.DeletadoEm == null).ToList();
                var vencedor = props.FirstOrDefault(f => f.FornecedorId == c.FornecedorVencedorId);
                var totalVencedor = vencedor?.Total ?? 0m;
                var maisCaro = props.Any() ? props.Max(f => f.Total) : 0m;
                var media = props.Any() ? props.Average(f => f.Total) : 0m;
                return new
                {
                    c.Id, c.Descricao, c.FornecedorVencedorId, c.DecididaEm,
                    Propostas = props.Count,
                    TotalVencedor = totalVencedor,
                    EconomiaVsMaisCaro = Math.Round(maisCaro - totalVencedor, 2, MidpointRounding.AwayFromZero),
                    EconomiaVsMedia = Math.Round(media - totalVencedor, 2, MidpointRounding.AwayFromZero)
                };
            }).ToList();

            return CommandResult.Ok("OK", new
            {
                Cotacoes = linhas.Count,
                SavingsTotalVsMaisCaro = linhas.Sum(l => l.EconomiaVsMaisCaro),
                SavingsTotalVsMedia = Math.Round(linhas.Sum(l => l.EconomiaVsMedia), 2, MidpointRounding.AwayFromZero),
                Linhas = linhas
            });
        }
    }

    // ---- CD7.3 Lead time de pedido ----
    public record LeadTimeComprasQuery(Guid? FornecedorId = null) : IRequest<CommandResult>;

    public class LeadTimeComprasQueryHandler : IRequestHandler<LeadTimeComprasQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public LeadTimeComprasQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(LeadTimeComprasQuery request, CancellationToken cancellationToken)
        {
            var query = _context.ScPedidosCompra.AsNoTracking()
                .Where(p => p.DataPedido != null && p.DataPrevistaEntrega != null);
            if (request.FornecedorId.HasValue) query = query.Where(p => p.FornecedorId == request.FornecedorId.Value);

            var pedidos = await query.Select(p => new { p.FornecedorId, p.DataPedido, p.DataPrevistaEntrega }).ToListAsync(cancellationToken);
            var dias = pedidos.Select(p => (p.DataPrevistaEntrega!.Value.Date - p.DataPedido!.Value.Date).TotalDays).ToList();

            return CommandResult.Ok("OK", new
            {
                Pedidos = dias.Count,
                LeadTimeMedioDias = dias.Any() ? Math.Round((decimal)dias.Average(), 2, MidpointRounding.AwayFromZero) : 0m,
                LeadTimeMinDias = dias.Any() ? (int)dias.Min() : 0,
                LeadTimeMaxDias = dias.Any() ? (int)dias.Max() : 0
            });
        }
    }

    // ---- CD7.4 Aderência de alçada ----
    public record AderenciaAlcadaQuery() : IRequest<CommandResult>;

    public class AderenciaAlcadaQueryHandler : IRequestHandler<AderenciaAlcadaQuery, CommandResult>
    {
        private readonly ContextEstoque _context;
        public AderenciaAlcadaQueryHandler(ContextEstoque context) => _context = context;

        public async Task<CommandResult> Handle(AderenciaAlcadaQuery request, CancellationToken cancellationToken)
        {
            var pedidos = await _context.ComprasPedidosAprovacao.AsNoTracking()
                .Select(p => new { p.Status, p.QuantidadeNiveis }).ToListAsync(cancellationToken);

            var total = pedidos.Count;
            var aprovados = pedidos.Count(p => p.Status == EStatusPedidoAprovacaoCompra.Aprovado);
            var reprovados = pedidos.Count(p => p.Status == EStatusPedidoAprovacaoCompra.Reprovado);
            var pendentes = pedidos.Count(p => p.Status == EStatusPedidoAprovacaoCompra.Pendente);
            var cancelados = pedidos.Count(p => p.Status == EStatusPedidoAprovacaoCompra.Cancelado);
            var decididos = aprovados + reprovados;
            // Multi-nível efetivamente exercido (alçada com >1 nível) entre os aprovados.
            var comAlcadaMultinivel = pedidos.Count(p => p.Status == EStatusPedidoAprovacaoCompra.Aprovado && p.QuantidadeNiveis > 1);

            return CommandResult.Ok("OK", new
            {
                Total = total, Aprovados = aprovados, Reprovados = reprovados, Pendentes = pendentes, Cancelados = cancelados,
                TaxaAprovacaoPercent = decididos == 0 ? 0m : Math.Round((decimal)aprovados / decididos * 100m, 2, MidpointRounding.AwayFromZero),
                AprovadosComAlcadaMultinivel = comAlcadaMultinivel
            });
        }
    }
}
