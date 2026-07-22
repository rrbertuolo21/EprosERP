using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Epros.API.Middlewares
{
    public class DataMaskingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DataMaskingMiddleware> _logger;

        // Expressões regulares compiladas para identificação de padrões sensíveis (CPF e PAN/Cartão de Crédito)
        private static readonly Regex CpfRegex = new Regex(@"\b\d{3}\.\d{3}\.\d{3}-\d{2}\b", RegexOptions.Compiled);
        private static readonly Regex CardPanRegex = new Regex(@"\b(?:\d[ -]*?){13,16}\b", RegexOptions.Compiled);

        public DataMaskingMiddleware(RequestDelegate next, ILogger<DataMaskingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // O pipeline de escrita prossegue. Os métodos estáticos são chamados pelas políticas de logs
            // (Serilog custom sinks/formatters) para higienizar logs antes da gravação (PCI DSS Req. 3).
            await _next(context);
        }

        /// <summary>
        /// Mascara padrões de dados sensíveis em payloads de logs ou mensagens antes de serem persistidos.
        /// </summary>
        public static string MascararDadosSensiveis(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Mascaramento de cartão (PAN) - exibe apenas os últimos 4 dígitos
            var result = CardPanRegex.Replace(input, match =>
            {
                var cleanPan = Regex.Replace(match.Value, @"[^\d]", "");
                if (cleanPan.Length >= 13 && cleanPan.Length <= 16)
                {
                    return "****-****-****-" + cleanPan.Substring(cleanPan.Length - 4);
                }
                return match.Value;
            });

            // Mascaramento de CPF - oculta dígitos centrais
            result = CpfRegex.Replace(result, match =>
            {
                var val = match.Value;
                return val.Substring(0, 4) + "***.***-" + val.Substring(val.Length - 2);
            });

            return result;
        }
    }
}
