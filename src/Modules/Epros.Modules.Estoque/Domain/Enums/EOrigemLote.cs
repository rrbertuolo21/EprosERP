using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>Origem operacional do lote (EF Rastreabilidade §7.2 `rlt_lote.origem`).</summary>
    public enum EOrigemLote
    {
        [Description("Compra")] Compra = 0,
        [Description("Produção")] Producao = 1,
        [Description("Ajuste")] Ajuste = 2,
        [Description("Importação")] Importacao = 3,
        [Description("Manual")] Manual = 4
    }
}
