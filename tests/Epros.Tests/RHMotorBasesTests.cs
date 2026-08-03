using System;
using Epros.Modules.RH.Domain.Folha.Calculo;
using Xunit;

namespace Epros.Tests
{
    // Motor de folha — bases legais (INSS/IRRF/FGTS). Oráculo = tabelas 2026 da skill
    // departamento-pessoal/folha (RN-01, RN-02, RN-03). Valores conferidos à mão a partir das faixas.
    public class RHMotorBasesTests
    {
        private static readonly TabelasFolha T = TabelasFolha.Vigente(2026);

        // ---- regra #0: ano sem tabela confirmada não pode inventar ----
        [Fact]
        public void Tabelas_Ano_Sem_Confirmacao_Lanca()
        {
            Assert.Throws<InvalidOperationException>(() => TabelasFolha.Vigente(2025));
            Assert.Throws<InvalidOperationException>(() => TabelasFolha.Vigente(2027));
        }

        // ================= INSS (RN-01, progressivo por faixas) =================

        [Fact]
        public void Inss_Primeira_Faixa()
        {
            // 1500 só na 1ª faixa: 1500 × 7,5% = 112,50
            var r = MotorInss.Calcular(1500m, T.Inss);
            Assert.Equal(112.50m, r.Valor);
            Assert.Equal(1500m, r.BaseContribuicao);
        }

        [Fact]
        public void Inss_Duas_Faixas()
        {
            // 2000: 1621×7,5% + (2000−1621)×9% = 121,575 + 34,11 = 155,685 → 155,69
            var r = MotorInss.Calcular(2000m, T.Inss);
            Assert.Equal(155.69m, r.Valor);
        }

        [Fact]
        public void Inss_Tres_Faixas()
        {
            // 3000: 121,575 + 1281,84×9% + (3000−2902,84)×12% = 121,575 + 115,3656 + 11,6592 = 248,5998 → 248,60
            var r = MotorInss.Calcular(3000m, T.Inss);
            Assert.Equal(248.60m, r.Valor);
        }

        [Fact]
        public void Inss_Acima_Do_Teto_Usa_Desconto_Maximo()
        {
            // Acima do teto (8.475,55) o desconto é o do teto. Somando as 4 faixas 2026 = 988,09.
            // NOTA valida-contador: a skill cita "≈ R$ 951,62" como desconto máximo, mas esse número
            // é o de 2025 (teto antigo 8.157,41). Com as FAIXAS 2026 (teto 8.475,55) o cálculo dá 988,09.
            // O motor computa a partir das faixas (dado estruturado autoritativo). Divergência -> valida-contador.
            var r = MotorInss.Calcular(20000m, T.Inss);
            Assert.Equal(988.09m, r.Valor);
            Assert.Equal(8475.55m, r.BaseContribuicao);
        }

        [Fact]
        public void Inss_Zero_Ou_Negativo_Nao_Desconta()
        {
            Assert.Equal(0m, MotorInss.Calcular(0m, T.Inss).Valor);
            Assert.Equal(0m, MotorInss.Calcular(-100m, T.Inss).Valor);
        }

        // ================= IRRF (RN-02) =================

        [Fact]
        public void Irrf_Tabela_Faixa_Isenta()
        {
            var (imp, aliq) = MotorIrrf.AplicarTabela(2400m, T.Irrf);
            Assert.Equal(0m, imp);
            Assert.Equal(0m, aliq);
        }

        [Fact]
        public void Irrf_Tabela_Faixa_15()
        {
            // base 3000: 3000×15% − 394,16 = 55,84
            var (imp, aliq) = MotorIrrf.AplicarTabela(3000m, T.Irrf);
            Assert.Equal(55.84m, imp);
            Assert.Equal(0.15m, aliq);
        }

        [Fact]
        public void Irrf_Tabela_Faixa_Maxima()
        {
            // base 5000: 5000×27,5% − 908,73 = 466,27
            var (imp, aliq) = MotorIrrf.AplicarTabela(5000m, T.Irrf);
            Assert.Equal(466.27m, imp);
            Assert.Equal(0.275m, aliq);
        }

        // ---- redutor 2026 (Lei 15.270/2025), modelo default valida-contador ----
        [Fact]
        public void Irrf_Redutor_Zera_Ate_Piso()
        {
            // rendimento ≤ 5.000 → redução = imposto (IRRF efetivo zero)
            var redutor = MotorIrrf.CalcularRedutor(466.27m, 4000m, T.Irrf);
            Assert.Equal(466.27m, redutor);
        }

        [Fact]
        public void Irrf_Redutor_Sem_Reducao_Acima_Do_Teto()
        {
            var redutor = MotorIrrf.CalcularRedutor(466.27m, 8000m, T.Irrf);
            Assert.Equal(0m, redutor);
        }

        [Fact]
        public void Irrf_Redutor_Linear_No_Meio()
        {
            // 6.175 é o ponto médio de [5.000, 7.350] → fator 0,5
            var redutor = MotorIrrf.CalcularRedutor(400m, 6175m, T.Irrf);
            Assert.Equal(200m, redutor);
        }

        [Fact]
        public void Irrf_Completo_Alta_Renda_Escolhe_Menor_Carga_E_Sem_Redutor()
        {
            // Salário 10.000; INSS teto 988,09; 0 dependentes; rendimento > 7.350 → sem redutor.
            // Via legal: base = 10000 − 988,09 = 9011,91; 9011,91×27,5% − 908,73 = 1569,54525 → 1569,55.
            // Via simplificada: base = 10000 − 607,20 = 9392,80 → imposto maior → escolhe legal.
            var r = MotorIrrf.Calcular(10000m, 988.09m, 0, T.Irrf);
            Assert.Equal(1569.55m, r.Imposto);
            Assert.Equal(0m, r.Redutor);
            Assert.False(r.UsouSimplificado);
        }

        [Fact]
        public void Irrf_Completo_Baixa_Renda_Fica_Isento_Por_Redutor()
        {
            // Salário 3.000: base simplificada 2.392,80 (isenta) → imposto 0, usa simplificado.
            var r = MotorIrrf.Calcular(3000m, 248.60m, 0, T.Irrf);
            Assert.Equal(0m, r.Imposto);
            Assert.True(r.UsouSimplificado);
        }

        [Fact]
        public void Irrf_Dependentes_Reduzem_Imposto()
        {
            var semDep = MotorIrrf.Calcular(6000m, 641.51m, 0, T.Irrf);
            var comDep = MotorIrrf.Calcular(6000m, 641.51m, 4, T.Irrf);
            Assert.True(comDep.Imposto <= semDep.Imposto);
        }

        [Fact]
        public void Irrf_Redutor_Reduz_Imposto_Na_Faixa_De_Transicao()
        {
            // rendimento entre 5.000 e 7.350 → há redutor > 0 e imposto final < imposto de tabela.
            var r = MotorIrrf.Calcular(6000m, 641.51m, 0, T.Irrf);
            Assert.True(r.Redutor > 0m);
            Assert.True(r.Imposto < r.ImpostoAntesRedutor);
        }

        // ================= FGTS (RN-03) =================

        [Fact]
        public void Fgts_Oito_Porcento()
        {
            Assert.Equal(240.00m, MotorFgts.Calcular(3000m, T));
        }

        [Fact]
        public void Fgts_Aprendiz_Dois_Porcento()
        {
            Assert.Equal(60.00m, MotorFgts.Calcular(3000m, T, aprendiz: true));
        }
    }
}
