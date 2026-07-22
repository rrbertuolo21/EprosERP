using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Indicador da Inscrição Estadual do destinatário. Porte fiel do legado
    /// Epros.ERP.Shared.Enums.ETipoIndicadorIe.
    /// </summary>
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
