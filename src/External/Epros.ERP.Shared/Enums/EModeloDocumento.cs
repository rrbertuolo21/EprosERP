using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
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
