using System;

namespace Epros.Modules.Financeiro.Domain.Services
{
    /// <summary>Natureza contábil da posição em moeda estrangeira.</summary>
    public enum ENaturezaPosicaoCambial { Ativo = 0, Passivo = 1 }

    /// <summary>Resultado da variação cambial (mark-to-market) de uma posição.</summary>
    public enum EResultadoCambial { Nulo = 0, Ganho = 1, Perda = 2 }

    /// <summary>
    /// Motor de câmbio (FIN-CAM é o DONO do câmbio) — mark-to-market / variação cambial.
    /// Fórmulas UNIVERSAIS (identidade aritmética): valor em moeda base = valor em ME × cotação;
    /// variação cambial = valor_ME × (cotação_atual − cotação_registro).
    ///
    /// ⚠️ A COTAÇÃO/taxa (PTAX, fechamento) é fato factual de mercado por data → // valida-contador;
    /// chega INFORMADA. O motor só aplica a conversão e classifica ganho/perda; nunca inventa cotação.
    /// </summary>
    public static class CalculoVariacaoCambial
    {
        private static decimal R2(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

        /// <summary>Converte um valor em moeda estrangeira para a moeda base pela cotação informada.</summary>
        public static decimal ConverterParaBase(decimal valorMoedaEstrangeira, decimal cotacao)
        {
            if (cotacao < 0m) throw new ArgumentOutOfRangeException(nameof(cotacao));
            return R2(valorMoedaEstrangeira * cotacao);
        }

        /// <summary>
        /// Variação cambial em moeda base entre a cotação de registro e a atual (mark-to-market).
        /// Positiva = a posição vale mais em reais; negativa = vale menos. A leitura ganho/perda
        /// depende da natureza (ativo × passivo) — ver <see cref="Classificar"/>.
        /// </summary>
        public static decimal Variacao(decimal valorMoedaEstrangeira, decimal cotacaoRegistro, decimal cotacaoAtual)
            => R2(valorMoedaEstrangeira * (cotacaoAtual - cotacaoRegistro));

        /// <summary>
        /// Classifica o resultado cambial. Para um ATIVO (ex.: contas a receber em ME), a valorização
        /// da moeda (variação &gt; 0) é GANHO. Para um PASSIVO (ex.: dívida em ME), a mesma valorização
        /// é PERDA (a dívida cresce em reais). Regra contábil universal — cita Negocio-acumulado/contabil.
        /// </summary>
        public static EResultadoCambial Classificar(decimal variacao, ENaturezaPosicaoCambial natureza)
        {
            if (variacao == 0m) return EResultadoCambial.Nulo;
            var favoravelSeSobe = natureza == ENaturezaPosicaoCambial.Ativo;
            var sobe = variacao > 0m;
            return sobe == favoravelSeSobe ? EResultadoCambial.Ganho : EResultadoCambial.Perda;
        }
    }
}
