using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Indica se o valor do item compõe o total da NF-e. Porte fiel do legado Epros.ERP.Shared.Enums.EIndicadorTotalizador.
    /// </summary>
    public enum EIndicadorTotalizador
    {
        [Description("Valor do item não compõe o total da NF-e")]
        ValorDoItemNaoCompoeTotalNF = 0,

        [Description("Valor do item compõe o total da NF-e")]
        ValorDoItemCompoeTotalNF = 1
    }
}
