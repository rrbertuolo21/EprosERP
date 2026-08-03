using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Financeiro.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// FIN-GCF / dívida estruturada — motor de crédito (fórmulas UNIVERSAIS de matemática financeira;
    /// cita Negocio-acumulado/financeiro/credito). Oráculos = exemplos trabalhados da skill
    /// (PV=10.000, i=2% a.m., n=12). Alíquotas de IOF e divulgação do CET = fato legal (valida-contador);
    /// estes testes exercitam a fórmula, não fixam número legal.
    /// </summary>
    public class CalculoAmortizacaoTests
    {
        // ===== Tabela Price =====
        [Fact(DisplayName = "Price | PMT = PV·i/(1−(1+i)^−n) (exemplo da skill = 945,60)")]
        public void Price_Prestacao_ConfereComSkill()
        {
            var pmt = CalculoAmortizacao.PrestacaoPrice(10000m, 0.02m, 12);
            Assert.Equal(945.60m, decimal.Round(pmt, 2));
        }

        [Fact(DisplayName = "Price | primeira linha: juros 200, amortização 745,60, saldo 9.254,40")]
        public void Price_PrimeiraLinha_ConfereComSkill()
        {
            var t = CalculoAmortizacao.Tabela(ESistemaAmortizacao.Price, 10000m, 0.02m, 12);
            Assert.Equal(945.60m, t[0].Prestacao);
            Assert.Equal(200.00m, t[0].Juros);
            Assert.Equal(745.60m, t[0].Amortizacao);
            Assert.Equal(9254.40m, t[0].SaldoDevedor);
        }

        [Fact(DisplayName = "Price | saldo final zera e total de juros ≈ 1.347,20")]
        public void Price_SaldoZera_TotalJuros()
        {
            var t = CalculoAmortizacao.Tabela(ESistemaAmortizacao.Price, 10000m, 0.02m, 12);
            Assert.Equal(0m, t.Last().SaldoDevedor);
            Assert.InRange(CalculoAmortizacao.TotalJuros(t), 1347.10m, 1347.30m);
        }

        // ===== Tabela SAC =====
        [Fact(DisplayName = "SAC | amortização constante 833,33 e 1ª prestação 1.033,33")]
        public void Sac_ConfereComSkill()
        {
            var t = CalculoAmortizacao.Tabela(ESistemaAmortizacao.Sac, 10000m, 0.02m, 12);
            Assert.Equal(833.33m, t[0].Amortizacao);
            Assert.Equal(200.00m, t[0].Juros);
            Assert.Equal(1033.33m, t[0].Prestacao);
            Assert.Equal(0m, t.Last().SaldoDevedor);
        }

        [Fact(DisplayName = "SAC | total de juros = i·PV·(n+1)/2 = 1.300 (< Price)")]
        public void Sac_TotalJuros_Identidade()
        {
            var sac = CalculoAmortizacao.Tabela(ESistemaAmortizacao.Sac, 10000m, 0.02m, 12);
            var price = CalculoAmortizacao.Tabela(ESistemaAmortizacao.Price, 10000m, 0.02m, 12);
            // identidade aritmética: 0,02 · 10000 · 13/2 = 1300
            Assert.InRange(CalculoAmortizacao.TotalJuros(sac), 1299.90m, 1300.10m);
            // consequência aritmética universal: SAC paga menos juros que Price
            Assert.True(CalculoAmortizacao.TotalJuros(sac) < CalculoAmortizacao.TotalJuros(price));
        }

        [Fact(DisplayName = "SAC | prestação decrescente (juros caem com o saldo)")]
        public void Sac_PrestacaoDecrescente()
        {
            var t = CalculoAmortizacao.Tabela(ESistemaAmortizacao.Sac, 10000m, 0.02m, 12);
            for (var k = 1; k < t.Count; k++)
                Assert.True(t[k].Prestacao <= t[k - 1].Prestacao);
        }

        // ===== Equivalência de taxa =====
        [Fact(DisplayName = "Taxa | equivalência composta i_mês = (1+i_ano)^(1/12)−1 (não i/12)")]
        public void Taxa_EquivalenciaComposta()
        {
            var im = CalculoAmortizacao.MensalDeAnual(0.2680m); // ~2% a.m.
            Assert.InRange(im, 0.0198m, 0.0202m);
            // round-trip: anualizar volta perto do anual
            Assert.InRange(CalculoAmortizacao.Anualizar(im), 0.2678m, 0.2682m);
        }

        // ===== IOF (estrutura) =====
        [Fact(DisplayName = "IOF | estrutura = diário (teto 365) + adicional; alíquota informada (valida-contador)")]
        public void Iof_Estrutura_ComTeto()
        {
            // alíquotas de referência da skill (pré-2025) usadas SÓ como parâmetro de teste
            const decimal aliqDia = 0.000082m, aliqAdic = 0.0038m;
            // 30 dias: 10000·0,000082·30 + 10000·0,0038 = 24,60 + 38,00 = 62,60
            Assert.Equal(62.60m, CalculoAmortizacao.IofCredito(10000m, 30, aliqDia, aliqAdic));
            // teto de 365 dias: 400 dias trava em 365
            var d365 = CalculoAmortizacao.IofCredito(10000m, 365, aliqDia, aliqAdic);
            var d400 = CalculoAmortizacao.IofCredito(10000m, 400, aliqDia, aliqAdic);
            Assert.Equal(d365, d400);
        }

        // ===== CET =====
        [Fact(DisplayName = "CET | sem custos além do juro, CET_anual ≈ juro anual equivalente")]
        public void Cet_SemCustos_IgualaJuro()
        {
            var t = CalculoAmortizacao.Tabela(ESistemaAmortizacao.Price, 10000m, 0.02m, 12);
            var parcelas = t.Select(p => p.Prestacao).ToList();
            var cet = CalculoAmortizacao.CetAnual(10000m, parcelas, 12);
            var juroAnual = CalculoAmortizacao.Anualizar(0.02m, 12); // ~26,82%
            Assert.InRange(cet, juroAnual - 0.005m, juroAnual + 0.005m);
        }

        [Fact(DisplayName = "CET | com tarifa retida na liberação, CET_anual > juro anual (sanidade)")]
        public void Cet_ComTarifa_MaiorQueJuro()
        {
            var t = CalculoAmortizacao.Tabela(ESistemaAmortizacao.Price, 10000m, 0.02m, 12);
            var parcelas = t.Select(p => p.Prestacao).ToList();
            // tarifa de cadastro de R$ 200 retida ⇒ cliente recebe 9.800, paga as mesmas parcelas
            var cet = CalculoAmortizacao.CetAnual(9800m, parcelas, 12);
            var juroAnual = CalculoAmortizacao.Anualizar(0.02m, 12);
            Assert.True(cet > juroAnual, $"CET {cet} deveria ser > juro {juroAnual}");
        }
    }
}
