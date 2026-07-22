using System.ComponentModel;

namespace Epros.Shared.Domain.Enums
{
    public enum ECodigoSituacaoTributariaIcms
    {
        [Description("00 - Tributada integralmente")]
        Cst00 = 0,
        [Description("02 - Tributação Monofásica própria sobre Combustíveis")]
        Cst02 = 2,
        [Description("10 - Tributada e com cobrança do ICMS por substituição tributária")]
        Cst10 = 10,
        [Description("12 - Tributada com ICMS devido por substituição tributária relativo às operações e prestações antecedentes")]
        Cst12 = 12,
        [Description("13 - Tributada com ICMS devido por substituição tributária relativo às operações e prestações concomitante")]
        Cst13 = 13,
        [Description("15 - Tributação Monofásica própria e com responsabilidade pela retenção sobre Combustíveise")]
        Cst15 = 15,
        [Description("20 - Com redução de base de cálculo")]
        Cst20 = 20,
        [Description("30 - Isenta ou não tributada e com cobrança do ICMS por substituição tributária")]
        Cst30 = 30,
        [Description("40 - Isenta")]
        Cst40 = 40,
        [Description("41 - Não tributada")]
        Cst41 = 41,
        [Description("50 - Suspensão")]
        Cst50 = 50,
        [Description("51 - Diferimento")]
        Cst51 = 51,
        [Description("52 - Diferimento com ICMS devido por substituição tributária relativo às operações e prestações subsequentes")]
        Cst52 = 52,
        [Description("53 - Tributação Monofásica sobre Combustíveis com recolhimento diferido")]
        Cst53 = 53,
        [Description("60 - ICMS cobrado anteriormente por substituição tributária")]
        Cst60 = 60,
        [Description("61 - Tributação Monofásica sobre Combustíveis cobrada anteriormente")]
        Cst61 = 61,
        [Description("70 - Com redução de base de cálculo e cobrança do ICMS por substituição tributária")]
        Cst70 = 70,
        [Description("72 - Tributada com redução de base de cálculo e com ICMS devido por substituição tributária relativo às operações e prestações antecedentes")]
        Cst72 = 72,
        [Description("74 - Tributada com redução de base de cálculo e com ICMS devido por substituição tributária relativo às operações e prestações concomitantes")]
        Cst74 = 74,
        [Description("90 - Outras")]
        Cst90 = 90
    }

    public enum ECodigoSituacaoTributariaIcmsConsumidor
    {
        [Description("00 - Tributada integralmente")]
        Cst00 = 0,
        [Description("20 - Com redução de base de cálculo")]
        Cst20 = 20,
        [Description("40 - Isenta")]
        Cst40 = 40,
        [Description("41 - Não tributada")]
        Cst41 = 41,
        [Description("60 - ICMS cobrado anteriormente por substituição tributária")]
        Cst60 = 60,
        [Description("61 - Tributação Monofásica sobre Combustíveis cobrada anteriormente")]
        Cst61 = 61
    }
}
