using System.Collections.Generic;
using System.Linq;

namespace Epros.Modules.Qualidade.Domain.Services.Qps
{
    /// <summary>Indicador que alimenta o score (valor 0..100 esperado, com peso relativo). Rastreavel a rejeicoes/NCR.</summary>
    public readonly struct IndicadorScore
    {
        public string Codigo { get; }
        public decimal Valor { get; }
        public decimal Peso { get; }
        public IndicadorScore(string codigo, decimal valor, decimal peso) { Codigo = codigo; Valor = valor; Peso = peso; }
    }

    public sealed class ResultadoScore
    {
        public decimal Score { get; }
        public bool AbaixoLimite { get; }
        public decimal LimiteBloqueio { get; }
        public ResultadoScore(decimal score, bool abaixoLimite, decimal limiteBloqueio)
        { Score = score; AbaixoLimite = abaixoLimite; LimiteBloqueio = limiteBloqueio; }
    }

    /// <summary>
    /// Motor de score de fornecedor (QLD-QPS). PARAMETRIZAVEL: a fabrica entrega a mecanica (media
    /// ponderada dos indicadores normalizada 0..100); a FORMULA e os PESOS especificos (PPM, OTIF-qualidade,
    /// peso de NCR, limite de bloqueio) sao POLITICA DA SISER + skill canonica — NAO inventados aqui (D14).
    ///
    /// // valida (homologacao Siser — formula/pesos/limiar): os pesos e o limiar chegam por parametro
    /// (qld_qps_parametro); este motor apenas os aplica. GAP de negocio registrado no dossie (D14).
    /// </summary>
    public sealed class MotorScoreFornecedor
    {
        /// <summary>Limiar default de bloqueio quando o tenant nao parametrizou (conservador; sugere, nao bloqueia sozinho).</summary>
        public const decimal LimiteBloqueioDefault = 60m;

        /// <summary>
        /// Calcula o score ponderado (0..100). Se a soma dos pesos for 0, retorna a media simples;
        /// sem indicadores, retorna 0. abaixoLimite = score &lt; limiteBloqueio (parametro).
        /// </summary>
        public ResultadoScore Calcular(IEnumerable<IndicadorScore> indicadores, decimal? limiteBloqueio = null)
        {
            var limite = limiteBloqueio ?? LimiteBloqueioDefault;
            var lista = indicadores?.ToList() ?? new List<IndicadorScore>();
            if (lista.Count == 0)
                return new ResultadoScore(0m, 0m < limite, limite);

            var somaPesos = lista.Sum(i => i.Peso);
            decimal score = somaPesos > 0
                ? lista.Sum(i => i.Valor * i.Peso) / somaPesos
                : lista.Average(i => i.Valor);

            score = decimal.Round(score, 2);
            return new ResultadoScore(score, score < limite, limite);
        }
    }
}
