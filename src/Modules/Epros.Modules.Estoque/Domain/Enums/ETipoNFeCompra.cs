using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo da NF-e de compra. Porte fiel do legado Epros.ERP.Shared.Enums.ETipoNFeCompra.
    /// </summary>
    public enum ETipoNFeCompra
    {
        [Description("Não Aplicável")]
        NaoAplicavel = -1,

        [Description("Nota Fiscal de Entrada Fornecedor")]
        NotaFiscalEntrada = 0,

        [Description("Nota Fiscal de Entrada Própria")]
        NotaFiscalEntradaPropria = 1,

        [Description("Nota Fiscal de Importação")]
        NotaFiscalImportacao = 2
    }
}
