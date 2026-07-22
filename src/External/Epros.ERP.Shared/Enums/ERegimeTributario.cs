using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ERegimeTributario
    {
        [Description("Não Utiliza")]
        NaoUtiliza = -1,

        [Description("Simples Nacional")]
        SimplesNacional = 1,

        [Description("Simples Nacional – excesso de sublimite de receita bruta")]
        SimplesNacionalExcessoSublimite = 2,

        [Description("Regime Normal")]
        RegimeNormal = 3,

        [Description("Simples Nacional MEI")]
        SimplesNacionalMei = 4
    }
}
