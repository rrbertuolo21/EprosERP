using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.Financeiro.Domain.Services
{
    /// <summary>Granularidade do balde de projeção de fluxo de caixa.</summary>
    public enum EGranularidadeFluxo { Diario = 0, Semanal = 1, Mensal = 2 }

    /// <summary>Um lançamento previsto (título a receber/pagar em aberto, por data de vencimento).</summary>
    /// <param name="Data">Data prevista (vencimento).</param>
    /// <param name="Valor">Valor absoluto (sempre &gt; 0).</param>
    /// <param name="EhEntrada">true = entrada (CR); false = saída (CP).</param>
    public sealed record LancamentoPrevisto(DateTime Data, decimal Valor, bool EhEntrada);

    /// <summary>Posição consolidada de um período (balde) da projeção de fluxo de caixa.</summary>
    public sealed record BaldeFluxoCaixa(
        DateTime PeriodoInicio, DateTime PeriodoFim, decimal SaldoInicial,
        decimal Entradas, decimal Saidas, decimal SaldoFinal)
    {
        /// <summary>Geração líquida de caixa do período (entradas − saídas).</summary>
        public decimal FluxoLiquido => Entradas - Saidas;
    }

    /// <summary>
    /// Motor de PROJEÇÃO de fluxo de caixa (FIN-TS §7.4 / RTS-036/037). Consolida a POSIÇÃO de caixa
    /// por período a partir de um saldo inicial (saldos das contas/caixa) e dos lançamentos previstos
    /// (títulos a receber/pagar em aberto, por vencimento), rolando o saldo acumulado balde a balde
    /// (saldo_final = saldo_inicial + entradas − saídas; o saldo final de um período é o inicial do
    /// seguinte). Função pura e determinística — não toca banco; o handler alimenta os dados.
    ///
    /// RTS-037: saldo do período = entradas − saídas; o acumulado projeta a liquidez futura. Critério
    /// de "aberto"/data-base e política de inadimplência = regra do submódulo dono do título.
    /// </summary>
    public static class ProjecaoFluxoCaixa
    {
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>
        /// Projeta <paramref name="numeroPeriodos"/> baldes a partir de <paramref name="dataBase"/> na
        /// granularidade indicada. Lançamentos anteriores à data-base entram no primeiro balde (posição
        /// vencida); lançamentos além do horizonte são ignorados. Cada balde é [início, fim).
        /// </summary>
        public static IReadOnlyList<BaldeFluxoCaixa> Projetar(
            decimal saldoInicial, IEnumerable<LancamentoPrevisto> lancamentos,
            DateTime dataBase, int numeroPeriodos, EGranularidadeFluxo granularidade = EGranularidadeFluxo.Mensal)
        {
            if (numeroPeriodos <= 0) throw new ArgumentOutOfRangeException(nameof(numeroPeriodos));
            var lista = (lancamentos ?? Enumerable.Empty<LancamentoPrevisto>()).ToList();

            var baldes = new List<BaldeFluxoCaixa>(numeroPeriodos);
            var inicioPrimeiro = InicioPeriodo(dataBase.Date, granularidade);
            var saldo = R2(saldoInicial);

            for (var p = 0; p < numeroPeriodos; p++)
            {
                var inicio = AvancarPeriodos(inicioPrimeiro, granularidade, p);
                var fim = AvancarPeriodos(inicioPrimeiro, granularidade, p + 1);

                // O primeiro balde captura também tudo que vence ANTES da data-base (posição vencida).
                bool NoBalde(LancamentoPrevisto l) => p == 0
                    ? l.Data.Date < fim
                    : l.Data.Date >= inicio && l.Data.Date < fim;

                var doBalde = lista.Where(NoBalde).ToList();
                var entradas = R2(doBalde.Where(l => l.EhEntrada).Sum(l => l.Valor));
                var saidas = R2(doBalde.Where(l => !l.EhEntrada).Sum(l => l.Valor));
                var saldoFinal = R2(saldo + entradas - saidas);

                baldes.Add(new BaldeFluxoCaixa(inicio, fim, saldo, entradas, saidas, saldoFinal));
                saldo = saldoFinal;
            }
            return baldes;
        }

        private static DateTime InicioPeriodo(DateTime data, EGranularidadeFluxo g) => g switch
        {
            EGranularidadeFluxo.Diario => data.Date,
            EGranularidadeFluxo.Semanal => data.Date.AddDays(-(int)data.DayOfWeek), // semana inicia no domingo
            EGranularidadeFluxo.Mensal => new DateTime(data.Year, data.Month, 1),
            _ => data.Date
        };

        private static DateTime AvancarPeriodos(DateTime inicio, EGranularidadeFluxo g, int n) => g switch
        {
            EGranularidadeFluxo.Diario => inicio.AddDays(n),
            EGranularidadeFluxo.Semanal => inicio.AddDays(7 * n),
            EGranularidadeFluxo.Mensal => inicio.AddMonths(n),
            _ => inicio.AddMonths(n)
        };
    }
}
