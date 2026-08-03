using System;
using Epros.Modules.RH.Domain.Folha.Calculo;

namespace Epros.Modules.RH.Domain.Sst.Calculo
{
    // ============================================================================================
    //  MOTOR DE ADICIONAIS DE SST — insalubridade (NR-15) e periculosidade (NR-16).
    //  Skill: departamento-pessoal/sst — RN-01 (insalubridade), RN-02 (periculosidade).
    //  ⚠️ O ENQUADRAMENTO de grau/agente é decisão de LAUDO TÉCNICO (LTCAT/perícia) — o sistema
    //     parametriza, nunca arbitra o grau. Aqui só se CALCULA a partir de um grau já enquadrado.
    //  // valida-contador — base da insalubridade (salário mínimo × piso da CCT) é controvérsia
    //     jurídica (SV4 STF / Súmula 228 TST suspensa). Default de mercado = salário mínimo.
    // ============================================================================================

    public enum GrauInsalubridade
    {
        Nenhum = 0,
        Minimo = 10,   // 10%
        Medio = 20,    // 20%
        Maximo = 40    // 40%
    }

    public sealed record ResultadoAdicionalSst(
        decimal Insalubridade,
        decimal Periculosidade,
        decimal AdicionalDevido,
        string Escolhido); // "Insalubridade", "Periculosidade" ou "Nenhum"

    public static class MotorAdicionalSst
    {
        public const decimal PercentualPericulosidade = 0.30m; // CLT art. 193 §1º — 30% do salário-base

        /// <summary>Adicional de insalubridade = percentual do grau × base (default: salário mínimo).</summary>
        public static decimal Insalubridade(GrauInsalubridade grau, decimal baseCalculo)
        {
            if (grau == GrauInsalubridade.Nenhum || baseCalculo <= 0) return 0m;
            var percentual = (int)grau / 100m;
            return Arredondamento.Reais(baseCalculo * percentual);
        }

        /// <summary>Adicional de periculosidade = 30% do salário-base (CLT art. 193 §1º).</summary>
        public static decimal Periculosidade(decimal salarioBase)
        {
            if (salarioBase <= 0) return 0m;
            return Arredondamento.Reais(salarioBase * PercentualPericulosidade);
        }

        /// <summary>
        /// Regra do NÃO CUMULATIVO (CLT art. 193 §2º): quando há direito a insalubridade E
        /// periculosidade, o empregado opta pelo MAIS BENÉFICO. Retorna ambos e o escolhido.
        /// Base da insalubridade = salário mínimo vigente (default valida-contador).
        /// </summary>
        public static ResultadoAdicionalSst Calcular(
            GrauInsalubridade grauInsalubridade,
            decimal salarioBase,
            TabelasFolha tabelas,
            bool temPericulosidade,
            decimal? baseInsalubridadeOverride = null)
        {
            if (tabelas == null) throw new ArgumentNullException(nameof(tabelas));

            var baseInsalub = baseInsalubridadeOverride ?? tabelas.SalarioMinimo;
            var insalub = Insalubridade(grauInsalubridade, baseInsalub);
            var pericul = temPericulosidade ? Periculosidade(salarioBase) : 0m;

            decimal devido;
            string escolhido;
            if (insalub == 0m && pericul == 0m) { devido = 0m; escolhido = "Nenhum"; }
            else if (pericul >= insalub) { devido = pericul; escolhido = "Periculosidade"; }
            else { devido = insalub; escolhido = "Insalubridade"; }

            return new ResultadoAdicionalSst(insalub, pericul, devido, escolhido);
        }
    }
}
