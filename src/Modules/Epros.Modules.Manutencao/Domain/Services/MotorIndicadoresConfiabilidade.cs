using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.Manutencao.Domain.Services
{
    /// <summary>
    /// MAN-CRV/MAN-PAR — Motor unico de indicadores de confiabilidade (D16/D24).
    /// Funcao PURA (sem dependencia de banco): recebe as paradas do equipamento no periodo e
    /// calcula MTTR, MTBF e Disponibilidade de forma auditavel (formula + fontes).
    ///
    /// Convencoes:
    /// - Trabalha em MINUTOS internamente; expoe MTTR/MTBF em HORAS (padrao de engenharia) e
    ///   Disponibilidade em fracao 0..1 (e percentual).
    /// - "Falha" = parada NAO planejada finalizada (tem fim). Parada planejada/setup conta como
    ///   indisponibilidade (afeta Disponibilidade) mas NAO como falha (nao afeta MTTR/MTBF).
    /// - CA-008: MTBF/MTTR sao NULL quando nao ha falha concluida (nao se inventa valor).
    /// </summary>
    public static class MotorIndicadoresConfiabilidade
    {
        /// <summary>Entrada minima de parada para o calculo.</summary>
        public readonly struct ParadaCalculo
        {
            public DateTime Inicio { get; }
            public DateTime? Fim { get; }
            /// <summary>True quando a parada representa uma falha (nao planejada/corretiva).</summary>
            public bool EhFalha { get; }

            public ParadaCalculo(DateTime inicio, DateTime? fim, bool ehFalha)
            {
                Inicio = inicio;
                Fim = fim;
                EhFalha = ehFalha;
            }
        }

        public sealed class ResultadoIndicadores
        {
            public DateTime PeriodoInicio { get; init; }
            public DateTime PeriodoFim { get; init; }
            public decimal TempoCalendarioMinutos { get; init; }
            public decimal DowntimeTotalMinutos { get; init; }
            public decimal DowntimeFalhasMinutos { get; init; }
            public decimal TempoOperacionalMinutos { get; init; }
            public int QuantidadeFalhas { get; init; }
            /// <summary>MTTR em horas (null se nao ha falha concluida).</summary>
            public decimal? MttrHoras { get; init; }
            /// <summary>MTBF em horas (null se nao ha falha concluida).</summary>
            public decimal? MtbfHoras { get; init; }
            /// <summary>Disponibilidade em fracao 0..1.</summary>
            public decimal Disponibilidade { get; init; }
            /// <summary>Disponibilidade em percentual 0..100.</summary>
            public decimal DisponibilidadePercentual { get; init; }
        }

        public const string FormulaMttr = "MTTR = SOMA(tempo de reparo das falhas concluidas) / QTD(falhas concluidas)";
        public const string FormulaMtbf = "MTBF = tempo operacional (calendario - indisponibilidade) / QTD(falhas)";
        public const string FormulaDisponibilidade = "Disponibilidade = tempo operacional / tempo calendario";

        public static ResultadoIndicadores Calcular(
            IEnumerable<ParadaCalculo> paradas,
            DateTime periodoInicio,
            DateTime periodoFim)
        {
            if (paradas == null) throw new ArgumentNullException(nameof(paradas));
            if (periodoFim <= periodoInicio)
                throw new ArgumentException("O fim do periodo deve ser posterior ao inicio.", nameof(periodoFim));

            var tempoCalendarioMin = (decimal)(periodoFim - periodoInicio).TotalMinutes;

            decimal downtimeTotalMin = 0m;
            decimal downtimeFalhasMin = 0m;
            int qtdFalhas = 0;

            foreach (var p in paradas)
            {
                // So considera paradas finalizadas para reparo/indisponibilidade mensuravel.
                if (!p.Fim.HasValue) continue;

                var duracaoMin = MinutosNoPeriodo(p.Inicio, p.Fim.Value, periodoInicio, periodoFim);
                if (duracaoMin <= 0m) continue;

                downtimeTotalMin += duracaoMin;
                if (p.EhFalha)
                {
                    downtimeFalhasMin += duracaoMin;
                    qtdFalhas++;
                }
            }

            if (downtimeTotalMin > tempoCalendarioMin) downtimeTotalMin = tempoCalendarioMin;
            var tempoOperacionalMin = tempoCalendarioMin - downtimeTotalMin;
            if (tempoOperacionalMin < 0m) tempoOperacionalMin = 0m;

            decimal? mttrHoras = qtdFalhas > 0 ? Arredondar(downtimeFalhasMin / qtdFalhas / 60m) : (decimal?)null;
            decimal? mtbfHoras = qtdFalhas > 0 ? Arredondar(tempoOperacionalMin / qtdFalhas / 60m) : (decimal?)null;

            var disponibilidade = tempoCalendarioMin > 0m ? tempoOperacionalMin / tempoCalendarioMin : 0m;

            return new ResultadoIndicadores
            {
                PeriodoInicio = periodoInicio,
                PeriodoFim = periodoFim,
                TempoCalendarioMinutos = Arredondar(tempoCalendarioMin),
                DowntimeTotalMinutos = Arredondar(downtimeTotalMin),
                DowntimeFalhasMinutos = Arredondar(downtimeFalhasMin),
                TempoOperacionalMinutos = Arredondar(tempoOperacionalMin),
                QuantidadeFalhas = qtdFalhas,
                MttrHoras = mttrHoras,
                MtbfHoras = mtbfHoras,
                Disponibilidade = Arredondar(disponibilidade),
                DisponibilidadePercentual = Arredondar(disponibilidade * 100m)
            };
        }

        // Interseccao [inicio,fim] da parada com [periodoInicio,periodoFim], em minutos.
        private static decimal MinutosNoPeriodo(DateTime inicio, DateTime fim, DateTime periodoInicio, DateTime periodoFim)
        {
            var ini = inicio < periodoInicio ? periodoInicio : inicio;
            var f = fim > periodoFim ? periodoFim : fim;
            if (f <= ini) return 0m;
            return (decimal)(f - ini).TotalMinutes;
        }

        private static decimal Arredondar(decimal v) => Math.Round(v, 4, MidpointRounding.AwayFromZero);
    }
}
