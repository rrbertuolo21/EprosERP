using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Modelo do documento fiscal. Porte fiel do legado Epros.ERP.Shared.Enums.EModeloDocumento.
    /// </summary>
    public enum EModeloDocumento
    {
        [Description("NF-e")]
        NFe = 55,

        [Description("CT-e")]
        CTe = 57,

        [Description("MDF-e")]
        MDFe = 58,

        [Description("CF-e")]
        CFe = 59,

        [Description("NFC-e")]
        NFCe = 65,

        [Description("CTe-OS")]
        CTeOS = 67,

        [Description("Documento Auxiliar Interno")]
        DAI = 99
    }
}
