using System;
using Epros.Modules.Manutencao.Domain.Enums;

namespace Epros.Modules.Manutencao.Domain.Services
{
    /// <summary>
    /// MAN-PDT — Motor de avaliacao preditiva (D11/D12). Funcao PURA:
    /// - ValidarLeitura: unidade, sequencia temporal, duplicidade e qualidade (D12 -> LEITURA_INVALIDA).
    /// - RegraDispara: avalia automaticamente regra de LIMITE contra a leitura (D11).
    /// Tendencia/Desvio/IA = fase 2 (LAC-PDT-003) -> nao dispara automaticamente em V1.
    /// </summary>
    public static class MotorAvaliacaoPreditiva
    {
        public const decimal QualidadeMinimaDefault = 0.5m;

        public sealed class ResultadoValidacao
        {
            public bool Valida { get; init; }
            public string? Motivo { get; init; }
            public static ResultadoValidacao Ok() => new() { Valida = true };
            public static ResultadoValidacao Invalida(string motivo) => new() { Valida = false, Motivo = motivo };
        }

        public static ResultadoValidacao ValidarLeitura(
            string unidadeLeitura,
            string unidadePonto,
            decimal? qualidade,
            DateTime dataMedicao,
            DateTime? ultimaData,
            bool duplicada,
            decimal qualidadeMinima)
        {
            if (!UnidadesCompativeis(unidadeLeitura, unidadePonto))
                return ResultadoValidacao.Invalida($"LEITURA_INVALIDA: unidade divergente (leitura '{unidadeLeitura}' x ponto '{unidadePonto}').");

            if (duplicada)
                return ResultadoValidacao.Invalida("LEITURA_INVALIDA: leitura duplicada (mesmo ponto e instante).");

            if (ultimaData.HasValue && dataMedicao < ultimaData.Value)
                return ResultadoValidacao.Invalida("LEITURA_INVALIDA: fora de sequencia temporal (anterior a ultima leitura).");

            if (qualidade.HasValue && qualidade.Value < qualidadeMinima)
                return ResultadoValidacao.Invalida($"LEITURA_INVALIDA: qualidade insuficiente ({qualidade.Value} < {qualidadeMinima}).");

            return ResultadoValidacao.Ok();
        }

        private static bool UnidadesCompativeis(string a, string b)
        {
            if (string.IsNullOrWhiteSpace(a) || string.IsNullOrWhiteSpace(b)) return true; // sem base para comparar
            return string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Avalia se a leitura dispara a regra. Somente regras de LIMITE sao avaliadas em V1.
        /// Operadores: ">", "&gt;=", "&lt;", "&lt;=", "fora"/"outside" (fora da faixa), "entre"/"faixa" (dentro da faixa).
        /// </summary>
        public static bool RegraDispara(decimal valor, ETipoRegraMonitoramento tipo, string? operador, decimal? limiteMinimo, decimal? limiteMaximo)
        {
            if (tipo != ETipoRegraMonitoramento.Limite) return false;

            var op = (operador ?? string.Empty).Trim().ToLowerInvariant();
            switch (op)
            {
                case ">":
                    {
                        var t = limiteMaximo ?? limiteMinimo;
                        return t.HasValue && valor > t.Value;
                    }
                case ">=":
                    {
                        var t = limiteMaximo ?? limiteMinimo;
                        return t.HasValue && valor >= t.Value;
                    }
                case "<":
                    {
                        var t = limiteMinimo ?? limiteMaximo;
                        return t.HasValue && valor < t.Value;
                    }
                case "<=":
                    {
                        var t = limiteMinimo ?? limiteMaximo;
                        return t.HasValue && valor <= t.Value;
                    }
                case "fora":
                case "outside":
                    return (limiteMinimo.HasValue && valor < limiteMinimo.Value)
                        || (limiteMaximo.HasValue && valor > limiteMaximo.Value);
                case "entre":
                case "faixa":
                case "between":
                    return limiteMinimo.HasValue && limiteMaximo.HasValue
                        && valor >= limiteMinimo.Value && valor <= limiteMaximo.Value;
                default:
                    // Sem operador explicito: se ha faixa, dispara quando fora dela.
                    if (limiteMinimo.HasValue || limiteMaximo.HasValue)
                        return (limiteMinimo.HasValue && valor < limiteMinimo.Value)
                            || (limiteMaximo.HasValue && valor > limiteMaximo.Value);
                    return false;
            }
        }
    }
}
