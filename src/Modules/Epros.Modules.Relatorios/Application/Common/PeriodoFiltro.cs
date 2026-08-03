using System;

namespace Epros.Modules.Relatorios.Application.Common
{
    /// <summary>
    /// Contrato funcional de filtro por periodo reutilizado pelos relatorios operacionais e de BI.
    /// Corresponde a EF RELATORIOS/RELATORIOS_OPERACIONAIS secao 11.2 (Filtro por periodo).
    ///
    /// Decisoes de fork (default aplicado; registrado em DECISOES-PENDENTES-RAFAEL.md):
    /// - Inclusividade: intervalo [inicio, fim) semi-aberto quando ambos informados; quando so ha
    ///   data (sem hora) o fim e tratado ate o fim do dia informado (fim.Date + 1 dia). Aqui usamos
    ///   comparacao direta >= inicio e &lt;= fim para preservar o comportamento simples do legado
    ///   (EF RN-013 sugere D-1 ate agora em UTC).
    /// - Fuso: UTC (RN-013). A camada de API recebe as datas ja normalizadas.
    /// </summary>
    public sealed class PeriodoFiltro
    {
        public DateTime? Inicio { get; }
        public DateTime? Fim { get; }

        public PeriodoFiltro(DateTime? inicio, DateTime? fim)
        {
            Inicio = inicio;
            Fim = fim;
        }

        public bool TemInicio => Inicio.HasValue;
        public bool TemFim => Fim.HasValue;

        /// <summary>Default RN-013: quando nada e informado, sugere D-1 (UTC) ate agora.</summary>
        public static PeriodoFiltro PadraoDiaAnterior()
            => new PeriodoFiltro(DateTime.UtcNow.AddDays(-1), DateTime.UtcNow);
    }
}
