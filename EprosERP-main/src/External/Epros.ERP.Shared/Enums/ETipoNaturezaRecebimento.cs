using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoNaturezaRecebimento
    {
        [Description("Recebimento Caixa")]
        RecebimentoCaixa,

        [Description("Recebimento Banco")]
        RecebimentoBanco,

        [Description("Recebimento Desconto")]
        RecebimentoDesconto,

        [Description("Recebimento Multa")]
        RecebimentoMulta,

        [Description("Recebimento Juros")]
        RecebimentoJuros
    }
}
