using System.Collections.Generic;
using System.Linq;
using Epros.Modules.RH.Domain.Folha.Calculo;
using Xunit;

namespace Epros.Tests
{
    // Motor da folha mensal — ordem de apuração (skill departamento-pessoal/folha).
    public class RHMotorFolhaMensalTests
    {
        private static readonly TabelasFolha T = TabelasFolha.Vigente(2026);

        [Fact]
        public void Folha_Simples_Salario_Sem_Adicionais()
        {
            // 3000: INSS 248,60; IRRF 0 (redutor/simplificado); FGTS 240; líquido 2751,40.
            var r = MotorFolhaMensal.Calcular(new EntradaFolhaMensal(3000m), T);
            Assert.Equal(3000m, r.TotalProventos);
            Assert.Equal(248.60m, r.Inss);
            Assert.Equal(0m, r.Irrf);
            Assert.Equal(240.00m, r.Fgts);
            Assert.Equal(2751.40m, r.Liquido);
        }

        [Fact]
        public void Folha_Alta_Renda_Tem_Irrf_E_Fgts_Fora_Do_Liquido()
        {
            // 10000: INSS 988,09; IRRF 1569,55 (sem redutor); FGTS 800 (não entra no líquido).
            // líquido = 10000 − 988,09 − 1569,55 = 7442,36.
            var r = MotorFolhaMensal.Calcular(new EntradaFolhaMensal(10000m), T);
            Assert.Equal(988.09m, r.Inss);
            Assert.Equal(1569.55m, r.Irrf);
            Assert.Equal(800.00m, r.Fgts);
            Assert.Equal(7442.36m, r.Liquido);
        }

        [Fact]
        public void Folha_Com_Adicional_Entra_Nas_Bases()
        {
            // Salário 3000 + HE 500 → base INSS 3500 → INSS 308,60.
            var r = MotorFolhaMensal.Calcular(new EntradaFolhaMensal(
                3000m,
                ProventosAdicionais: new List<ItemProvento> { new("300", "Horas extras", 500m) }), T);
            Assert.Equal(3500m, r.TotalProventos);
            Assert.Equal(308.60m, r.Inss);
        }

        [Fact]
        public void Folha_Provento_Indenizatorio_Nao_Compoe_Bases()
        {
            // Abono indenizatório 1000 não incide INSS/IRRF/FGTS, mas soma ao líquido.
            var r = MotorFolhaMensal.Calcular(new EntradaFolhaMensal(
                3000m,
                ProventosAdicionais: new List<ItemProvento>
                {
                    new("500", "Abono indenizatório", 1000m, IncideInss: false, IncideIrrf: false, IncideFgts: false)
                }), T);
            Assert.Equal(4000m, r.TotalProventos);
            Assert.Equal(3000m, r.BaseInss);      // abono fora da base
            Assert.Equal(240.00m, r.Fgts);        // FGTS só sobre 3000
            Assert.Equal(248.60m, r.Inss);
            Assert.Equal(3751.40m, r.Liquido);    // 4000 − 248,60
        }

        [Fact]
        public void Folha_Vale_Transporte_Limitado_A_6_Porcento()
        {
            // Salário 2000, VT informado 200 > 6%×2000 = 120 → desconta 120.
            var r = MotorFolhaMensal.Calcular(new EntradaFolhaMensal(2000m, DescontoValeTransporte: 200m), T);
            var vt = r.Rubricas.Single(x => x.Codigo == "930");
            Assert.Equal(120.00m, vt.Valor);
        }

        [Fact]
        public void Folha_Descontos_Diversos_Reduzem_Liquido()
        {
            var r = MotorFolhaMensal.Calcular(new EntradaFolhaMensal(
                3000m,
                DescontosDiversos: new List<ItemDesconto> { new("910", "Adiantamento", 500m) }), T);
            // líquido = 3000 − 248,60 (INSS) − 500 (adiantamento) = 2251,40
            Assert.Equal(2251.40m, r.Liquido);
        }

        // ---- DSR sobre variáveis (RN-04) ----
        [Fact]
        public void Dsr_Sobre_Variaveis()
        {
            // 1100 de variáveis, 22 dias úteis, 8 de repouso → 1100/22×8 = 400.
            Assert.Equal(400.00m, MotorDsr.Calcular(1100m, 22, 8));
        }

        [Fact]
        public void Dsr_Zero_Quando_Sem_Variaveis()
        {
            Assert.Equal(0m, MotorDsr.Calcular(0m, 22, 8));
        }
    }
}
