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
        DateTime? DataAprovacao);

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
    }
}
