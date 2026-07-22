using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo de reajuste de valor de produto. Porte fiel do legado Epros.ERP.Shared.Enums.ETipoReajuste.
    /// </summary>
    public enum ETipoReajuste
    {
        [Description("Valor de venda")]
        ValorVenda = 0,

        [Description("Valor de compra")]
        Valorcompra = 1
    }
}
