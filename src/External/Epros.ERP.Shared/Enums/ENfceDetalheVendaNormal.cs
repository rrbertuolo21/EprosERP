using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ENfceDetalheVendaNormal
    {
        [Description("Não Imprimir")]
        NaoImprimir = 0,

        [Description("Uma Linha")]
        UmaLinha = 1,

        [Description("Duas Linha")]
        DuasLinhas = 2,

        Completo = 3
    }
}
