using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Modules.Relatorios.Application.Dtos;
using Epros.Modules.Relatorios.Application.Queries;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Relatorios.Application.Handlers
{
    // =====================================================================
    // RPT-ONM — Handlers de BI (KPIs e series). Multi-modulo, read-side.
    // Faturamento reconhecido por competencia (Negocio-acumulado/contabil:
    // "Competencia — o principio-base"): usa a data do fato gerador (DataVenda),
    // nao a data de recebimento. Metrica carrega formula/unidade (RN-BI-002).
    // =====================================================================

    internal static class BiCalculo
    {
        // Formula de margem bruta (fork): custo da linha = ValorCusto (unitario) * Quantidade.
        // A semantica de VendaItem.ValorCusto (unitario x total) nao esta definida na EF;
        // default = unitario. Registrado em DECISOES-PENDENTES-RAFAEL.md.
        public static decimal MargemPercentual(decimal receita, decimal custo)
            => receita > 0 ? Math.Round((receita - custo) / receita * 100m, 4) : 0m;
    }

    // ---- KPI faturamento --------------------------------------------------
    public sealed class KpiFaturamentoQueryHandler : IQueryHandler<KpiFaturamentoQuery, CommandResult>
    {
        private readonly ContextVendas _ctx;
        public KpiFaturamentoQueryHandler(ContextVendas ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(KpiFaturamentoQuery q, CancellationToken ct)
        {
            var total = await _ctx.Vendas.AsNoTracking()
                .Where(v => !v.Cancelada)
                .Where(v => (!q.Inicio.HasValue || v.DataVenda >= q.Inicio.Value)
                         && (!q.Fim.HasValue || v.DataVenda <= q.Fim.Value))
                .SumAsync(v => (decimal?)v.Total, ct) ?? 0m;

            var dto = new KpiValorDto("Faturamento", total, "BRL",
                "SUM(Venda.Total) WHERE nao cancelada (competencia por DataVenda)", q.Inicio, q.Fim);
            return CommandResult.Ok("KPI de faturamento calculado com sucesso.", dto);
        }
    }

    // ---- Serie faturamento ------------------------------------------------
    public sealed class SerieFaturamentoQueryHandler : IQueryHandler<SerieFaturamentoQuery, CommandResult>
    {
        private readonly ContextVendas _ctx;
        public SerieFaturamentoQueryHandler(ContextVendas ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(SerieFaturamentoQuery q, CancellationToken ct)
        {
            var vendas = await _ctx.Vendas.AsNoTracking()
                .Where(v => !v.Cancelada)
                .Where(v => (!q.Inicio.HasValue || v.DataVenda >= q.Inicio.Value)
                         && (!q.Fim.HasValue || v.DataVenda <= q.Fim.Value))
                .Select(v => new { v.DataVenda, v.Total })
                .ToListAsync(ct);

            var mensal = string.Equals(q.Granularidade, "Mes", StringComparison.OrdinalIgnoreCase);
            var pontos = vendas
                .GroupBy(v => mensal ? new DateTime(v.DataVenda.Year, v.DataVenda.Month, 1) : v.DataVenda.Date)
                .Select(g => new SerieTemporalPontoDto(g.Key, g.Sum(x => x.Total)))
                .OrderBy(p => p.Periodo)
                .ToList();

            var dto = new SerieTemporalDto("Faturamento", "BRL", mensal ? "Mes" : "Dia",
                pontos, pontos.Sum(p => p.Valor));
            return CommandResult.Ok("Serie de faturamento calculada com sucesso.", dto);
        }
    }

    // ---- KPI margem -------------------------------------------------------
    public sealed class KpiMargemQueryHandler : IQueryHandler<KpiMargemQuery, CommandResult>
    {
        private readonly ContextVendas _ctx;
        public KpiMargemQueryHandler(ContextVendas ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(KpiMargemQuery q, CancellationToken ct)
        {
            var vendaIds = await _ctx.Vendas.AsNoTracking()
                .Where(v => !v.Cancelada)
                .Where(v => (!q.Inicio.HasValue || v.DataVenda >= q.Inicio.Value)
                         && (!q.Fim.HasValue || v.DataVenda <= q.Fim.Value))
                .Select(v => v.Id).ToListAsync(ct);
            var idset = vendaIds.ToHashSet();

            var itens = await _ctx.VendaItens.AsNoTracking()
                .Select(i => new { i.VendaId, i.ValorTotal, i.ValorCusto, i.Quantidade })
                .ToListAsync(ct);

            var validos = itens.Where(i => idset.Contains(i.VendaId)).ToList();
            var receita = validos.Sum(i => i.ValorTotal);
            var custo = validos.Sum(i => i.ValorCusto * i.Quantidade);
            var dto = new MargemKpiDto(receita, custo, receita - custo,
                BiCalculo.MargemPercentual(receita, custo), q.Inicio, q.Fim);
            return CommandResult.Ok("KPI de margem calculado com sucesso.", dto);
        }
    }

    // ---- KPI inadimplencia ------------------------------------------------
    public sealed class KpiInadimplenciaQueryHandler : IQueryHandler<KpiInadimplenciaQuery, CommandResult>
    {
        private readonly ContextFinanceiro _ctx;
        public KpiInadimplenciaQueryHandler(ContextFinanceiro ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(KpiInadimplenciaQuery q, CancellationToken ct)
        {
            var refDate = (q.DataReferencia ?? DateTime.UtcNow).Date;
            var titulos = await _ctx.ContasAReceberAgregado.AsNoTracking()
                .Where(t => t.Situacao != ESituacao.Cancelado)
                .Select(t => new { t.DataVencimento, Saldo = t.ValorTotalAReceberTitulo })
                .ToListAsync(ct);

            var comSaldo = titulos.Where(t => t.Saldo > 0).ToList();
            var saldoTotal = comSaldo.Sum(t => t.Saldo);
            var saldoVencido = comSaldo.Where(t => t.DataVencimento.Date < refDate).Sum(t => t.Saldo);
            var pct = saldoTotal > 0 ? Math.Round(saldoVencido / saldoTotal * 100m, 4) : 0m;

            var dto = new InadimplenciaKpiDto(saldoTotal, saldoVencido, pct, refDate);
            return CommandResult.Ok("KPI de inadimplencia calculado com sucesso.", dto);
        }
    }

    // ---- KPI ruptura ------------------------------------------------------
    public sealed class KpiRupturaQueryHandler : IQueryHandler<KpiRupturaQuery, CommandResult>
    {
        private readonly ContextEstoque _ctx;
        public KpiRupturaQueryHandler(ContextEstoque ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(KpiRupturaQuery q, CancellationToken ct)
        {
            var estoques = await _ctx.EstoqueProdutos.AsNoTracking()
                .Select(e => new { e.QuantidadeSaldoEstoque, e.QuantidadeEstoqueMinimo })
                .ToListAsync(ct);

            var total = estoques.Count;
            var emRuptura = estoques.Count(e => e.QuantidadeSaldoEstoque <= 0);
            var abaixoMin = estoques.Count(e => e.QuantidadeSaldoEstoque > 0
                                             && e.QuantidadeSaldoEstoque < e.QuantidadeEstoqueMinimo);
            var pct = total > 0 ? Math.Round((decimal)emRuptura / total * 100m, 4) : 0m;

            var dto = new RupturaKpiDto(total, emRuptura, abaixoMin, pct);
            return CommandResult.Ok("KPI de ruptura calculado com sucesso.", dto);
        }
    }

    // ---- Top produtos -----------------------------------------------------
    public sealed class TopProdutosQueryHandler : IQueryHandler<TopProdutosQuery, CommandResult>
    {
        private readonly ContextVendas _ctx;
        public TopProdutosQueryHandler(ContextVendas ctx) => _ctx = ctx;

        public async Task<CommandResult> Handle(TopProdutosQuery q, CancellationToken ct)
        {
            var vendaIds = await _ctx.Vendas.AsNoTracking()
                .Where(v => !v.Cancelada)
                .Where(v => (!q.Inicio.HasValue || v.DataVenda >= q.Inicio.Value)
                         && (!q.Fim.HasValue || v.DataVenda <= q.Fim.Value))
                .Select(v => v.Id).ToListAsync(ct);
            var idset = vendaIds.ToHashSet();

            var itens = await _ctx.VendaItens.AsNoTracking()
                .Select(i => new { i.VendaId, i.ProdutoId, i.DescricaoProduto, i.Quantidade, i.ValorTotal })
                .ToListAsync(ct);

            var topN = q.TopN <= 0 ? 10 : q.TopN;
            var porQuantidade = string.Equals(q.Criterio, "Quantidade", StringComparison.OrdinalIgnoreCase);

            var agrupado = itens.Where(i => idset.Contains(i.VendaId))
                .GroupBy(i => new { i.ProdutoId, i.DescricaoProduto })
                .Select(g => new TopProdutoDto(
                    g.Key.ProdutoId, g.Key.DescricaoProduto ?? string.Empty,
                    g.Sum(x => x.Quantidade), g.Sum(x => x.ValorTotal)));

            agrupado = porQuantidade
                ? agrupado.OrderByDescending(p => p.Quantidade)
                : agrupado.OrderByDescending(p => p.ValorTotal);

            var itensTop = agrupado.Take(topN).ToList();
            var dto = new TopProdutosDto(porQuantidade ? "Quantidade" : "Valor", topN, itensTop);
            return CommandResult.Ok("Top produtos calculado com sucesso.", dto);
        }
    }

    // ---- Painel gerencial consolidado ------------------------------------
    public sealed class PainelGerencialQueryHandler : IQueryHandler<PainelGerencialQuery, CommandResult>
    {
        private readonly ContextVendas _ctxVendas;
        private readonly ContextFinanceiro _ctxFin;
        private readonly ContextEstoque _ctxEst;

        public PainelGerencialQueryHandler(ContextVendas ctxVendas, ContextFinanceiro ctxFin, ContextEstoque ctxEst)
        {
            _ctxVendas = ctxVendas;
            _ctxFin = ctxFin;
            _ctxEst = ctxEst;
        }

        public async Task<CommandResult> Handle(PainelGerencialQuery q, CancellationToken ct)
        {
            var refDate = (q.Fim ?? DateTime.UtcNow).Date;

            // Faturamento + margem (Vendas)
            var vendas = await _ctxVendas.Vendas.AsNoTracking()
                .Where(v => !v.Cancelada)
                .Where(v => (!q.Inicio.HasValue || v.DataVenda >= q.Inicio.Value)
                         && (!q.Fim.HasValue || v.DataVenda <= q.Fim.Value))
                .Select(v => new { v.Id, v.Total }).ToListAsync(ct);
            var idset = vendas.Select(v => v.Id).ToHashSet();
            var faturamento = vendas.Sum(v => v.Total);

            var itens = await _ctxVendas.VendaItens.AsNoTracking()
                .Select(i => new { i.VendaId, i.ValorTotal, i.ValorCusto, i.Quantidade }).ToListAsync(ct);
            var validos = itens.Where(i => idset.Contains(i.VendaId)).ToList();
            var receita = validos.Sum(i => i.ValorTotal);
            var custo = validos.Sum(i => i.ValorCusto * i.Quantidade);
            var margem = new MargemKpiDto(receita, custo, receita - custo,
                BiCalculo.MargemPercentual(receita, custo), q.Inicio, q.Fim);

            // Inadimplencia (Financeiro)
            var titulos = await _ctxFin.ContasAReceberAgregado.AsNoTracking()
                .Where(t => t.Situacao != ESituacao.Cancelado)
                .Select(t => new { t.DataVencimento, Saldo = t.ValorTotalAReceberTitulo })
                .ToListAsync(ct);
            var comSaldo = titulos.Where(t => t.Saldo > 0).ToList();
            var saldoTotal = comSaldo.Sum(t => t.Saldo);
            var saldoVencido = comSaldo.Where(t => t.DataVencimento.Date < refDate).Sum(t => t.Saldo);
            var pctInad = saldoTotal > 0 ? Math.Round(saldoVencido / saldoTotal * 100m, 4) : 0m;
            var inad = new InadimplenciaKpiDto(saldoTotal, saldoVencido, pctInad, refDate);

            // Fluxo/caixa liquido no periodo (Financeiro)
            var movs = await _ctxFin.MovimentosFinanceiros.AsNoTracking()
                .Where(m => !m.Planejamento)
                .Where(m => (!q.Inicio.HasValue || m.Emissao >= q.Inicio.Value)
                         && (!q.Fim.HasValue || m.Emissao <= q.Fim.Value))
                .Select(m => new { m.Credito, m.Debito }).ToListAsync(ct);
            var saldoCaixa = movs.Sum(m => m.Credito) - movs.Sum(m => m.Debito);

            // Ruptura (Estoque)
            var estoques = await _ctxEst.EstoqueProdutos.AsNoTracking()
                .Select(e => new { e.QuantidadeSaldoEstoque, e.QuantidadeEstoqueMinimo }).ToListAsync(ct);
            var total = estoques.Count;
            var emRuptura = estoques.Count(e => e.QuantidadeSaldoEstoque <= 0);
            var abaixoMin = estoques.Count(e => e.QuantidadeSaldoEstoque > 0
                                             && e.QuantidadeSaldoEstoque < e.QuantidadeEstoqueMinimo);
            var pctRup = total > 0 ? Math.Round((decimal)emRuptura / total * 100m, 4) : 0m;
            var ruptura = new RupturaKpiDto(total, emRuptura, abaixoMin, pctRup);

            var dto = new PainelGerencialDto(faturamento, margem, inad, ruptura, saldoCaixa, q.Inicio, q.Fim);
            return CommandResult.Ok("Painel gerencial consolidado com sucesso.", dto);
        }
    }
}
