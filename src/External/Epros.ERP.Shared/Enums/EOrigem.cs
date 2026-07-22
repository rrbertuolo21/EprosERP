using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum EOrigem
    {
        [Description("Manual contas a Receber")]
        ManualContasAReceber,

        [Description("Manual contas a pagar")]
        ManualContasAPagar,

        [Description("Emissão de saída NFe")]
        EmissaoSaidaNFe,

        [Description("Emissão de entrada NFe")]
        EmissaoEntradaNFe,

        [Description("Registro de movimento do estoque")]
        registroMovimentoEstoque,

        [Description("Emissão de saída NFce")]
        EmissaoSaidaNFce,

        [Description("Emissão de saída Cupom Não Fiscal")]
        EmissaoSaidaCupomNaoFiscal,

        [Description("Emissão de saída NFse")]
        EmissaoSaidaNFse,

        [Description("Emissão de entrada NFse")]
        EmissaoEntradaNFse,
    }
}
