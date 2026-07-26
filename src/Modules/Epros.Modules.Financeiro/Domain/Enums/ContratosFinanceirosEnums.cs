using System.ComponentModel;

namespace Epros.Modules.Financeiro.Domain.Enums
{
    /// <summary>Periodicidade de cobrança do plano/contrato (EF FIN-GCF §13.1).</summary>
    public enum EPeriodicidadeContrato
    {
        [Description("Mensal")] Mensal = 0,
        [Description("Bimestral")] Bimestral = 1,
        [Description("Trimestral")] Trimestral = 2,
        [Description("Semestral")] Semestral = 3,
        [Description("Anual")] Anual = 4
    }

    /// <summary>Estado do plano de contrato (EF FIN-GCF §8).</summary>
    public enum EStatusPlanoContrato
    {
        [Description("Ativo")] Ativo = 0,
        [Description("Inativo")] Inativo = 1
    }

    /// <summary>Estado operacional do contrato financeiro (EF FIN-GCF §8 / §13.2).</summary>
    public enum EStatusContratoFinanceiro
    {
        [Description("Ativo")] Ativo = 0,
        [Description("Suspenso")] Suspenso = 1,
        [Description("Cancelado")] Cancelado = 2
    }

    /// <summary>Estado da fatura recorrente (EF FIN-GCF §13.3).</summary>
    public enum EStatusFaturaRecorrente
    {
        [Description("Pendente")] Pendente = 0,
        [Description("Gerada")] Gerada = 1,
        [Description("Cancelada")] Cancelada = 2
    }
}
