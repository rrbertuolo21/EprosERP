using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo de valor lido na balança. Porte fiel do legado Epros.ERP.Shared.Enums.ETipoValorBalanca.
    /// </summary>
    public enum ETipoValorBalanca
    {
        [Description("Peso")]
        Peso = 1,

        [Description("Preço")]
        Preco = 2,
    }
}
