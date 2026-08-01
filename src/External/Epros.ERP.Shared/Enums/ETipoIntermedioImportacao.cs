using System.ComponentModel;
using System.Xml.Serialization;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoIntermedioImportacao
    {
        //ImportacaoDireta = 1,
        //ContaEOrdem = 2,
        //Encomenda = 3,

        [Description("Conta Própria")]
        ContaPropria = 1,

        [Description(" Conta e Ordem")]
        ContaeOrdem,

        [Description("Por Encomenda")]
        PorEncomenda

    }
}
