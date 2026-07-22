
using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoIndicadorIe
    {
        [Description("Contribuinte ICMS")]
        ContribuinteICMS = 1,

        [Description("Contribuinte isento de inscrição")]
        Isento = 2,

        [Description("Não Contribuinte")]
        NaoContribuinte = 9
    }
}
