using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ESituacaoDocumentoFiscal
    {
        [Description("Documento regular - Situação Normal")]
        DocumentoRegular = 0,

        [Description("Escrituração extemporânia de documento regular")]
        EscrituracaoExtemporaniaDocumentoRegular = 1,

        [Description("Documento cancelado")]
        DocumentoCancelado = 2,

        [Description("Escrituração extemporânia de documento cancelado")]
        EscrituracaoExtemporaniaDocumentoCancelado = 3,

        [Description("NFe, NFCe ou CTe - Denegado")]
        NfeNfceOuCteDenegado = 4,

        [Description("NFe, NFCe ou CTe - Numeração Inutilizada")]
        NfeNfceOuCteNumeracaoInutilizada = 5,

        [Description("Documento fiscal complementar")]
        DocumentoFiscalComplementar = 6,

        [Description("Escrituração extemporânia de documento complementar")]
        EscrituracaoExtemporaniaDocumentoComplementar = 7,

        [Description("Documento fiscal emitido com base em Regime Especial ou Norma Específica")]
        DocumentoFiscalRegimeEspecialOuNormaEspecifica = 8,
    }
}
