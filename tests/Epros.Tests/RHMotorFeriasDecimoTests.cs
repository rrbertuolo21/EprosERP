using System;
using Epros.Modules.RH.Domain.Folha.Calculo;
using Xunit;

namespace Epros.Tests
{
    // Motores de férias (RN-05) e 13º (RN-06). Skill departamento-pessoal/folha.
    public class RHMotorFeriasDecimoTests
    {
        private static readonly TabelasFolha T = TabelasFolha.Vigente(2026);

        // ---------------- Férias ----------------

        [Fact]
        public void Ferias_30_Dias_Com_Terco()
        {
            // Remuneração 3000, 30 dias: férias 3000 + 1/3 = 1000; tributável 4000; INSS 368,60.
            var r = MotorFerias.Calcular(3000m, T);
            Assert.Equal(3000m, r.ValorFerias);
            Assert.Equal(1000m, r.TercoConstitucional);
            Assert.Equal(4000m, r.BaseTributavel);
            Assert.Equal(368.60m, r.Inss);
            Assert.Equal(3631.40m, r.Liquido); // 4000 − 368,60 − 0 (IRRF zerado pelo redutor)
        }

        [Fact]
        public void Ferias_Com_Abono_Pecuniario_Indenizatorio()
        {
            // 20 dias de férias + 10 de abono (venda de 1/3). Abono indenizatório fora das bases.
            var r = MotorFerias.Calcular(3000m, T, diasFerias: 20, diasAbono: 10);
            Assert.Equal(2000.00m, r.ValorFerias);        // 3000/30×20
            Assert.Equal(666.67m, r.TercoConstitucional); // 2000/3
            Assert.Equal(1000.00m, r.Abono);              // 3000/30×10
            Assert.Equal(333.33m, r.TercoAbono);
            Assert.Equal(2666.67m, r.BaseTributavel);     // só férias + 1/3
        }

        [Fact]
        public void Ferias_Media_Variaveis_Integra_Base()
        {
            var semMedia = MotorFerias.Calcular(3000m, T);
            var comMedia = MotorFerias.Calcular(3000m, T, mediaVariaveis: 600m);
            Assert.True(comMedia.ValorFerias > semMedia.ValorFerias);
        }

        [Fact]
        public void Ferias_Abono_Acima_De_10_Dias_Rejeita()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MotorFerias.Calcular(3000m, T, diasAbono: 11));
        }

        // ---------------- 13º ----------------

        [Fact]
        public void Decimo_Integral_Duas_Parcelas()
        {
            // 3000, 12 meses: bruto 3000; 1ª parcela 1500; INSS13 248,60; IRRF13 0; 2ª = 1251,40.
            var r = MotorDecimoTerceiro.Calcular(3000m, 12, T);
            Assert.Equal(3000m, r.Bruto);
            Assert.Equal(1500m, r.PrimeiraParcela);
            Assert.Equal(248.60m, r.Inss);
            Assert.Equal(1251.40m, r.SegundaParcela);
            Assert.Equal(240.00m, r.Fgts);
        }

        [Fact]
        public void Decimo_Proporcional_Por_Avos()
        {
            // 6 meses: bruto 1500; INSS 112,50; 2ª parcela 637,50.
            var r = MotorDecimoTerceiro.Calcular(3000m, 6, T);
            Assert.Equal(1500m, r.Bruto);
            Assert.Equal(637.50m, r.SegundaParcela);
        }

        [Fact]
        public void Avos_Mes_Com_15_Dias_Conta_Inteiro()
        {
            // Admissão 17/01 → janeiro tem 15 dias (17..31) → conta; ano todo até 31/12 = 12 avos.
            Assert.Equal(12, MotorDecimoTerceiro.AvosPorDias(new DateTime(2026, 1, 17), new DateTime(2026, 12, 31)));
            // Admissão 18/01 → janeiro tem 14 dias → não conta → 11 avos.
            Assert.Equal(11, MotorDecimoTerceiro.AvosPorDias(new DateTime(2026, 1, 18), new DateTime(2026, 12, 31)));
        }
    }
}
