using System;
using Epros.Modules.Financeiro.Domain.Enums;

namespace Epros.Modules.Financeiro.Domain.Services
{
    /// <summary>
    /// Motor de cálculo de depreciação — fórmulas UNIVERSAIS de contabilidade do imobilizado
    /// (cita Negocio-acumulado/contabil): linear/cotas constantes, saldos decrescentes e soma
    /// dos dígitos (SYD). Estrutura pura, sem persistência.
    ///
    /// ⚠️ NÚMERO LEGAL NÃO É INVENTADO AQUI. A taxa anual/mensal e a vida útil de referência da RFB
    /// (IN 1.700/2017 Anexo III — ex.: 10% a.a. móveis, 20% a.a. veículos, 4% a.a. edificações) são
    /// FATO FISCAL e chegam INFORMADAS no ativo (TaxaMensal / vida útil). Este motor só APLICA a
    /// fórmula sobre a taxa recebida. Ao parametrizar taxas por grupo de bem: // valida-contador.
    ///
    /// Invariante contábil respeitada: o valor contábil nunca deprecia abaixo do valor residual;
    /// a última cota é ajustada ("plug") ao saldo depreciável restante.
    /// </summary>
    public static class CalculoDepreciacao
    {
        /// <summary>Base depreciável = custo de aquisição − valor residual (nunca negativa).</summary>
        public static decimal BaseDepreciavel(decimal custo, decimal valorResidual)
            => Math.Max(0m, custo - Math.Max(0m, valorResidual));

        /// <summary>
        /// Cota de depreciação do período corrente, respeitando o piso do valor residual.
        /// </summary>
        /// <param name="metodo">Método contábil (Linear/Acelerada = cotas constantes; SaldoDecrescente; SomaDosDigitos).</param>
        /// <param name="custo">Custo de aquisição (base do bem).</param>
        /// <param name="valorContabilAtual">Saldo contábil corrente (líquido de depreciação acumulada).</param>
        /// <param name="valorResidual">Valor residual estimado (piso; padrão 0).</param>
        /// <param name="taxaMensal">Taxa mensal de depreciação (fração, ex.: 0.00833 p/ 10% a.a.). Fato legal informado.</param>
        /// <param name="vidaUtilMeses">Vida útil total em meses (necessária p/ SomaDosDigitos e p/ Linear por prazo).</param>
        /// <param name="periodoCorrente">Índice 1-based do período corrente (necessário p/ SomaDosDigitos).</param>
        public static decimal CotaMensal(
            ETipoDepreciacaoAtivo metodo,
            decimal custo,
            decimal valorContabilAtual,
            decimal valorResidual = 0m,
            decimal? taxaMensal = null,
            int? vidaUtilMeses = null,
            int? periodoCorrente = null)
        {
            valorResidual = Math.Max(0m, valorResidual);
            var baseDep = BaseDepreciavel(custo, valorResidual);
            if (baseDep <= 0m) return 0m;

            // saldo ainda depreciável até atingir o residual
            var depreciavelRestante = Math.Max(0m, valorContabilAtual - valorResidual);
            if (depreciavelRestante <= 0m) return 0m;

            decimal cota;
            switch (metodo)
            {
                case ETipoDepreciacaoAtivo.SaldoDecrescente:
                    // saldos decrescentes: taxa aplicada sobre o valor contábil (declinante)
                    if (taxaMensal is not > 0m) return 0m;
                    cota = valorContabilAtual * taxaMensal.Value;
                    break;

                case ETipoDepreciacaoAtivo.SomaDosDigitos:
                    // SYD: fração decrescente (n−k+1)/Σdígitos aplicada sobre a base depreciável
                    if (vidaUtilMeses is not > 0 || periodoCorrente is not > 0) return 0m;
                    var n = vidaUtilMeses.Value;
                    var k = periodoCorrente.Value;
                    if (k > n) return 0m;
                    var somaDigitos = (decimal)n * (n + 1) / 2m;
                    cota = baseDep * (n - k + 1) / somaDigitos;
                    break;

                case ETipoDepreciacaoAtivo.Linear:
                case ETipoDepreciacaoAtivo.Acelerada:
                default:
                    // cotas constantes: base × taxa mensal, ou base ÷ vida útil (meses)
                    if (taxaMensal is > 0m)
                        cota = baseDep * taxaMensal.Value;
                    else if (vidaUtilMeses is > 0)
                        cota = baseDep / vidaUtilMeses.Value;
                    else
                        return 0m;
                    break;
            }

            if (cota < 0m) cota = 0m;
            // nunca deprecia abaixo do residual — última cota vira o saldo depreciável restante
            var ajustada = Math.Min(cota, depreciavelRestante);
            return Math.Round(ajustada, 2, MidpointRounding.AwayFromZero);
        }
    }
}
