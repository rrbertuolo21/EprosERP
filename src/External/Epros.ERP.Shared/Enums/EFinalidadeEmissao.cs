using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EFinalidadeEmissao
    {
        [Description("Normal")]
        fnNormal = 1,

        [Description("Complementar")]
        fnComplementar = 2,

        [Description("Ajuste")]
        fnAjuste = 3,

        [Description("Devolução")]
        fnDevolucao = 4,

        [Description("Nota de Crédito")]
        fnNotaCredito = 5,

        [Description("Nota de Débito")]
        fnNotaDebito = 6
    }
}
