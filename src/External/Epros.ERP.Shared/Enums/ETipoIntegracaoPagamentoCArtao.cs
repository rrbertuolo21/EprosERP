using System.ComponentModel;

namespace Epros.ERP.Shared.Enums
{
    public enum ETipoIntegracaoPagamentoCArtao
    {
        [Description("Não utiliza")]
        NaoUtiliza = -1,
        [Description("Pagamento integrado com o sistema de automação da empresa (Ex.: equipamento TEF, Comércio Eletrônico)")]
        PagamentoIntegradoComSistemaAutomacao = 1,
        [Description(" Pagamento não integrado com o sistema de automação da empresa (Ex.: equipamento POS)")]
        PagamentoNaoIntegradoComSistemaAutomacao = 2
    }
}
