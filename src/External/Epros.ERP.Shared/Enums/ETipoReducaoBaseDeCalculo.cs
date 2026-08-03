using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoReducaoBaseDeCalculo
    {
        [Description("Não Utiliza")]
        NaoUtiliza = 0,

        [Description("Em = (-) subtração da base calculo")]
        Em = 1,

        [Description("Para = (*) multiplicação da base calculo")]
        Para = 2
    }
}
