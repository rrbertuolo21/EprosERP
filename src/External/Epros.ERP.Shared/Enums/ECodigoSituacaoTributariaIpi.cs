using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ECodigoSituacaoTributariaIpi
    {
        [Description("00 - Entrada com Recuperação de Crédito")]
        Cst00 = 00,
        [Description("01 - Entrada Tributada com Alíquota Zero")]
        Cst01 = 01,
        [Description("02 - Entrada Isenta")]
        Cst02 = 02,
        [Description("03 - Entrada Não Tributada")]
        Cst03 = 03,
        [Description("04 - Entrada Imune")]
        Cst04 = 04,
        [Description("05 - Entrada com Suspensão")]
        Cst05 = 05,
        [Description("49 - Outras Entradas")]
        Cst49 = 49,
        [Description("50 - Saída Tributada")]
        Cst50 = 50,
        [Description("51 - Saída Tributável com Alíquota Zero")]
        Cst51 = 51,
        [Description("52 - Saída Isenta")]
        Cst52 = 52,
        [Description("53 - Saída Não-Tributada")]
        Cst53 = 53,
        [Description("54 - Saída Imune")]
        Cst54 = 54,
        [Description("55 - Saída com Suspensão")]
        Cst55 = 55,
        [Description("99 - Outras Saídas")]
        Cst99 = 99
    }
}
