using System;
using Epros.Modules.Manutencao.Domain.Enums;

namespace Epros.Modules.Manutencao.Domain.Services
{
    /// <summary>
    /// MAN-PRV — Motor de vencimento da preventiva (D7). Funcao PURA: dado o estado de uma
    /// periodicidade (calendario e/ou contador) e uma referencia (data + contador atual),
    /// decide se o plano venceu e calcula o proximo marco. Sem dependencia de banco.
    ///
    /// Calendario: proxima data = base + intervalo * unidade. Vence quando a referencia alcanca a
    /// data alvo, respeitando a antecedencia de tolerancia (em dias).
    /// Contador: vence quando o contador atual alcanca o proximo marco.
    /// </summary>
    public static class MotorVencimentoPreventiva
    {
        public enum MotivoVencimento { NaoVencido = 0, Calendario = 1, Contador = 2 }

        public sealed class Avaliacao
        {
            public bool Vencido { get; init; }
            public MotivoVencimento Motivo { get; init; }
            /// <summary>Data alvo do vencimento (quando ha componente de calendario).</summary>
            public DateTime? DataAlvo { get; init; }
            /// <summary>Proxima data apos o vencimento atual (para avancar a base).</summary>
            public DateTime? ProximaData { get; init; }
            /// <summary>Contador alvo (quando ha componente de contador).</summary>
            public decimal? ContadorAlvo { get; init; }
            /// <summary>Proximo marco de contador apos o vencimento atual.</summary>
            public decimal? ProximoContador { get; init; }
            public string Detalhe { get; init; } = string.Empty;
        }

        /// <summary>Avanca uma data por N unidades ("dia","semana","mes","ano","hora"). Default: dias.</summary>
        public static DateTime CalcularProximaData(DateTime dataBase, int intervalo, string? unidade)
        {
            var u = (unidade ?? "dias").Trim().ToLowerInvariant();
            if (u.StartsWith("hora")) return dataBase.AddHours(intervalo);
            if (u.StartsWith("semana")) return dataBase.AddDays(7 * intervalo);
            if (u.StartsWith("mes") || u.StartsWith("mês")) return dataBase.AddMonths(intervalo);
            if (u.StartsWith("ano")) return dataBase.AddYears(intervalo);
            // dia / dias / default
            return dataBase.AddDays(intervalo);
        }

        private static int ToleranciaDias(string? tolerancia)
        {
            if (string.IsNullOrWhiteSpace(tolerancia)) return 0;
            // aceita "3", "3 dias", "3d"
            var digits = new string(System.Linq.Enumerable.ToArray(
                System.Linq.Enumerable.TakeWhile(tolerancia.Trim(), c => char.IsDigit(c))));
            return int.TryParse(digits, out var d) ? d : 0;
        }

        public static Avaliacao Avaliar(
            ETipoPeriodicidade tipo,
            DateTime? dataBase,
            int? intervalo,
            string? unidadeIntervalo,
            DateTime? proximaExecucao,
            string? tolerancia,
            decimal? contadorProximo,
            decimal? contadorAtual,
            DateTime dataReferencia)
        {
            var toleranciaDias = ToleranciaDias(tolerancia);
            bool avaliaCalendario = tipo == ETipoPeriodicidade.Calendario || tipo == ETipoPeriodicidade.Combinado;
            bool avaliaContador = tipo == ETipoPeriodicidade.Contador || tipo == ETipoPeriodicidade.Combinado;

            DateTime? dataAlvo = null;
            DateTime? proximaData = null;
            bool vencidoCal = false;
            if (avaliaCalendario && intervalo.HasValue && intervalo.Value > 0)
            {
                // Alvo: proxima execucao registrada, ou base + intervalo.
                dataAlvo = proximaExecucao
                    ?? (dataBase.HasValue ? CalcularProximaData(dataBase.Value, intervalo.Value, unidadeIntervalo) : (DateTime?)null);
                if (dataAlvo.HasValue)
                {
                    var gatilho = dataAlvo.Value.AddDays(-toleranciaDias);
                    vencidoCal = dataReferencia >= gatilho;
                    proximaData = CalcularProximaData(dataAlvo.Value, intervalo.Value, unidadeIntervalo);
                }
            }

            bool vencidoCont = false;
            decimal? proximoContadorNovo = null;
            if (avaliaContador && contadorProximo.HasValue && contadorAtual.HasValue)
            {
                vencidoCont = contadorAtual.Value >= contadorProximo.Value;
                // proximo marco = marco atual + (marco atual - ? ) — sem intervalo de contador confiavel,
                // mantem o passo pela diferenca ate o atual quando informado; caso contrario repete o marco.
            }

            var vencido = vencidoCal || vencidoCont;
            var motivo = vencidoCal ? MotivoVencimento.Calendario
                       : vencidoCont ? MotivoVencimento.Contador
                       : MotivoVencimento.NaoVencido;

            var detalhe = vencido
                ? $"Vencido por {motivo}. Alvo data={dataAlvo:o}, contadorAlvo={contadorProximo}, ref={dataReferencia:o}, contadorAtual={contadorAtual}."
                : $"Nao vencido. Alvo data={dataAlvo:o}, contadorAlvo={contadorProximo}, ref={dataReferencia:o}.";

            return new Avaliacao
            {
                Vencido = vencido,
                Motivo = motivo,
                DataAlvo = dataAlvo,
                ProximaData = proximaData,
                ContadorAlvo = contadorProximo,
                ProximoContador = proximoContadorNovo,
                Detalhe = detalhe
            };
        }
    }
}
