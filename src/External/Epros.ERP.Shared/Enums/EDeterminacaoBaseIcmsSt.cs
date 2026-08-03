using System.ComponentModel;
using System.Xml.Serialization;

namespace Epros.ERP.Shared.Enums
{
    public enum EDeterminacaoBaseIcmsSt
    {
        [Description("Preço tabelado ou máximo  sugerido")]
        [XmlEnum("0")]
        DbisPrecoTabelado,
        [Description("Lista Negativa (valor)")]
        [XmlEnum("1")]
        DbisListaNegativa,
        [Description("Lista Positiva (valor)")]
        [XmlEnum("2")]
        DbisListaPositiva,
        [Description("Lista Neutra (valor)")]
        [XmlEnum("3")]
        DbisListaNeutra,
        [Description("Margem Valor Agregado (%)")]
        [XmlEnum("4")]
        DbisMargemValorAgregado,
        [Description("Pauta (valor)")]
        [XmlEnum("5")]
        DbisPauta,
        [Description("Valor da Operação")]
        [XmlEnum("6")]
        DbisValordaOperacao
    }
}
