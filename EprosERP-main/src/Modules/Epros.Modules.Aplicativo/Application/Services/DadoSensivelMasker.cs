using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Epros.Modules.Aplicativo.Application.Services
{
    /// <summary>
    /// Mascara dados sensíveis (LGPD) antes de gravá-los na trilha de auditoria do workflow (payload_json).
    /// Regra: campos cujo nome bate com a lista de sensíveis têm o valor substituído por máscara.
    /// [Origem: EF WORKFLOW 7.7.3 / WF-CA-014]
    /// </summary>
    public static class DadoSensivelMasker
    {
        private static readonly string[] ChavesSensiveis =
        {
            "senha", "password", "token", "secret", "cpf", "cnpj", "rg", "email",
            "telefone", "celular", "cartao", "card", "pix", "conta", "agencia",
            "salario", "remuneracao", "documento"
        };

        private const string Mascara = "***";

        /// <summary>Serializa um par antes/depois já mascarado como JSON para a trilha imutável.</summary>
        public static string SerializarTrilha(object? antes, object? depois)
        {
            var payload = new Dictionary<string, object?>
            {
                ["antes"] = Mascarar(antes),
                ["depois"] = Mascarar(depois)
            };
            return JsonSerializer.Serialize(payload);
        }

        /// <summary>Recebe um JSON de payload e devolve o mesmo JSON com campos sensíveis mascarados.</summary>
        public static string? MascararJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return json;
            try
            {
                using var doc = JsonDocument.Parse(json);
                var mascarado = MascararElemento(doc.RootElement);
                return JsonSerializer.Serialize(mascarado);
            }
            catch (JsonException)
            {
                // Não é JSON estruturado — devolve máscara total por precaução (como string JSON válida).
                return JsonSerializer.Serialize(Mascara);
            }
        }

        private static object? Mascarar(object? valor)
        {
            if (valor == null) return null;
            var json = JsonSerializer.Serialize(valor);
            using var doc = JsonDocument.Parse(json);
            return MascararElemento(doc.RootElement);
        }

        private static object? MascararElemento(JsonElement el)
        {
            switch (el.ValueKind)
            {
                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object?>();
                    foreach (var prop in el.EnumerateObject())
                    {
                        obj[prop.Name] = EhSensivel(prop.Name)
                            ? Mascara
                            : MascararElemento(prop.Value);
                    }
                    return obj;
                case JsonValueKind.Array:
                    return el.EnumerateArray().Select(MascararElemento).ToList();
                case JsonValueKind.String:
                    return el.GetString();
                case JsonValueKind.Number:
                    return el.GetDecimal();
                case JsonValueKind.True:
                case JsonValueKind.False:
                    return el.GetBoolean();
                default:
                    return null;
            }
        }

        private static bool EhSensivel(string nomeCampo)
        {
            var normalizado = Regex.Replace(nomeCampo, "[^a-zA-Z]", string.Empty).ToLowerInvariant();
            return ChavesSensiveis.Any(c => normalizado.Contains(c));
        }
    }
}
