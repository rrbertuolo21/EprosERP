using Flunt.Notifications;
using Flunt.Validations;
using NFe.Classes.Informacoes.Pagamento;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaPagamentoCartao : Notifiable<Notification>
    {
        protected VendaPagamentoCartao() { }
        public VendaPagamentoCartao(TipoIntegracaoPagamento? integracaoPagamento, BandeiraCartao? bandeiraCartao, string? numeroAutorizacaoOperaCartao)
        {
            if (Enum.IsDefined(typeof(TipoIntegracaoPagamento), integracaoPagamento!))
                TipoIntegracaoPagamento = integracaoPagamento;

            if (Enum.IsDefined(typeof(BandeiraCartao), bandeiraCartao!))
                BandeiraCartao = bandeiraCartao;

            NumeroAutorizacaoOperaCartao = string.IsNullOrEmpty(numeroAutorizacaoOperaCartao) ? null : numeroAutorizacaoOperaCartao;
        }

        public TipoIntegracaoPagamento? TipoIntegracaoPagamento { get; private set; }
        public BandeiraCartao? BandeiraCartao { get; private set; }
        public string? NumeroAutorizacaoOperaCartao { get; private set; }

        public void Validar()
        {
            AddNotifications(new Contract<Notification>()

                .IsLowerOrEqualsThan(20, (NumeroAutorizacaoOperaCartao ?? "").Length, "NumeroAutorizacaoOperaCartao", "Número Autorização Operação em Cartão do pagamento pode conter no max 20 caracteres")
           );

        }
    }

}
