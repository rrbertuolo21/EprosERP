using Flunt.Notifications;
using Flunt.Validations;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Pagamento;

namespace Epros.ERP.DfeCalculos.Models.Vendas
{
    public class VendaPagamento : Notifiable<Notification>
    {
        protected VendaPagamento() { }
        public VendaPagamento(decimal valorPago, decimal valorTroco, FormaPagamento formaPagamento, IndicadorPagamentoDetalhePagamento indicacaoPagamento, string descricao, VendaPagamentoCartao? cartao)
        {
            ValorPago = valorPago;
            ValorTroco = valorTroco;
            FormaPagamento = formaPagamento;
            IndicacaoPagamento = indicacaoPagamento;
            Descricao = FormaPagamento != FormaPagamento.fpOutro ? null : string.IsNullOrEmpty(descricao) ? null : descricao;
            Cartao = cartao;
            //IntegracaoPagamento = integracaoPagamento == 0 ? null : integracaoPagamento; ;
            //BandeiraCartao = bandeiraCartao == 0 ? null : bandeiraCartao;
            //NumeroAutorizacaoOperaCartao = string.IsNullOrEmpty(numeroAutorizacaoOperaCartao) ? null : numeroAutorizacaoOperaCartao;
            Validar();

        }

        public decimal ValorPago { get; private set; }
        public decimal ValorTroco { get; private set; }
        public FormaPagamento FormaPagamento { get; private set; }
        public IndicadorPagamentoDetalhePagamento IndicacaoPagamento { get; private set; }
        public string? Descricao { get; private set; }
        public VendaPagamentoCartao? Cartao { get; set; }
        public void Validar()
        {
            AddNotifications(new Contract<Notification>()

                .IsGreaterOrEqualsThan(ValorPago, decimal.Zero, "ValorPago", "ValorPago deve ser maior ou igual a zero")

                .IsGreaterOrEqualsThan(ValorTroco, decimal.Zero, "ValorTroco", "Troco da NF deve ser maior ou igual a zero")

                .IsLowerOrEqualsThan(60, (Descricao ?? "").Length, "Descricao", "Descrição do pagamento pode conter entre 2 e 60 caracteres")
           );

            if (!Enum.TryParse(FormaPagamento.GetHashCode().ToString(), out TipoIntegracaoPagamento integracaoPagamento))
                AddNotification("IntegracaoPagamento", $"Integraçãoo do Pagamento, inválido");

            if (!Enum.TryParse(FormaPagamento.GetHashCode().ToString(), out FormaPagamento modeloDocumento))
                AddNotification("FormaPagamento", $"Forma de Pagamento inválida");

            if (!Enum.TryParse(IndicacaoPagamento.GetHashCode().ToString(), out IndicadorPagamentoDetalhePagamento indicacaoPagamento))
                AddNotification("IndicacaoPagamento", $"Indicação do Pagamento inválido");
        }

        public void RedefinirTroco(decimal valorTroco)
        {
            ValorTroco = valorTroco;
        }
    }

}
