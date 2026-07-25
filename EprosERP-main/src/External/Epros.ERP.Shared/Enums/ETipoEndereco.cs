using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoEndereco
    {
        Principal = 1, // Comercial e Residencial
        [Description("Cobrança")]
        Cobranca,
        Entrega,
        Obra
    }
}