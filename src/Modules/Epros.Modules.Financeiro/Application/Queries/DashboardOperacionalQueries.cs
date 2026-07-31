using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    // ─────────────────────────────────────────────────────
    // DTOs do painel operacional do TENANT (submódulo 1.03 DASHBOARD_E_LAYOUT).
    // Compõe indicadores a partir dos agregados fiéis do Financeiro
    // (ContasAReceber / ContasAPagar) — sem modelo de "LedgerId" novo.
    // ─────────────────────────────────────────────────────

    /// <summary>Ponto da série mensal (últimos 6 meses): receita recebida x despesa paga no mês.</summary>
    public record DashboardOperacionalMesDto(int Ano, int Mes, string Rotulo, decimal Receita, decimal Despesa);

    /// <summary>Transação recente do painel (título a receber = Receita, título a pagar = Despesa).</summary>
    public record DashboardOperacionalTransacaoDto(DateTime Data, string Descricao, decimal Valor, string Tipo);

    /// <summary>Payload do painel operacional financeiro do tenant.</summary>
    public record DashboardOperacionalDto(
        decimal TotalAReceber,
        decimal TotalAPagar,
        decimal ReceitaMes,
        decimal DespesaMes,
        decimal Saldo,
        IReadOnlyList<DashboardOperacionalMesDto> SerieMensal,
        IReadOnlyList<DashboardOperacionalTransacaoDto> UltimasTransacoes
    );

    // ─────────────────────────────────────────────────────
    // QUERY: Dashboard operacional do tenant
    // - TotalAReceber / TotalAPagar: saldo em aberto (títulos não cancelados nem quitados).
    // - ReceitaMes / DespesaMes: valores efetivamente baixados (Pago) no mês corrente.
    // - Saldo: ReceitaMes - DespesaMes (resultado de caixa do mês).
    // - SerieMensal: últimos 6 meses (recebido x pago por mês da baixa).
    // - UltimasTransacoes: 6 títulos mais recentes por data de emissão (CR+CP).
    // TenantId/RLS aplicados automaticamente pelo QueryFilter do ContextBase.
    // ─────────────────────────────────────────────────────
    public record ObterDashboardOperacionalQuery() : IRequest<CommandResult>;

    public class ObterDashboardOperacionalQueryHandler : IRequestHandler<ObterDashboardOperacionalQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public ObterDashboardOperacionalQueryHandler(ContextFinanceiro context) => _context = context;

        private static readonly string[] MesesAbrev =
            { "jan", "fev", "mar", "abr", "mai", "jun", "jul", "ago", "set", "out", "nov", "dez" };

        private static string Descrever(string? detalhamento, string? nomePessoa, string? documento)
        {
            if (!string.IsNullOrWhiteSpace(detalhamento)) return detalhamento!;
            if (!string.IsNullOrWhiteSpace(nomePessoa)) return nomePessoa!;
            if (!string.IsNullOrWhiteSpace(documento)) return documento!;
            return "Sem descrição";
        }

        public async Task<CommandResult> Handle(ObterDashboardOperacionalQuery request, CancellationToken cancellationToken)
        {
            var hoje = DateTime.UtcNow.Date;
            var inicioMesAtual = new DateTime(hoje.Year, hoje.Month, 1);
            var inicioJanela = inicioMesAtual.AddMonths(-5); // 6 meses incluindo o corrente
            var fimMesAtual = inicioMesAtual.AddMonths(1);    // exclusivo (1º dia do mês seguinte)

            // ----- Saldos em aberto -----
            var totalAReceber = await _context.ContasAReceberAgregado
                .AsNoTracking()
                .Where(c => c.Situacao != ESituacao.Cancelado && c.Situacao != ESituacao.Pago)
                .SumAsync(c => (decimal?)c.ValorTotalAReceberTitulo, cancellationToken) ?? 0m;

            var totalAPagar = await _context.ContasAPagarAgregado
                .AsNoTracking()
                .Where(c => c.Situacao != ESituacao.Cancelado && c.Situacao != ESituacao.Pago)
                .SumAsync(c => (decimal?)c.ValorTotalAPagarTitulo, cancellationToken) ?? 0m;

            // ----- Receita / despesa efetivadas no mês corrente (por data de baixa) -----
            var receitaMes = await _context.ContasAReceberAgregado
                .AsNoTracking()
                .Where(c => c.Situacao == ESituacao.Pago
                         && c.DataBaixa >= inicioMesAtual && c.DataBaixa < fimMesAtual)
                .SumAsync(c => (decimal?)c.ValorTotalRecebido, cancellationToken) ?? 0m;

            var despesaMes = await _context.ContasAPagarAgregado
                .AsNoTracking()
                .Where(c => c.Situacao == ESituacao.Pago
                         && c.DataBaixa >= inicioMesAtual && c.DataBaixa < fimMesAtual)
                .SumAsync(c => (decimal?)c.ValorTotalPago, cancellationToken) ?? 0m;

            // ----- Série mensal (últimos 6 meses) — agregação em memória sobre janela reduzida -----
            var recebidos = await _context.ContasAReceberAgregado
                .AsNoTracking()
                .Where(c => c.Situacao == ESituacao.Pago
                         && c.DataBaixa >= inicioJanela && c.DataBaixa < fimMesAtual)
                .Select(c => new { Data = c.DataBaixa!.Value, Valor = c.ValorTotalRecebido })
                .ToListAsync(cancellationToken);

            var pagos = await _context.ContasAPagarAgregado
                .AsNoTracking()
                .Where(c => c.Situacao == ESituacao.Pago
                         && c.DataBaixa >= inicioJanela && c.DataBaixa < fimMesAtual)
                .Select(c => new { Data = c.DataBaixa!.Value, Valor = c.ValorTotalPago })
                .ToListAsync(cancellationToken);

            var serie = new List<DashboardOperacionalMesDto>(6);
            for (var i = 0; i < 6; i++)
            {
                var mesRef = inicioJanela.AddMonths(i);
                var receita = recebidos
                    .Where(r => r.Data.Year == mesRef.Year && r.Data.Month == mesRef.Month)
                    .Sum(r => r.Valor);
                var despesa = pagos
                    .Where(p => p.Data.Year == mesRef.Year && p.Data.Month == mesRef.Month)
                    .Sum(p => p.Valor);
                serie.Add(new DashboardOperacionalMesDto(
                    mesRef.Year, mesRef.Month,
                    $"{MesesAbrev[mesRef.Month - 1]}/{mesRef:yy}",
                    receita, despesa));
            }

            // ----- Últimas transações (6 títulos mais recentes por emissão, CR + CP) -----
            var ultimasReceber = await _context.ContasAReceberAgregado
                .AsNoTracking()
                .Where(c => c.Situacao != ESituacao.Cancelado)
                .OrderByDescending(c => c.DataEmissao)
                .Take(6)
                .Select(c => new { c.DataEmissao, c.Detalhamento, c.NomePessoa, c.Documento, Valor = c.ValorTitulo })
                .ToListAsync(cancellationToken);

            var ultimasPagar = await _context.ContasAPagarAgregado
                .AsNoTracking()
                .Where(c => c.Situacao != ESituacao.Cancelado)
                .OrderByDescending(c => c.DataEmissao)
                .Take(6)
                .Select(c => new { c.DataEmissao, c.Detalhamento, c.NomePessoa, c.Documento, Valor = c.ValorTitulo })
                .ToListAsync(cancellationToken);

            var transacoes = ultimasReceber
                .Select(c => new DashboardOperacionalTransacaoDto(
                    c.DataEmissao, Descrever(c.Detalhamento, c.NomePessoa, c.Documento), c.Valor, "Receita"))
                .Concat(ultimasPagar.Select(c => new DashboardOperacionalTransacaoDto(
                    c.DataEmissao, Descrever(c.Detalhamento, c.NomePessoa, c.Documento), c.Valor, "Despesa")))
                .OrderByDescending(t => t.Data)
                .Take(6)
                .ToList();

            var dto = new DashboardOperacionalDto(
                TotalAReceber: totalAReceber,
                TotalAPagar: totalAPagar,
                ReceitaMes: receitaMes,
                DespesaMes: despesaMes,
                Saldo: receitaMes - despesaMes,
                SerieMensal: serie,
                UltimasTransacoes: transacoes);

            return CommandResult.Ok("OK", dto);
        }
    }
}
