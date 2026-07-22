using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo de endereço. Porte fiel do legado Epros.ERP.Shared.Enums.ETipoEndereco.
    /// </summary>
    public enum ETipoEndereco
    {
        Principal = 1, // Comercial e Residencial

        [Description("Cobrança")]
        Cobranca,

        Entrega,

        Obra
    }
}
