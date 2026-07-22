using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Regime tributário do emitente. Porte fiel do legado Epros.ERP.Shared.Enums.ERegimeTributario.
    /// </summary>
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
