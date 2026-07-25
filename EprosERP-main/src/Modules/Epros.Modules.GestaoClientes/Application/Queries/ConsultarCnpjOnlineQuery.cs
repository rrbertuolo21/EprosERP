using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    /// <summary>
    /// Consulta pública de dados de CNPJ em serviço externo (ReceitaWS/BrasilAPI). Porte fiel do
    /// legado <c>CnpjOnlineController</c>. O endpoint é configurável por <c>CnpjOnline:Endpoint</c>
    /// (padrão ReceitaWS) e pode ser desligado por <c>CnpjOnline:Habilitado=false</c>; sem configuração
    /// de rede/serviço, degrada honestamente (não simula dados).
    /// </summary>
    public record ConsultarCnpjOnlineQuery(string Cnpj) : IQuery<CommandResult>;

    public class ConsultarCnpjOnlineQueryHandler : IQueryHandler<ConsultarCnpjOnlineQuery, CommandResult>
    {
        private const string EndpointPadrao = "https://www.receitaws.com.br/v1/cnpj/";
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private readonly IConfiguration _configuration;

        public ConsultarCnpjOnlineQueryHandler(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<CommandResult> Handle(ConsultarCnpjOnlineQuery request, CancellationToken cancellationToken)
        {
            var cnpj = SomenteNumeros(request.Cnpj);
            if (cnpj.Length != 14)
                return CommandResult.Falha("CNPJ inválido. Informe 14 dígitos.");

            var habilitado = _configuration.GetValue<bool?>("CnpjOnline:Habilitado") ?? true;
            if (!habilitado)
                return CommandResult.Falha("Consulta de CNPJ não configurada.");

            var endpoint = _configuration.GetValue<string>("CnpjOnline:Endpoint");
            if (string.IsNullOrWhiteSpace(endpoint))
                endpoint = EndpointPadrao;

            var url = endpoint.EndsWith("/") ? endpoint + cnpj : endpoint + "/" + cnpj;

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    return CommandResult.Falha($"Falha na consulta do CNPJ (HTTP {(int)response.StatusCode}).");

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;

                // ReceitaWS devolve { "status": "ERROR", "message": "..." } em caso de erro.
                if (root.TryGetProperty("status", out var statusProp) &&
                    string.Equals(statusProp.GetString(), "ERROR", StringComparison.OrdinalIgnoreCase))
                {
                    var msg = root.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "CNPJ não encontrado.";
                    return CommandResult.Falha(msg ?? "CNPJ não encontrado.");
                }

                var dados = new
                {
                    Cnpj = cnpj,
                    Nome = Texto(root, "nome"),
                    NomeFantasia = Texto(root, "fantasia"),
                    Telefone = SomenteNumeros(PrimeiroTelefone(Texto(root, "telefone"))),
                    Email = Texto(root, "email"),
                    NaturezaJuridica = Texto(root, "natureza_juridica"),
                    Logradouro = Texto(root, "logradouro"),
                    Numero = Texto(root, "numero"),
                    Complemento = Texto(root, "complemento"),
                    Cep = SomenteNumeros(Texto(root, "cep")),
                    Bairro = Texto(root, "bairro"),
                    Municipio = Texto(root, "municipio"),
                    Uf = Texto(root, "uf"),
                    AtividadePrincipal = Atividades(root, "atividade_principal"),
                    AtividadesSecundarias = Atividades(root, "atividades_secundarias")
                };

                return CommandResult.Ok("OK", dados);
            }
            catch (Exception ex)
            {
                return CommandResult.Falha($"Não foi possível consultar o CNPJ no serviço externo: {ex.Message}");
            }
        }

        private static string SomenteNumeros(string? valor)
            => string.IsNullOrEmpty(valor) ? string.Empty : new string(Array.FindAll(valor.ToCharArray(), char.IsDigit));

        private static string Texto(JsonElement root, string propriedade)
            => root.TryGetProperty(propriedade, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? string.Empty
                : string.Empty;

        private static string PrimeiroTelefone(string telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone)) return string.Empty;
            var partes = telefone.Split('/');
            return partes.Length > 0 ? partes[0].Trim() : telefone;
        }

        private static List<object> Atividades(JsonElement root, string propriedade)
        {
            var lista = new List<object>();
            if (root.TryGetProperty(propriedade, out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in arr.EnumerateArray())
                {
                    var code = item.TryGetProperty("code", out var c) ? c.GetString() : string.Empty;
                    var text = item.TryGetProperty("text", out var t) ? t.GetString() : string.Empty;
                    lista.Add(new
                    {
                        Cnae = SomenteNumeros(code),
                        Descricao = text ?? string.Empty
                    });
                }
            }
            return lista;
        }
    }
}
