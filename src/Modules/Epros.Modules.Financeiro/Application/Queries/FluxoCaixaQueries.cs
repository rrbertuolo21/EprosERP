using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Financeiro.Domain.Services;
using Epros.Modules.Financeiro.Infrastructure.Data;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Financeiro.Application.Queries
{
    /// <summary>
    /// Projeção de fluxo de caixa (FIN-TS §7.4). Consolida a posição consolidada por período a partir
    /// dos títulos a receber/pagar EM ABERTO (por vencimento) e do saldo atual das contas financeiras.
    /// </summary>
    public record ProjetarFluxoCaixaQuery(
        DateTime? DataBase, int NumeroPeriodos = 12, EGranularidadeFluxo Granularidade = EGranularidadeFluxo.Mensal)
        : IRequest<CommandResult>;

    public class FluxoCaixaQueryHandlers : IRequestHandler<ProjetarFluxoCaixaQuery, CommandResult>
    {
        private readonly ContextFinanceiro _context;
        public FluxoCaixaQueryHandlers(ContextFinanceiro context) => _context = context;

        public async Task<CommandResult> Handle(ProjetarFluxoCaixaQuery request, CancellationToken ct)
        {
            var numeroPeriodos = request.NumeroPeriodos is <= 0 or > 120 ? 12 : request.NumeroPeriodos;
            var dataBase = (request.DataBase ?? DateTime.UtcNow).Date;

            // ----- Saldo inicial: posição atual de caixa -----
            // saldo de abertura das contas não fechadas + movimentos REALIZADOS (Planejamento = false).
            var saldoAbertura = await _context.ContasFinanceiras
                .Where(c => !c.Fechada)
                .SumAsync(c => c.SaldoAbertura ?? 0m, ct);

            var movimentos = await _context.MovimentosFinanceiros
                .Where(m => !m.Planejamento)
                .GroupBy(_ => 1)
                .Select(g => new { Credito = g.Sum(x => x.Credito), Debito = g.Sum(x => x.Debito) })
                .FirstOrDefaultAsync(ct);

            var saldoInicial = saldoAbertura + (movimentos?.Credito ?? 0m) - (movimentos?.Debito ?? 0m);

            // ----- Lançamentos previstos: CR (entradas) e CP (saídas) em aberto -----
            var receber = await _context.ContasAReceberAgregado
                .Where(cr => cr.Situacao == ESituacao.Aberto || cr.Situacao == ESituacao.PagoParcial)
                .Select(cr => new
                {
                    cr.DataVencimento,
                    Saldo = cr.ValorTotalAReceberTitulo > 0m ? cr.ValorTotalAReceberTitulo : cr.ValorInicialAReceberTitulo
                })
                .ToListAsync(ct);

            var pagar = await _context.ContasAPagarAgregado
                .Where(cp => cp.Situacao == ESituacao.Aberto || cp.Situacao == ESituacao.PagoParcial)
                .Select(cp => new
                {
                    cp.DataVencimento,
                    Saldo = cp.ValorTotalAPagarTitulo > 0m ? cp.ValorTotalAPagarTitulo : cp.ValorInicialAPagarTitulo
                })
                .ToListAsync(ct);

            var lancamentos = receber.Where(r => r.Saldo > 0m).Select(r => new LancamentoPrevisto(r.DataVencimento, r.Saldo, true))
                .Concat(pagar.Where(p => p.Saldo > 0m).Select(p => new LancamentoPrevisto(p.DataVencimento, p.Saldo, false)))
                .ToList();

            var baldes = ProjecaoFluxoCaixa.Projetar(saldoInicial, lancamentos, dataBase, numeroPeriodos, request.Granularidade);

            return CommandResult.Ok("Projeção de fluxo de caixa consolidada.", new
            {
                dataBase,
                granularidade = request.Granularidade.ToString(),
                saldoInicial,
                totalEntradas = baldes.Sum(b => b.Entradas),
                totalSaidas = baldes.Sum(b => b.Saidas),
                saldoProjetadoFinal = baldes.Count > 0 ? baldes[^1].SaldoFinal : saldoInicial,
                periodos = baldes.Select(b => new
                {
                    b.PeriodoInicio, b.PeriodoFim, b.SaldoInicial, b.Entradas, b.Saidas, b.FluxoLiquido, b.SaldoFinal
                })
            });
        }
    }
}
