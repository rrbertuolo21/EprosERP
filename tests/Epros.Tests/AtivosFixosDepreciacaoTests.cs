using System;
using Epros.Modules.Financeiro.Domain.Entities;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Modules.Financeiro.Domain.Services;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// FIN-AFX — motor de depreciação (fórmulas UNIVERSAIS de contabilidade do imobilizado;
    /// cita Negocio-acumulado/contabil). A taxa/vida útil de referência da RFB é fato legal
    /// informado (valida-contador); estes testes exercitam a fórmula, não fixam número legal.
    /// </summary>
    public class AtivosFixosDepreciacaoTests
    {
        private const string TenantId = "tenant-afx-dep-001";
        private const string UserId = "user-afx-dep-001";

        private static AtivoFixo NovoAtivo(decimal custo, ETipoDepreciacaoAtivo metodo, decimal? taxaMensal)
            => new AtivoFixo("Bem", custo, DateTime.UtcNow, null, null, null, null, null, null,
                null, null, null, null, null, null, custo, null, true, metodo, null, taxaMensal, TenantId, UserId);

        // ===== Método linear (cotas constantes) =====
        [Fact(DisplayName = "Depreciação linear | base × taxa mensal = cota constante")]
        public void Linear_BaseVezesTaxa_CotaConstante()
        {
            // 10% a.a. => ~0.0083333 a.m. (taxa informada = fato legal RFB, valida-contador)
            var taxaMensal = 0.10m / 12m;
            var cota = CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.Linear, 12000m, 12000m, 0m, taxaMensal);
            Assert.Equal(100.00m, cota); // 12000 * 0.10 / 12
        }

        [Fact(DisplayName = "Depreciação linear | por vida útil (sem taxa) = base ÷ meses")]
        public void Linear_PorVidaUtil_BaseDivididaPelosMeses()
        {
            var cota = CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.Linear, 6000m, 6000m, 0m, taxaMensal: null, vidaUtilMeses: 60);
            Assert.Equal(100.00m, cota); // 6000 / 60
        }

        [Fact(DisplayName = "Depreciação | valor residual reduz a base depreciável")]
        public void Residual_ReduzBaseDepreciavel()
        {
            // base = 10000 - 2000 = 8000; /40 meses = 200
            var cota = CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.Linear, 10000m, 10000m, 2000m, taxaMensal: null, vidaUtilMeses: 40);
            Assert.Equal(200.00m, cota);
        }

        // ===== Saldos decrescentes =====
        [Fact(DisplayName = "Saldos decrescentes | taxa sobre o valor contábil (declinante)")]
        public void SaldoDecrescente_TaxaSobreValorContabil()
        {
            var taxa = 0.20m;
            var mes1 = CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.SaldoDecrescente, 1000m, 1000m, 0m, taxa);
            Assert.Equal(200.00m, mes1); // 1000 * 0.20
            var mes2 = CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.SaldoDecrescente, 1000m, 800m, 0m, taxa);
            Assert.Equal(160.00m, mes2); // 800 * 0.20 — cota menor que a do mês anterior
            Assert.True(mes2 < mes1);
        }

        // ===== Soma dos dígitos (SYD) =====
        [Fact(DisplayName = "Soma dos dígitos | fração decrescente (n−k+1)/Σ sobre a base")]
        public void SomaDosDigitos_FracaoDecrescente()
        {
            // vida útil 4; Σ dígitos = 10; base = 1000
            var p1 = CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.SomaDosDigitos, 1000m, 1000m, 0m, null, vidaUtilMeses: 4, periodoCorrente: 1);
            var p4 = CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.SomaDosDigitos, 1000m, 100m, 0m, null, vidaUtilMeses: 4, periodoCorrente: 4);
            Assert.Equal(400.00m, p1); // 1000 * 4/10
            Assert.Equal(100.00m, p4); // 1000 * 1/10
            Assert.True(p1 > p4);
        }

        [Fact(DisplayName = "Soma dos dígitos | soma das cotas = base depreciável")]
        public void SomaDosDigitos_SomaDasCotas_IgualBase()
        {
            decimal soma = 0m;
            for (int k = 1; k <= 4; k++)
                soma += CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.SomaDosDigitos, 1000m, 1000m, 0m, null, 4, k);
            Assert.Equal(1000.00m, soma);
        }

        // ===== Invariante: nunca depreciar abaixo do residual =====
        [Fact(DisplayName = "Depreciação | última cota é ajustada para não passar do residual")]
        public void UltimaCota_NaoPassaDoResidual()
        {
            // saldo contábil já no residual: cota = 0
            var cota = CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.SaldoDecrescente, 1000m, 100m, 100m, 0.50m);
            Assert.Equal(0m, cota);

            // saldo pouco acima do residual: cota limitada ao depreciável restante (120-100=20)
            var cotaLimite = CalculoDepreciacao.CotaMensal(ETipoDepreciacaoAtivo.SaldoDecrescente, 1000m, 120m, 100m, 0.50m);
            Assert.Equal(20.00m, cotaLimite); // 0.50*120=60, mas limitado a 20
        }

        // ===== Integração com a entidade =====
        [Fact(DisplayName = "AtivoFixo | CalcularCotaDepreciacaoMensal usa a taxa do ativo")]
        public void Entidade_CalcularCota_UsaTaxaDoAtivo()
        {
            var ativo = NovoAtivo(12000m, ETipoDepreciacaoAtivo.Linear, 0.10m / 12m);
            Assert.Equal(100.00m, ativo.CalcularCotaDepreciacaoMensal());
        }

        [Fact(DisplayName = "AtivoFixo | não depreciável retorna cota zero")]
        public void Entidade_NaoDepreciavel_CotaZero()
        {
            var ativo = new AtivoFixo("Terreno", 50000m, DateTime.UtcNow, null, null, null, null, null, null,
                null, null, null, null, null, null, 50000m, null, false, null, null, null, TenantId, UserId);
            Assert.Equal(0m, ativo.CalcularCotaDepreciacaoMensal());
        }

        [Fact(DisplayName = "AtivoFixo | cota calculada aplicada reduz o valor contábil")]
        public void Entidade_CotaCalculada_ReduzValorContabil()
        {
            var ativo = NovoAtivo(12000m, ETipoDepreciacaoAtivo.Linear, 0.10m / 12m);
            var cota = ativo.CalcularCotaDepreciacaoMensal();
            ativo.AplicarDepreciacao(cota, DateTime.UtcNow, UserId);
            Assert.Equal(11900.00m, ativo.ValorAtualizado);
        }
    }
}
