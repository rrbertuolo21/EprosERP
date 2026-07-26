using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Enums do submódulo Logística de Saída (VEN-LDS).
    // Fonte funcional: EF_7_VENDAS_LOGISTICA_DE_SAIDA_V1 (§19). Enums locais do módulo.
    // ============================================================================

    /// <summary>Estado funcional da expedição. EF §19.1 (status).</summary>
    public enum EExpedicaoStatus
    {
        [Description("Rascunho")]
        Rascunho = 0,
        [Description("Confirmado")]
        Confirmado = 1,
        [Description("Faturado")]
        Faturado = 2,
        [Description("Cancelado")]
        Cancelado = 3,
        [Description("Estornado")]
        Estornado = 4
    }
}
