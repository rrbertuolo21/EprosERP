using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Status do lote rastreável (EF Rastreabilidade §7.2 `rlt_lote.status`).</summary>
    public enum EStatusLoteRastreabilidade
    {
        [Description("Ativo")] Ativo = 0,
        [Description("Bloqueado")] Bloqueado = 1,
        [Description("Consumido")] Consumido = 2,
        [Description("Vencido")] Vencido = 3,
        [Description("Cancelado")] Cancelado = 4
    }
}
