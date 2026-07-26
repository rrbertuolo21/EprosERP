using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo do XML importado. Porte fiel do legado Epros.ERP.Shared.Enums.ETipoXml.
    /// </summary>
    public enum ETipoXml
    {
        [Description("Não Aplicável")]
        NaoAplicavel = -1,

        [Description("Nota Fiscal de Entrada")]
        NotaFiscalEntrada = 1,

        [Description("Nota Fiscal de Saída")]
        NotaFiscalSaida = 2,

        [Description("Nota Fiscal de Entrada Própria")]
        NotaFiscalEntradaPropria = 3,

        [Description("Nota Fiscal de Cancelamento")]
        NotaFiscalCancelamento = 4,

        [Description("Nota Fiscal de Importação")]
        NotaFiscalImportacao = 5
    }
}
