using System;
using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.Aplicativo.Application.Services
{
    /// <summary>
    /// Serviço dedicado de cálculo de próximas execuções de agendamentos de workflow (PLT-WF §7.4.3).
    /// Interpreta a expressão intervalar funcional <c>mins::hours::day_of_month::months::day_of_week</c>.
    /// Cada segmento aceita: <c>*</c> (qualquer), inteiro, lista (<c>1,15,30</c>) ou passo (<c>*/5</c>).
    /// [Origem: EF WORKFLOW 7.4 / 10.10]
    /// </summary>
    public interface IAgendaIntervalarService
    {
        /// <summary>Valida a expressão intervalar (5 segmentos e faixas coerentes).</summary>
        bool ExpressaoValida(string expressao, out string? erro);

        /// <summary>Calcula a próxima execução estritamente após <paramref name="apos"/> (UTC). Nulo se não houver.</summary>
        DateTime? ProximaExecucao(string expressao, DateTime apos);
    }

    public class AgendaIntervalarService : IAgendaIntervalarService
    {
        // Limite de varredura para evitar laço infinito em expressões impossíveis (ex.: 31 de fevereiro).
        private const int LimiteMinutosVarredura = 400 * 24 * 60;

        public bool ExpressaoValida(string expressao, out string? erro)
        {
            erro = null;
            if (string.IsNullOrWhiteSpace(expressao))
            {
                erro = "A expressão intervalar é obrigatória.";
                return false;
            }

            var seg = expressao.Split("::");
            if (seg.Length != 5)
            {
                erro = "A expressão intervalar deve ter 5 segmentos (mins::hours::day_of_month::months::day_of_week).";
                return false;
            }

            // mins 0-59, hours 0-23, dom 1-31, months 1-12, dow 0-6
            if (!SegmentoValido(seg[0], 0, 59)) { erro = "Segmento de minutos inválido (0-59)."; return false; }
            if (!SegmentoValido(seg[1], 0, 23)) { erro = "Segmento de horas inválido (0-23)."; return false; }
            if (!SegmentoValido(seg[2], 1, 31)) { erro = "Segmento de dia do mês inválido (1-31)."; return false; }
            if (!SegmentoValido(seg[3], 1, 12)) { erro = "Segmento de mês inválido (1-12)."; return false; }
            if (!SegmentoValido(seg[4], 0, 6)) { erro = "Segmento de dia da semana inválido (0-6)."; return false; }
            return true;
        }

        public DateTime? ProximaExecucao(string expressao, DateTime apos)
        {
            if (!ExpressaoValida(expressao, out _)) return null;

            var seg = expressao.Split("::");
            var mins = ExpandirSegmento(seg[0], 0, 59);
            var hours = ExpandirSegmento(seg[1], 0, 23);
            var dom = ExpandirSegmento(seg[2], 1, 31);
            var months = ExpandirSegmento(seg[3], 1, 12);
            var dow = ExpandirSegmento(seg[4], 0, 6);

            var domRestrito = seg[2].Trim() != "*";
            var dowRestrito = seg[4].Trim() != "*";

            // Começa no próximo minuto cheio após 'apos' (UTC), zerando segundos/fração.
            var candidato = new DateTime(apos.Year, apos.Month, apos.Day, apos.Hour, apos.Minute, 0, DateTimeKind.Utc).AddMinutes(1);

            for (var i = 0; i < LimiteMinutosVarredura; i++, candidato = candidato.AddMinutes(1))
            {
                if (!months.Contains(candidato.Month)) continue;
                if (!hours.Contains(candidato.Hour)) continue;
                if (!mins.Contains(candidato.Minute)) continue;

                var diaMesOk = dom.Contains(candidato.Day);
                var diaSemanaOk = dow.Contains((int)candidato.DayOfWeek);

                // Semântica cron: quando ambos restritos, casa se qualquer um casar (OR); senão, AND.
                bool diaOk = (domRestrito && dowRestrito) ? (diaMesOk || diaSemanaOk) : (diaMesOk && diaSemanaOk);
                if (!diaOk) continue;

                return candidato;
            }

            return null;
        }

        private static bool SegmentoValido(string parte, int min, int max)
        {
            try { return ExpandirSegmento(parte, min, max).Count > 0; }
            catch { return false; }
        }

        private static HashSet<int> ExpandirSegmento(string parte, int min, int max)
        {
            parte = parte.Trim();
            var valores = new HashSet<int>();

            foreach (var token in parte.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                var t = token.Trim();
                if (t == "*")
                {
                    for (var v = min; v <= max; v++) valores.Add(v);
                }
                else if (t.StartsWith("*/"))
                {
                    if (!int.TryParse(t.AsSpan(2), out var passo) || passo <= 0)
                        throw new FormatException("Passo inválido.");
                    for (var v = min; v <= max; v += passo) valores.Add(v);
                }
                else if (int.TryParse(t, out var n))
                {
                    if (n < min || n > max) throw new FormatException("Valor fora da faixa.");
                    valores.Add(n);
                }
                else
                {
                    throw new FormatException("Token inválido.");
                }
            }

            return valores;
        }
    }
}
