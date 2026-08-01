using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Interfaces
{
    /// <summary>Dados do pagador enviados ao gateway na criação da cobrança.</summary>
    public record DadosPagador(string Email, string? PrimeiroNome = null, string? Documento = null);

    /// <summary>Resultado da criação de uma cobrança PIX no gateway.</summary>
    public record CobrancaPixResultado(
        string PaymentId,
        string? QrCode,
        string? QrCodeBase64,
        string? TicketUrl,
        DateTime? DataExpiracao,
        string Status);

    /// <summary>Resultado da consulta de um pagamento no gateway.</summary>
    public record ConsultaPagamentoResultado(
        string PaymentId,
        string Status,
        decimal? ValorTransacao,
        decimal? ValorTarifa,
        DateTime? DataAprovacao,
        string? ExternalReference = null,
        decimal? ValorLiquido = null);

    /// <summary>1.08B — Resultado da geração de um BOLETO no gateway.</summary>
    public record CobrancaBoletoResultado(
        string PaymentId,
        string? LinhaDigitavel,
        string? CodigoBarras,
        DateTime? DataVencimento,
        string? UrlBoleto,
        string Status);

    /// <summary>
    /// 1.08B — Resultado da criação de um cartão-on-file no gateway (Customers/Cards do MP).
    /// Só carrega identificadores OPACOS + metadados NÃO sensíveis — nunca PAN/CVV (PCI).
    /// </summary>
    public record CartaoOnFileResultado(
        string CustomerId,
        string CardId,
        string? Bandeira,
        string? UltimosQuatro,
        int? ValidadeMes,
        int? ValidadeAno);

    /// <summary>1.08B — Resultado da criação de uma preferência de Checkout (Checkout Pro do MP).</summary>
    public record PreferenciaCheckoutResultado(
        string PreferenceId,
        string InitPoint);

    /// <summary>1.08E — Resultado de um ESTORNO/refund de pagamento no gateway (MP refunds).</summary>
    public record EstornoResultado(
        string RefundId,
        string PaymentId,
        decimal? Valor,
        string Status);

    /// <summary>
    /// Abstração do lado OUTBOUND de pagamento (criação da cobrança e consulta).
    /// Implementada por provedor concreto (ex.: <c>MercadoPagoGateway</c>).
    /// Todos os métodos retornam <see cref="CommandResult"/>: em sucesso, <c>Dados</c> carrega
    /// o DTO tipado; em falha, <c>Erros</c>/<c>Mensagem</c> descrevem o problema.
    /// </summary>
    public interface IPaymentGateway
    {
        /// <summary>
        /// Cria uma cobrança PIX no gateway para a fatura informada.
        /// Sucesso → <c>Dados</c> é <see cref="CobrancaPixResultado"/>.
        /// </summary>
        Task<CommandResult> GerarCobrancaPixAsync(
            Fatura fatura,
            ConfiguracaoGatewayPagamento config,
            DadosPagador pagador,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Consulta o status/valores de um pagamento no gateway.
        /// Sucesso → <c>Dados</c> é <see cref="ConsultaPagamentoResultado"/>.
        /// </summary>
        Task<CommandResult> ConsultarPagamentoAsync(
            string paymentId,
            ConfiguracaoGatewayPagamento config,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Faz uma chamada leve ao gateway para validar as credenciais (access token) da configuração.
        /// </summary>
        Task<CommandResult> TestarConexaoAsync(
            ConfiguracaoGatewayPagamento config,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 1.08B — Gera um BOLETO para a fatura. Sucesso → <c>Dados</c> é <see cref="CobrancaBoletoResultado"/>.
        /// A conciliação do pagamento do boleto acontece pelo MESMO webhook unificado (o MP concilia por webhook).
        /// </summary>
        Task<CommandResult> GerarBoletoAsync(
            Fatura fatura,
            ConfiguracaoGatewayPagamento config,
            DadosPagador pagador,
            DateTime dataVencimento,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 1.08B — Cria um cartão-on-file no gateway a partir do TOKEN gerado pelo front (a lib do MP tokeniza
        /// o cartão no navegador). ⛔ PCI: o backend recebe apenas o token; PAN/CVV nunca trafegam aqui.
        /// Sucesso → <c>Dados</c> é <see cref="CartaoOnFileResultado"/> (customer_id + card_id + metadados).
        /// </summary>
        Task<CommandResult> CriarCartaoOnFileAsync(
            ConfiguracaoGatewayPagamento config,
            DadosPagador pagador,
            string cardToken,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 1.08B — Cobra a fatura no cartão-on-file salvo (customer_id + card_id opacos do gateway).
        /// ⛔ PCI: nenhum dado de cartão cru é enviado — o gateway usa o cartão já tokenizado.
        /// Sucesso → <c>Dados</c> é <see cref="ConsultaPagamentoResultado"/> (payment id + status + valores).
        /// </summary>
        Task<CommandResult> CobrarCartaoAsync(
            Fatura fatura,
            ConfiguracaoGatewayPagamento config,
            string customerId,
            string cardId,
            DadosPagador pagador,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 1.08B — Cria uma preferência de Checkout (Checkout Pro do MP) para o cliente pagar por
        /// PIX/cartão/boleto na tela hospedada do gateway. O retorno concilia pelo webhook unificado
        /// (external_reference = Id da Fatura). Sucesso → <c>Dados</c> é <see cref="PreferenciaCheckoutResultado"/>.
        /// </summary>
        Task<CommandResult> CriarPreferenciaCheckoutAsync(
            Fatura fatura,
            ConfiguracaoGatewayPagamento config,
            string descricao,
            DadosPagador pagador,
            string? urlRetorno,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// 1.08E — Estorna (refund) um pagamento no gateway (Mercado Pago: <c>POST /v1/payments/{id}/refunds</c>).
        /// <paramref name="valor"/> null → estorno TOTAL; informado → estorno parcial.
        /// Sucesso → <c>Dados</c> é <see cref="EstornoResultado"/> (refund id + status).
        /// </summary>
        Task<CommandResult> EstornarPagamentoAsync(
            string paymentId,
            ConfiguracaoGatewayPagamento config,
            decimal? valor,
            CancellationToken cancellationToken = default);
    }
}
