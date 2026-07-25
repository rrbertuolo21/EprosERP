using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
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
