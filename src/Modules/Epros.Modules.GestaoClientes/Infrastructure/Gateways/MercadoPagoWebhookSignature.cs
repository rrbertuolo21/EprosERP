using System;
using System.Security.Cryptography;
using System.Text;

namespace Epros.Modules.GestaoClientes.Infrastructure.Gateways
{
    /// <summary>
    /// 1.08A — Validação da assinatura do webhook do Mercado Pago no esquema OFICIAL.
    ///
    /// O MP envia dois cabeçalhos:
    ///   x-signature: "ts=&lt;timestamp&gt;,v1=&lt;hash&gt;"
    ///   x-request-id: &lt;uuid&gt;
    ///
    /// O manifesto assinado é o template <c>id:&lt;data.id&gt;;request-id:&lt;x-request-id&gt;;ts:&lt;ts&gt;;</c>
    /// (campos omitidos quando o valor correspondente está ausente), e <c>v1</c> é o HMAC-SHA256 hex do
    /// manifesto com o <b>webhook secret</b> do painel do MP. O <c>data.id</c> alfanumérico é comparado
    /// em minúsculas (para pagamentos é numérico). Ver docs oficiais do Mercado Pago (Webhooks / Secret).
    ///
    /// Este é o esquema correto que substitui o antigo "{action}:{data.id}" (que nunca validaria um
    /// webhook genuíno do MP). Helper puro/estático para ser testável sem I/O.
    /// </summary>
    public static class MercadoPagoWebhookSignature
    {
        /// <summary>
        /// Valida a assinatura. Retorna <c>true</c> somente se o header estiver bem-formado e o HMAC bater.
        /// </summary>
        public static bool Validar(string secret, string? xSignatureHeader, string? xRequestId, string? dataId)
        {
            if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(xSignatureHeader))
                return false;

            var (ts, v1) = ParseSignatureHeader(xSignatureHeader);
            if (string.IsNullOrWhiteSpace(ts) || string.IsNullOrWhiteSpace(v1))
                return false;

            var esperado = ComputarV1(secret, dataId, xRequestId, ts);

            // Comparação de tempo constante para evitar timing attacks.
            return TempoConstanteIguais(esperado, v1.Trim());
        }

        /// <summary>Monta o header x-signature ("ts=..,v1=..") — usado por testes e clientes internos.</summary>
        public static string MontarHeader(string secret, string? dataId, string? xRequestId, string ts)
            => $"ts={ts},v1={ComputarV1(secret, dataId, xRequestId, ts)}";

        /// <summary>Computa o hash v1 (HMAC-SHA256 hex do manifesto oficial).</summary>
        public static string ComputarV1(string secret, string? dataId, string? xRequestId, string ts)
        {
            var manifesto = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(dataId))
                manifesto.Append("id:").Append(dataId!.ToLowerInvariant()).Append(';');
            if (!string.IsNullOrWhiteSpace(xRequestId))
                manifesto.Append("request-id:").Append(xRequestId).Append(';');
            if (!string.IsNullOrWhiteSpace(ts))
                manifesto.Append("ts:").Append(ts).Append(';');

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(manifesto.ToString()));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private static (string? ts, string? v1) ParseSignatureHeader(string header)
        {
            string? ts = null, v1 = null;
            foreach (var parte in header.Split(','))
            {
                var kv = parte.Split('=', 2);
                if (kv.Length != 2) continue;
                var chave = kv[0].Trim().ToLowerInvariant();
                var valor = kv[1].Trim();
                if (chave == "ts") ts = valor;
                else if (chave == "v1") v1 = valor;
            }
            return (ts, v1);
        }

        private static bool TempoConstanteIguais(string a, string b)
        {
            var ba = Encoding.UTF8.GetBytes(a);
            var bb = Encoding.UTF8.GetBytes(b);
            return CryptographicOperations.FixedTimeEquals(ba, bb);
        }
    }
}
