using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo de operação da NF-e (entrada/saída). Porte fiel do legado Epros.ERP.Shared.Enums.ETipoOperacaoNfe.
    /// </summary>
    public enum ETipoOperacaoNfe
    {
        Entrada = 0,

        [Description("Saída")]
        Saida = 1
    }
}
