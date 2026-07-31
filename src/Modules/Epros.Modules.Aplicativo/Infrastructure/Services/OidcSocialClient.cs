using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace Epros.Modules.Aplicativo.Infrastructure.Services
{
    /// <summary>Perfil verificado extraído do <c>id_token</c> do provedor social após validação de assinatura.</summary>
    public sealed record PerfilSocial(string SubjectId, string? Email, bool EmailVerificado, string? Nome);

    /// <summary>
    /// Cliente OAuth 2.0 / OIDC (Authorization Code) genérico para login social. Sem dependência nova
    /// pesada: usa <c>HttpClient</c> para o discovery/token e <c>System.IdentityModel.Tokens.Jwt</c> +
    /// <c>Microsoft.IdentityModel.Tokens</c> (já no solution via Epros.Shared) para validar o id_token
    /// contra o JWKS do provedor. Suporta Google e Microsoft (Entra, endpoint v2.0 /common).
    /// </summary>
    public interface IOidcSocialClient
    {
        /// <summary>Monta a URL de autorização (Authorization Code) para redirecionar o usuário ao provedor.</summary>
        Task<string> MontarUrlAutorizacaoAsync(ProvedorSocialOptions opcoes, string state, string nonce, CancellationToken ct);

        /// <summary>
        /// Troca o <c>code</c> por token no provedor, valida o <c>id_token</c> (assinatura via JWKS,
        /// emissor, audiência = ClientId, expiração e nonce) e devolve o perfil verificado.
        /// Lança <see cref="OidcSocialException"/> em qualquer falha (perfil não confiável).
        /// </summary>
        Task<PerfilSocial> TrocarCodigoPorPerfilAsync(ProvedorSocialOptions opcoes, ProvedorSocial provedor, string code, string nonceEsperado, CancellationToken ct);
    }

    /// <summary>Falha no fluxo OIDC (discovery, troca de code, ou id_token inválido). Mensagem segura para log.</summary>
    public sealed class OidcSocialException : Exception
    {
        public OidcSocialException(string message) : base(message) { }
    }

    public sealed class OidcSocialClient : IOidcSocialClient
    {
        public const string HttpClientName = "oidc-social";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _cache;
        private static readonly JwtSecurityTokenHandler _jwtHandler = new();

        public OidcSocialClient(IHttpClientFactory httpClientFactory, IMemoryCache cache)
        {
            _httpClientFactory = httpClientFactory;
            _cache = cache;
        }

        public async Task<string> MontarUrlAutorizacaoAsync(ProvedorSocialOptions opcoes, string state, string nonce, CancellationToken ct)
        {
            var discovery = await ObterDiscoveryAsync(opcoes.Authority, ct);

            var parametros = new Dictionary<string, string?>
            {
                ["client_id"] = opcoes.ClientId,
                ["response_type"] = "code",
                ["redirect_uri"] = opcoes.RedirectUri,
                ["scope"] = opcoes.Scopes,
                ["state"] = state,
                ["nonce"] = nonce,
                // response_mode=query garante o code na querystring do callback (GET).
                ["response_mode"] = "query"
            };

            return QueryHelpers.AddQueryString(discovery.AuthorizationEndpoint, parametros);
        }

        public async Task<PerfilSocial> TrocarCodigoPorPerfilAsync(ProvedorSocialOptions opcoes, ProvedorSocial provedor, string code, string nonceEsperado, CancellationToken ct)
        {
            var discovery = await ObterDiscoveryAsync(opcoes.Authority, ct);

            // 1. Troca o Authorization Code por tokens no token_endpoint (client_secret_post).
            var http = _httpClientFactory.CreateClient(HttpClientName);
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = opcoes.RedirectUri,
                ["client_id"] = opcoes.ClientId,
                ["client_secret"] = opcoes.ClientSecret
            };

            using var resp = await http.PostAsync(discovery.TokenEndpoint, new FormUrlEncodedContent(form), ct);
            var body = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                throw new OidcSocialException($"Falha ao trocar o código no provedor ({(int)resp.StatusCode}).");
            }

            string? idToken;
            try
            {
                using var doc = JsonDocument.Parse(body);
                idToken = doc.RootElement.TryGetProperty("id_token", out var it) ? it.GetString() : null;
            }
            catch (JsonException)
            {
                throw new OidcSocialException("Resposta de token do provedor inválida.");
            }

            if (string.IsNullOrWhiteSpace(idToken))
            {
                throw new OidcSocialException("O provedor não retornou id_token.");
            }

            // 2. Valida o id_token (assinatura via JWKS, emissor, audiência, expiração).
            var chaves = await ObterChavesAsync(discovery.JwksUri, ct);
            var parametros = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = chaves,
                ValidateIssuer = true,
                IssuerValidator = (issuerNoToken, _, _) => ValidarEmissor(issuerNoToken, discovery.Issuer),
                ValidateAudience = true,
                ValidAudience = opcoes.ClientId,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            JwtSecurityToken jwt;
            try
            {
                _jwtHandler.ValidateToken(idToken, parametros, out var validado);
                jwt = (JwtSecurityToken)validado;
            }
            catch (Exception ex)
            {
                throw new OidcSocialException($"id_token inválido: {ex.GetType().Name}.");
            }

            // 3. Valida o nonce (anti-replay: liga a resposta ao /start desta sessão).
            var nonceToken = jwt.Claims.FirstOrDefault(c => c.Type == "nonce")?.Value;
            if (string.IsNullOrEmpty(nonceEsperado) || nonceToken != nonceEsperado)
            {
                throw new OidcSocialException("nonce do id_token não confere.");
            }

            // 4. Extrai o perfil. SubjectId: 'sub' (Google) ou 'oid' estável (Microsoft/Entra).
            string? Claim(params string[] tipos)
            {
                foreach (var t in tipos)
                {
                    var v = jwt.Claims.FirstOrDefault(c => c.Type == t)?.Value;
                    if (!string.IsNullOrWhiteSpace(v)) return v;
                }
                return null;
            }

            var subjectId = provedor == ProvedorSocial.Microsoft
                ? Claim("oid", "http://schemas.microsoft.com/identity/claims/objectidentifier", "sub")
                : Claim("sub");

            if (string.IsNullOrWhiteSpace(subjectId))
            {
                throw new OidcSocialException("id_token sem identificador de usuário (sub/oid).");
            }

            var email = Claim("email", "preferred_username", "upn");
            var nome = Claim("name", "given_name");

            // email_verified: Google envia bool. Microsoft/Entra normalmente NÃO envia — nesse caso
            // tratamos como NÃO verificado (default seguro: não vincula automaticamente a conta existente,
            // encaminha ao onboarding). Aceitamos bool ou string "true".
            var emailVerified = false;
            var evClaim = Claim("email_verified");
            if (!string.IsNullOrEmpty(evClaim))
            {
                emailVerified = bool.TryParse(evClaim, out var b) && b;
            }

            return new PerfilSocial(subjectId!, email, emailVerified, nome);
        }

        private static string ValidarEmissor(string issuerNoToken, string issuerDiscovery)
        {
            if (string.Equals(issuerNoToken, issuerDiscovery, StringComparison.Ordinal))
            {
                return issuerNoToken;
            }

            // Microsoft /common: o discovery anuncia o emissor templatizado com {tenantid}. Aceitamos
            // qualquer emissor concreto que case com esse molde (tenant GUID no lugar do placeholder).
            if (issuerDiscovery.Contains("{tenantid}", StringComparison.OrdinalIgnoreCase))
            {
                var padrao = "^" + Regex.Escape(issuerDiscovery).Replace("\\{tenantid\\}", "[0-9a-fA-F-]+", StringComparison.OrdinalIgnoreCase) + "$";
                if (Regex.IsMatch(issuerNoToken, padrao))
                {
                    return issuerNoToken;
                }
            }

            throw new SecurityTokenInvalidIssuerException("Emissor do id_token não confere com o provedor configurado.");
        }

        private async Task<DiscoveryDoc> ObterDiscoveryAsync(string authority, CancellationToken ct)
        {
            var chaveCache = $"oidc:discovery:{authority}";
            if (_cache.TryGetValue<DiscoveryDoc>(chaveCache, out var cached) && cached is not null)
            {
                return cached;
            }

            var url = $"{authority.TrimEnd('/')}/.well-known/openid-configuration";
            var http = _httpClientFactory.CreateClient(HttpClientName);

            string json;
            try
            {
                json = await http.GetStringAsync(url, ct);
            }
            catch (Exception)
            {
                throw new OidcSocialException("Não foi possível obter a configuração OIDC do provedor.");
            }

            DiscoveryDoc doc;
            try
            {
                using var parsed = JsonDocument.Parse(json);
                var root = parsed.RootElement;
                doc = new DiscoveryDoc(
                    Issuer: root.GetProperty("issuer").GetString()!,
                    AuthorizationEndpoint: root.GetProperty("authorization_endpoint").GetString()!,
                    TokenEndpoint: root.GetProperty("token_endpoint").GetString()!,
                    JwksUri: root.GetProperty("jwks_uri").GetString()!);
            }
            catch (Exception)
            {
                throw new OidcSocialException("Configuração OIDC do provedor incompleta.");
            }

            _cache.Set(chaveCache, doc, TimeSpan.FromHours(1));
            return doc;
        }

        private async Task<IReadOnlyCollection<SecurityKey>> ObterChavesAsync(string jwksUri, CancellationToken ct)
        {
            var chaveCache = $"oidc:jwks:{jwksUri}";
            if (_cache.TryGetValue<IReadOnlyCollection<SecurityKey>>(chaveCache, out var cached) && cached is not null)
            {
                return cached;
            }

            var http = _httpClientFactory.CreateClient(HttpClientName);
            string json;
            try
            {
                json = await http.GetStringAsync(jwksUri, ct);
            }
            catch (Exception)
            {
                throw new OidcSocialException("Não foi possível obter as chaves de assinatura do provedor.");
            }

            IReadOnlyCollection<SecurityKey> chaves;
            try
            {
                chaves = new JsonWebKeySet(json).GetSigningKeys().ToList();
            }
            catch (Exception)
            {
                throw new OidcSocialException("Chaves de assinatura do provedor inválidas.");
            }

            // TTL curto: JWKS rotaciona; 1h equilibra segurança e latência.
            _cache.Set(chaveCache, chaves, TimeSpan.FromHours(1));
            return chaves;
        }

        private sealed record DiscoveryDoc(string Issuer, string AuthorizationEndpoint, string TokenEndpoint, string JwksUri);
    }
}
