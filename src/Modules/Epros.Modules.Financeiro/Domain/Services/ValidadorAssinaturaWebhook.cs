using System;
using System.Security.Cryptography;
using System.Text;

namespace Epros.Modules.Financeiro.Domain.Services
{
    /// <summary>
    /// Validação de assinatura de webhook de pagamento por HMAC-SHA256 (estrutura genérica e agnóstica).
    /// O provedor assina o corpo (payload) com um segredo compartilhado; nós recomputamos o HMAC com a
    /// chave do gateway e comparamos em tempo constante.
    ///
    /// ⚠️ // valida-ambiente: o ESQUEMA EXATO de assinatura é específico do provedor real (ex.: Mercado
    /// Pago usa o header <c>x-signature</c> com <c>ts</c> + <c>v1</c> sobre um "manifest" id/ts). Aqui
    /// entregamos o HMAC-SHA256 hex do payload como default seguro/estrutural; a adaptação ao header e ao
    /// manifest do provedor é feita no ambiente (nunca chamamos o provedor externo daqui).
    /// </summary>
    public static class ValidadorAssinaturaWebhook
    {
        /// <summary>HMAC-SHA256(payload, chave) em hex minúsculo.</summary>
        public static string Assinar(string payload, string chave)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(chave ?? string.Empty));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));
            var sb = new StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        /// <summary>
        /// True quando a assinatura recebida confere com o HMAC recomputado. Comparação em tempo constante
        /// para não vazar timing. Assinatura vazia = inválida.
        /// </summary>
        public static bool Conferir(string payload, string chave, string? assinaturaRecebida)
        {
            if (string.IsNullOrWhiteSpace(assinaturaRecebida)) return false;
            var esperada = Assinar(payload, chave);
            // Aceita o formato "sha256=<hex>" além do hex puro (comum em webhooks).
            var recebida = assinaturaRecebida.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
                ? assinaturaRecebida.Substring("sha256=".Length)
                : assinaturaRecebida;
            var a = Encoding.UTF8.GetBytes(esperada);
            var b = Encoding.UTF8.GetBytes(recebida.Trim().ToLowerInvariant());
            if (a.Length != b.Length) return false;
            return CryptographicOperations.FixedTimeEquals(a, b);
        }
    }
}
