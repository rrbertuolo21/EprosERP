using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EIndicadorTotalizador
    {
        [Description("Valor do item não compõe o total da NF-e")]
        ValorDoItemNaoCompoeTotalNF = 0,
        [Description("Valor do item compõe o total da NF-e")]
        ValorDoItemCompoeTotalNF = 1
    }
}
