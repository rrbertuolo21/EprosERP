using System;

namespace Epros.Modules.Projetos.Domain.Services
{
    /// <summary>
    /// Resultado de uma medição de Earned Value Management (EVM / Gestão de Valor Agregado).
    /// Valores em moeda do orçamento; índices adimensionais.
    /// </summary>
    public readonly record struct ResultadoEvm(
        decimal Bac,   // Budget At Completion (orçamento na conclusão = baseline budget)
        decimal Pv,    // Planned Value (valor planejado)
        decimal Ev,    // Earned Value (valor agregado)
        decimal Ac,    // Actual Cost (custo real)
        decimal Cv,    // Cost Variance = EV - AC
        decimal Sv,    // Schedule Variance = EV - PV
        decimal Cpi,   // Cost Performance Index = EV / AC
        decimal Spi,   // Schedule Performance Index = EV / PV
        decimal Eac,   // Estimate At Completion
        decimal Etc,   // Estimate To Complete
        decimal Vac,   // Variance At Completion = BAC - EAC
        decimal Tcpi   // To-Complete Performance Index
    );

    /// <summary>
    /// EVM — Earned Value Management (universal, PMBOK/agnóstico). Motor puro de cálculo, sem I/O.
    /// Fórmulas padrão de gestão de valor agregado; a interpretação contábil de PV/EV/AC e o método de
    /// medição de EV (0/100, %físico, marcos ponderados) referenciam <c>Negocio-acumulado/contabil</c>
    /// (DP-ORC-004/005 — método de EV e fórmulas contábeis validadas com o contador). Aqui não se inventa
    /// alíquota, câmbio nem política contábil: só se aplicam as fórmulas canônicas sobre valores informados.
    /// </summary>
    public static class EvmCalculadora
    {
        /// <summary>
        /// Calcula os indicadores EVM a partir dos três valores primários.
        /// </summary>
        /// <param name="bac">Budget At Completion (orçamento total da baseline).</param>
        /// <param name="pv">Planned Value (custo orçado do trabalho previsto até a data).</param>
        /// <param name="ev">Earned Value (custo orçado do trabalho realizado até a data).</param>
        /// <param name="ac">Actual Cost (custo real incorrido até a data).</param>
        public static ResultadoEvm Calcular(decimal bac, decimal pv, decimal ev, decimal ac)
        {
            var cv = ev - ac;
            var sv = ev - pv;
            var cpi = ac != 0 ? ev / ac : 0m;
            var spi = pv != 0 ? ev / pv : 0m;

            // EAC = BAC / CPI (assume desempenho de custo futuro = passado). Se CPI==0, cai para BAC.
            var eac = cpi != 0 ? bac / cpi : bac;
            var etc = eac - ac;
            var vac = bac - eac;

            // TCPI = trabalho restante / verba restante = (BAC - EV) / (BAC - AC).
            var tcpi = (bac - ac) != 0 ? (bac - ev) / (bac - ac) : 0m;

            return new ResultadoEvm(
                Bac: Arred(bac), Pv: Arred(pv), Ev: Arred(ev), Ac: Arred(ac),
                Cv: Arred(cv), Sv: Arred(sv),
                Cpi: Arred(cpi, 4), Spi: Arred(spi, 4),
                Eac: Arred(eac), Etc: Arred(etc), Vac: Arred(vac), Tcpi: Arred(tcpi, 4));
        }

        /// <summary>
        /// Conveniência: deriva PV e EV de percentuais (0..100) sobre o BAC e calcula o EVM.
        /// EV = BAC * %concluído; PV = BAC * %planejado. Método de EV = valida-contador (contabil).
        /// </summary>
        public static ResultadoEvm CalcularPorPercentual(decimal bac, decimal percentualPlanejado, decimal percentualConcluido, decimal actualCost)
        {
            var pv = bac * (Clamp(percentualPlanejado) / 100m);
            var ev = bac * (Clamp(percentualConcluido) / 100m);
            return Calcular(bac, pv, ev, actualCost);
        }

        private static decimal Clamp(decimal pct) => pct < 0 ? 0 : (pct > 100 ? 100 : pct);
        private static decimal Arred(decimal v, int casas = 2) => Math.Round(v, casas, MidpointRounding.AwayFromZero);
    }
}
