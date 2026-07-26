using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Interfaces;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.Extensions.Logging;

namespace Epros.Modules.GestaoClientes.Infrastructure.Gateways
{
    /// <summary>
    /// Implementação do <see cref="IPaymentGateway"/> para o Mercado Pago (REST v1).
    /// Usa <see cref="IHttpClientFactory"/> (client nomeado "MercadoPago") e descriptografa o
    /// access token via <see cref="ISegredoCofreService"/> (o token é persistido cifrado).
    /// </summary>
    public class MercadoPagoGateway : IPaymentGateway
    {
        public const string HttpClientName = "MercadoPago";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISegredoCofreService _cofreService;
        private readonly ILogger<MercadoPagoGateway>? _logger;

        public MercadoPagoGateway(
            IHttpClientFactory httpClientFactory,
            ISegredoCofreService cofreService,
            ILogger<MercadoPagoGateway>? logger = null)
        {
            _httpClientFactory = httpClientFactory;
            _cofreService = cofreService;
            _logger = logger;
        }

        public async Task<CommandResult> GerarCobrancaPixAsync(
            Fatura fatura,
            ConfiguracaoGatewayPagamento config,
            DadosPagador pagador,
            CancellationToken cancellationToken = default)
        {
            if (config == null || !config.Ativo)
                return CommandResult.Falha("Gateway não configurado", "Nenhum gateway de pagamento ativo foi encontrado.");

            var (accessToken, erroToken) = await ObterAccessTokenAsync(config);
            if (erroToken != null)
                return CommandResult.Falha(erroToken, "Gateway não configurado");

            var body = new Dictionary<string, object?>
            {
                ["transaction_amount"] = fatura.Valor,
                ["payment_method_id"] = "pix",
                ["external_reference"] = fatura.Id.ToString(),
                ["date_of_expiration"] = DateTimeOffset.UtcNow.AddMinutes(30).ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                ["description"] = $"Fatura {fatura.Id}",
                ["payer"] = MontarPagador(pagador)
            };

            if (!string.IsNullOrWhiteSpace(config.NotificationUrl))
                body["notification_url"] = config.NotificationUrl;

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "v1/payments");
                AplicarAutenticacao(request, accessToken);
                // Idempotência exigida pelo MP para evitar cobranças duplicadas.
                request.Headers.TryAddWithoutValidation("X-Idempotency-Key", $"{fatura.Id}:{Guid.NewGuid()}");
                request.Content = JsonContent.Create(body);

                var client = CriarClient();
                using var response = await client.SendAsync(request, cancellationToken);
                var json = await LerJsonAsync(response, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return CommandResult.Falha(ExtrairMensagemErro(json, response), "Falha ao gerar cobrança PIX no Mercado Pago.");

                var root = json.RootElement;
                var paymentId = LerString(root, "id");
                var status = LerString(root, "status") ?? "pending";

                string? qrCode = null, qrCodeBase64 = null, ticketUrl = null;
                if (root.TryGetProperty("point_of_interaction", out var poi) &&
                    poi.ValueKind == JsonValueKind.Object &&
                    poi.TryGetProperty("transaction_data", out var td) &&
                    td.ValueKind == JsonValueKind.Object)
                {
                    qrCode = LerString(td, "qr_code");
                    qrCodeBase64 = LerString(td, "qr_code_base64");
                    ticketUrl = LerString(td, "ticket_url");
                }

                var dataExpiracao = LerData(root, "date_of_expiration");

                if (string.IsNullOrWhiteSpace(paymentId))
                    return CommandResult.Falha("O Mercado Pago não retornou o identificador do pagamento.", "Falha ao gerar cobrança PIX.");

                var dto = new CobrancaPixResultado(paymentId!, qrCode, qrCodeBase64, ticketUrl, dataExpiracao, status);
                return CommandResult.Ok("Cobrança PIX gerada com sucesso.", dto);
            }
            catch (TaskCanceledException)
            {
                _logger?.LogWarning("Timeout ao gerar cobrança PIX no Mercado Pago para fatura {FaturaId}.", fatura.Id);
                return CommandResult.Falha("Tempo limite excedido ao comunicar com o Mercado Pago.", "Falha ao gerar cobrança PIX.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao gerar cobrança PIX no Mercado Pago para fatura {FaturaId}.", fatura.Id);
                return CommandResult.Falha($"Erro de comunicação com o Mercado Pago: {ex.Message}", "Falha ao gerar cobrança PIX.");
            }
        }

        public async Task<CommandResult> ConsultarPagamentoAsync(
            string paymentId,
            ConfiguracaoGatewayPagamento config,
            CancellationToken cancellationToken = default)
        {
            if (config == null || !config.Ativo)
                return CommandResult.Falha("Gateway não configurado", "Nenhum gateway de pagamento ativo foi encontrado.");
            if (string.IsNullOrWhiteSpace(paymentId))
                return CommandResult.Falha("Identificador do pagamento é obrigatório.");

            var (accessToken, erroToken) = await ObterAccessTokenAsync(config);
            if (erroToken != null)
                return CommandResult.Falha(erroToken, "Gateway não configurado");

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/payments/{paymentId}");
                AplicarAutenticacao(request, accessToken);

                var client = CriarClient();
                using var response = await client.SendAsync(request, cancellationToken);
                var json = await LerJsonAsync(response, cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return CommandResult.Falha(ExtrairMensagemErro(json, response), "Falha ao consultar pagamento no Mercado Pago.");

                var root = json.RootElement;
                var status = LerString(root, "status") ?? "unknown";
                decimal? valor = LerDecimal(root, "transaction_amount");
                decimal? tarifa = SomarTarifas(root);
                var dataAprovacao = LerData(root, "date_approved");

                var dto = new ConsultaPagamentoResultado(paymentId, status, valor, tarifa, dataAprovacao);
                return CommandResult.Ok("Consulta realizada com sucesso.", dto);
            }
            catch (TaskCanceledException)
            {
                return CommandResult.Falha("Tempo limite excedido ao comunicar com o Mercado Pago.", "Falha ao consultar pagamento.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao consultar pagamento {PaymentId} no Mercado Pago.", paymentId);
                return CommandResult.Falha($"Erro de comunicação com o Mercado Pago: {ex.Message}", "Falha ao consultar pagamento.");
            }
        }

        public async Task<CommandResult> TestarConexaoAsync(
            ConfiguracaoGatewayPagamento config,
            CancellationToken cancellationToken = default)
        {
            if (config == null)
                return CommandResult.Falha("Gateway não configurado", "Configuração de gateway não informada.");

            var (accessToken, erroToken) = await ObterAccessTokenAsync(config);
            if (erroToken != null)
                return CommandResult.Falha(erroToken, "Gateway não configurado");

            try
            {
                // GET leve: lista os meios de pagamento — 200 confirma que o access token é válido.
                using var request = new HttpRequestMessage(HttpMethod.Get, "v1/payment_methods");
                AplicarAutenticacao(request, accessToken);

                var client = CriarClient();
                using var response = await client.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                    return CommandResult.Ok("Conexão com o Mercado Pago validada com sucesso.");

                var json = await LerJsonAsync(response, cancellationToken);
                return CommandResult.Falha(ExtrairMensagemErro(json, response), "Falha ao validar a conexão com o Mercado Pago.");
            }
            catch (TaskCanceledException)
            {
                return CommandResult.Falha("Tempo limite excedido ao comunicar com o Mercado Pago.", "Falha ao validar a conexão.");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Erro ao testar conexão com o Mercado Pago.");
                return CommandResult.Falha($"Erro de comunicação com o Mercado Pago: {ex.Message}", "Falha ao validar a conexão.");
            }
        }

        // ===== Helpers =====

        private HttpClient CriarClient()
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            if (client.BaseAddress == null)
                client.BaseAddress = new Uri("https://api.mercadopago.com/");
            return client;
        }

        private async Task<(string? token, string? erro)> ObterAccessTokenAsync(ConfiguracaoGatewayPagamento config)
        {
            if (string.IsNullOrWhiteSpace(config.AccessToken))
                return (null, "Gateway sem access token configurado.");

            try
            {
                var token = await _cofreService.DescriptografarAsync(config.AccessToken);
                if (string.IsNullOrWhiteSpace(token))
                    return (null, "Access token do gateway está vazio após descriptografia.");
                return (token, null);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Falha ao descriptografar o access token do gateway {ConfigId}.", config.Id);
                return (null, "Não foi possível ler o access token do gateway (falha no cofre de segredos).");
            }
        }

        private static void AplicarAutenticacao(HttpRequestMessage request, string? accessToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static Dictionary<string, object?> MontarPagador(DadosPagador pagador)
        {
            var payer = new Dictionary<string, object?>
            {
                ["email"] = pagador.Email
            };
            if (!string.IsNullOrWhiteSpace(pagador.PrimeiroNome))
                payer["first_name"] = pagador.PrimeiroNome;

            if (!string.IsNullOrWhiteSpace(pagador.Documento))
            {
                var doc = new string(pagador.Documento!.Where(char.IsDigit).ToArray());
                if (doc.Length > 0)
                {
                    payer["identification"] = new Dictionary<string, object?>
                    {
                        ["type"] = doc.Length > 11 ? "CNPJ" : "CPF",
                        ["number"] = doc
                    };
                }
            }
            return payer;
        }

        private static async Task<JsonDocument> LerJsonAsync(HttpResponseMessage response, CancellationToken ct)
        {
            var raw = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(raw))
                return JsonDocument.Parse("{}");
            try
            {
                return JsonDocument.Parse(raw);
            }
            catch
            {
                return JsonDocument.Parse("{}");
            }
        }

        private static string ExtrairMensagemErro(JsonDocument json, HttpResponseMessage response)
        {
            var root = json.RootElement;
            var msg = LerString(root, "message");
            if (!string.IsNullOrWhiteSpace(msg))
                return $"Mercado Pago ({(int)response.StatusCode}): {msg}";
            return $"Mercado Pago retornou status {(int)response.StatusCode} ({response.StatusCode}).";
        }

        private static string? LerString(JsonElement el, string prop)
        {
            if (el.ValueKind != JsonValueKind.Object || !el.TryGetProperty(prop, out var v))
                return null;
            return v.ValueKind switch
            {
                JsonValueKind.String => v.GetString(),
                JsonValueKind.Number => v.ToString(),
                JsonValueKind.Null => null,
                _ => v.ToString()
            };
        }

        private static decimal? LerDecimal(JsonElement el, string prop)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var v) &&
                v.ValueKind == JsonValueKind.Number && v.TryGetDecimal(out var d))
                return d;
            return null;
        }

        private static DateTime? LerData(JsonElement el, string prop)
        {
            if (el.ValueKind == JsonValueKind.Object && el.TryGetProperty(prop, out var v) &&
                v.ValueKind == JsonValueKind.String &&
                DateTime.TryParse(v.GetString(), null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt))
                return dt;
            return null;
        }

        private static decimal? SomarTarifas(JsonElement root)
        {
            if (root.ValueKind != JsonValueKind.Object ||
                !root.TryGetProperty("fee_details", out var fees) ||
                fees.ValueKind != JsonValueKind.Array)
                return null;

            decimal total = 0m;
            var achou = false;
            foreach (var fee in fees.EnumerateArray())
            {
                if (fee.ValueKind == JsonValueKind.Object && fee.TryGetProperty("amount", out var amount) &&
                    amount.ValueKind == JsonValueKind.Number && amount.TryGetDecimal(out var d))
                {
                    total += d;
                    achou = true;
                }
            }
            return achou ? total : null;
        }
    }
}
