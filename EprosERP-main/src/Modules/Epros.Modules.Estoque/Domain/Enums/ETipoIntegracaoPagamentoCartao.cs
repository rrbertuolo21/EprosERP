using System.ComponentModel;

namespace Epros.Modules.Estoque.Domain.Enums
{
    /// <summary>
    /// Tipo de integração do pagamento com cartão. Porte fiel do legado
    /// Epros.ERP.Shared.Enums.ETipoIntegracaoPagamentoCArtao (nome corrigido para ETipoIntegracaoPagamentoCartao).
    /// </summary>
    public enum ETipoIntegracaoPagamentoCartao
    {
        [Description("Não utiliza")]
        NaoUtiliza = -1,

        [Description("Pagamento integrado com o sistema de automação da empresa (Ex.: equipamento TEF, Comércio Eletrônico)")]
        PagamentoIntegradoComSistemaAutomacao = 1,

        [Description(" Pagamento não integrado com o sistema de automação da empresa (Ex.: equipamento POS)")]
        PagamentoNaoIntegradoComSistemaAutomacao = 2
    }
}
