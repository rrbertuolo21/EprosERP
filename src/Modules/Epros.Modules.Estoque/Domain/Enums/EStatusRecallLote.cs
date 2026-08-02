using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Status do recall (EF Rastreabilidade §7.7 `rlt_recall_lote.status`).</summary>
    public enum EStatusRecallLote
    {
        [Description("Aberto")] Aberto = 0,
        [Description("Em andamento")] EmAndamento = 1,
        [Description("Concluído")] Concluido = 2,
        [Description("Cancelado")] Cancelado = 3
    }
}
