using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo de intermédio da importação. Porte fiel do legado
    /// Epros.ERP.Shared.Enums.ETipoIntermedioImportacao.
    /// </summary>
    public enum ETipoIntermedioImportacao
    {
        [Description("Conta Própria")]
        ContaPropria = 1,

        [Description(" Conta e Ordem")]
        ContaeOrdem,

        [Description("Por Encomenda")]
        PorEncomenda
    }
}
