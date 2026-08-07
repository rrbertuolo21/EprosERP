using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Epros.Modules.Aplicativo.Application.Contracts;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>
    /// PLT · CONECTORES (ED-08) — dispatcher REAL de webhook: POST HTTP ao endpoint externo do tenant.
    /// Substitui o stub "não configurado". Envia o payload como JSON, a assinatura HMAC no header
    /// <c>X-Epros-Signature</c> e os headers customizados do endpoint. Timeout curto (o retry/backoff e
    /// o dead-letter são responsabilidade do handler de entrega, que interpreta o resultado). Só chama
    /// endpoints http/https (guarda mínima contra esquemas inválidos).
    /// </summary>
    public sealed class WebhookDispatchHttpService : IWebhookDispatchService
    {
        public const string HttpClientName = "webhook-dispatch";
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<WebhookDispatchHttpService> _logger;

        public WebhookDispatchHttpService(IHttpClientFactory factory, ILogger<WebhookDispatchHttpService> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task<ResultadoEntregaWebhook> EnviarAsync(string url, string payload, string? assinaturaHmac,
            IReadOnlyDictionary<string, string>? headers, CancellationToken cancellationToken = default)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return ResultadoEntregaWebhook.Falha(null, $"URL de webhook inválida: '{url}'.");
            }

            try
            {
                var client = _factory.CreateClient(HttpClientName);
                using var req = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Content = new StringContent(payload ?? string.Empty, Encoding.UTF8, "application/json")
                };
                if (!string.IsNullOrWhiteSpace(assinaturaHmac))
                    req.Headers.TryAddWithoutValidation("X-Epros-Signature", assinaturaHmac);
                if (headers != null)
                    foreach (var kv in headers)
                        req.Headers.TryAddWithoutValidation(kv.Key, kv.Value);

                using var resp = await client.SendAsync(req, cancellationToken);
                var codigo = (int)resp.StatusCode;
                if (resp.IsSuccessStatusCode)
                    return ResultadoEntregaWebhook.Ok(codigo);

                var corpo = await SafeLerAsync(resp, cancellationToken);
                return ResultadoEntregaWebhook.Falha(codigo, $"HTTP {codigo}: {corpo}");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                return ResultadoEntregaWebhook.Falha(null, "Timeout ao entregar o webhook.");
            }
            catch (HttpRequestException ex)
            {
                return ResultadoEntregaWebhook.Falha(null, $"Falha de rede: {ex.Message}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Erro inesperado ao entregar webhook para {Url}.", url);
                return ResultadoEntregaWebhook.Falha(null, ex.Message);
            }
        }

        private static async Task<string> SafeLerAsync(HttpResponseMessage resp, CancellationToken ct)
        {
            try
            {
                var s = await resp.Content.ReadAsStringAsync(ct);
                return s.Length > 500 ? s.Substring(0, 500) : s;
            }
            catch { return "(sem corpo)"; }
        }
    }
}
