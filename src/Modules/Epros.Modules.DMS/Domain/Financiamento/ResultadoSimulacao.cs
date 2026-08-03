using System.Collections.Generic;

namespace Epros.Modules.DMS.Domain.Financiamento
{
    /// <summary>
    /// Saída do motor de financiamento (skill Negocio-acumulado/financeiro/credito — "O motor em uma frase"):
    /// tabela de parcelas + total pago / total de juros + IOF + CET anual.
    /// </summary>
    public sealed class ResultadoSimulacao
    {
        public decimal Principal { get; init; }
        public decimal TaxaPeriodica { get; init; }
        public int Prazo { get; init; }
        public SistemaAmortizacao Sistema { get; init; }

        /// <summary>Primeira prestação (na Price todas são iguais; no SAC é a maior).</summary>
        public decimal PrimeiraPrestacao { get; init; }
        public decimal TotalPago { get; init; }
        public decimal TotalJuros { get; init; }
        public decimal Iof { get; init; }

        /// <summary>Custo Efetivo Total anualizado (TIR do fluxo do cliente). CET_anual ≥ taxa_juros_anual.</summary>
        public decimal CetAnual { get; init; }
        public decimal TaxaJurosAnual { get; init; }

        public IReadOnlyList<ParcelaFinanciamento> Parcelas { get; init; } = new List<ParcelaFinanciamento>();
    }
}
