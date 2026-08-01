using System.ComponentModel;
using System.Xml.Serialization;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoNFe
    {
        [Description("Entrada")]
        [XmlEnum("0")]
        tnEntrada,
        [Description("Saída")]
        [XmlEnum("1")]
        tnSaida
    }
}
