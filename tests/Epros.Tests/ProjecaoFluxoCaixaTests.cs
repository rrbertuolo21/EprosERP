using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// FIN-TS — motor de projeção de fluxo de caixa (§7.4 / RTS-036/037). Verifica a consolidação por
    /// período e a rolagem do saldo acumulado (saldo_final de um período = inicial do seguinte).
    /// </summary>
    public class ProjecaoFluxoCaixaTests
    {
        private static readonly DateTime Base = new(2026, 1, 15);

        [Fact(DisplayName = "Fluxo | saldo rola entre baldes (final de um = inicial do próximo)")]
        public void Saldo_Rola_Entre_Baldes()
        {
            var lanc = new List<LancamentoPrevisto>
            {
                new(new DateTime(2026, 1, 20), 1000m, true),   // entrada jan
                new(new DateTime(2026, 2, 10), 300m, false),   // saída fev
                new(new DateTime(2026, 3, 5), 500m, true),     // entrada mar
            };
            var baldes = ProjecaoFluxoCaixa.Projetar(2000m, lanc, Base, 3, EGranularidadeFluxo.Mensal);

            Assert.Equal(3, baldes.Count);
            Assert.Equal(2000m, baldes[0].SaldoInicial);
            Assert.Equal(3000m, baldes[0].SaldoFinal);   // 2000 + 1000
            Assert.Equal(3000m, baldes[1].SaldoInicial); // rola
            Assert.Equal(2700m, baldes[1].SaldoFinal);   // 3000 − 300
            Assert.Equal(2700m, baldes[2].SaldoInicial);
            Assert.Equal(3200m, baldes[2].SaldoFinal);   // 2700 + 500
        }

        [Fact(DisplayName = "Fluxo | entradas e saídas do período conferem (RTS-037: líquido = entradas − saídas)")]
        public void Entradas_Saidas_Por_Periodo()
        {
            var lanc = new List<LancamentoPrevisto>
            {
                new(new DateTime(2026, 1, 20), 1000m, true),
                new(new DateTime(2026, 1, 25), 400m, false),
            };
            var baldes = ProjecaoFluxoCaixa.Projetar(0m, lanc, Base, 1, EGranularidadeFluxo.Mensal);
            Assert.Equal(1000m, baldes[0].Entradas);
            Assert.Equal(400m, baldes[0].Saidas);
            Assert.Equal(600m, baldes[0].FluxoLiquido);
            Assert.Equal(600m, baldes[0].SaldoFinal);
        }

        [Fact(DisplayName = "Fluxo | vencidos (antes da data-base) entram no primeiro balde")]
        public void Vencidos_No_Primeiro_Balde()
        {
            var lanc = new List<LancamentoPrevisto>
            {
                new(new DateTime(2025, 12, 1), 250m, true), // vencido, antes da data-base
            };
            var baldes = ProjecaoFluxoCaixa.Projetar(0m, lanc, Base, 2, EGranularidadeFluxo.Mensal);
            Assert.Equal(250m, baldes[0].Entradas);
            Assert.Equal(0m, baldes[1].Entradas);
        }

        [Fact(DisplayName = "Fluxo | granularidade diária cria baldes de 1 dia")]
        public void Granularidade_Diaria()
        {
            var lanc = new List<LancamentoPrevisto> { new(Base, 100m, true) };
            var baldes = ProjecaoFluxoCaixa.Projetar(0m, lanc, Base, 3, EGranularidadeFluxo.Diario);
            Assert.Equal(Base.Date, baldes[0].PeriodoInicio);
            Assert.Equal(Base.Date.AddDays(1), baldes[0].PeriodoFim);
            Assert.Equal(100m, baldes[0].Entradas);
        }

        [Fact(DisplayName = "Fluxo | além do horizonte é ignorado")]
        public void Alem_Do_Horizonte_Ignorado()
        {
            var lanc = new List<LancamentoPrevisto> { new(new DateTime(2026, 12, 1), 999m, true) };
            var baldes = ProjecaoFluxoCaixa.Projetar(0m, lanc, Base, 3, EGranularidadeFluxo.Mensal);
            Assert.All(baldes, b => Assert.Equal(0m, b.Entradas));
        }
    }
}
