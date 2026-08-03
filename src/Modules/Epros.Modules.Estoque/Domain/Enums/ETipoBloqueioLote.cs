using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Motivo do bloqueio do lote (EF Rastreabilidade §7.6 `rlt_bloqueio_lote.tipo_bloqueio`).</summary>
    public enum ETipoBloqueioLote
    {
        [Description("Qualidade")] Qualidade = 0,
        [Description("Recall")] Recall = 1,
        [Description("Validade")] Validade = 2,
        [Description("Manual")] Manual = 3
    }
}
