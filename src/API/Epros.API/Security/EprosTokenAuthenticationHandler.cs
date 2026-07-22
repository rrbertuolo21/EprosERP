using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Epros.API.Security
{
    /// <summary>
    /// Esquema de autenticação nativo do EprosERP.
    ///
    /// A autenticação real da plataforma (enquanto o Keycloak/JWT real não é o mecanismo ativo)
    /// usa tokens estruturados emitidos por <c>AutenticarUsuarioCommandHandler</c>:
    ///   - jwt-token-basico-{tenantId}-{usuarioId}-{guid}
    ///   - jwt-token-completo-{tenantId}-{usuarioId}-{empresaId}-{perfilId}-{guid}
    ///
    /// Este handler valida esses tokens e materializa um <see cref="ClaimsPrincipal"/> autenticado
    /// (claims tenantId, NameIdentifier, empresaId, perfilId), habilitando a imposição de
    /// autorização por FallbackPolicy (RequireAuthenticatedUser) sem depender do Keycloak.
    ///
    /// Em ambientes de Desenvolvimento/Testing, aceita também os headers X-Tenant-Id / X-User-Id
    /// (usados pelos testes de integração e pelo desenvolvimento local). Em Produção, apenas o
    /// token estruturado é aceito — requisição sem token válido resulta em 401.
    /// </summary>
    public class EprosTokenAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "EprosToken";

        private readonly IHostEnvironmentFlags _env;

        public EprosTokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IHostEnvironmentFlags env)
            : base(options, logger, encoder)
        {
            _env = env;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1. Autenticação via token estruturado (mecanismo de produção)
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                var token = authHeader.Replace("Bearer ", "").Trim();
                var principal = TentarConstruirPrincipalDeToken(token);
                if (principal != null)
                {
                    return Task.FromResult(AuthenticateResult.Success(
                        new AuthenticationTicket(principal, SchemeName)));
                }
            }

            // 2. Fallback por headers (apenas em ambientes não-produtivos: dev e testes de integração)
            if (_env.PermiteHeadersDeAutenticacao)
            {
                if (Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenant) &&
                    !string.IsNullOrWhiteSpace(headerTenant.ToString()))
                {
                    var tenantId = headerTenant.ToString();
                    var userId = Request.Headers.TryGetValue("X-User-Id", out var headerUser) &&
                                 !string.IsNullOrWhiteSpace(headerUser.ToString())
                        ? headerUser.ToString()
                        : "system";

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId),
                        new Claim("tenantId", tenantId)
                    };

                    if (Request.Headers.TryGetValue("X-Empresa-Id", out var headerEmpresa) &&
                        !string.IsNullOrWhiteSpace(headerEmpresa.ToString()))
                    {
                        claims.Add(new Claim("empresaId", headerEmpresa.ToString()));
                    }

                    var identity = new ClaimsIdentity(claims, SchemeName);
                    var principal = new ClaimsPrincipal(identity);
                    return Task.FromResult(AuthenticateResult.Success(
                        new AuthenticationTicket(principal, SchemeName)));
                }
            }

            // Sem credencial válida: NoResult -> a FallbackPolicy/[Authorize] responde 401.
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        private static ClaimsPrincipal? TentarConstruirPrincipalDeToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var partes = token.Split('-');

            var indexBasico = Array.IndexOf(partes, "basico");
            var indexCompleto = Array.IndexOf(partes, "completo");

            string? tenantId = null;
            string? usuarioId = null;
            string? empresaId = null;
            string? perfilId = null;

            if (indexCompleto != -1 && partes.Length > indexCompleto + 3)
            {
                tenantId = partes[indexCompleto + 1];
                usuarioId = partes[indexCompleto + 2];
                empresaId = partes[indexCompleto + 3];
                if (partes.Length > indexCompleto + 4)
                {
                    perfilId = partes[indexCompleto + 4];
                }
            }
            else if (indexBasico != -1 && partes.Length > indexBasico + 2)
            {
                tenantId = partes[indexBasico + 1];
                usuarioId = partes[indexBasico + 2];
            }

            if (string.IsNullOrEmpty(tenantId) || string.IsNullOrEmpty(usuarioId))
            {
                return null;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuarioId),
                new Claim("tenantId", tenantId)
            };

            if (!string.IsNullOrEmpty(empresaId) && empresaId != "null")
            {
                claims.Add(new Claim("empresaId", empresaId));
            }
            if (!string.IsNullOrEmpty(perfilId) && perfilId != "null")
            {
                claims.Add(new Claim("perfilId", perfilId));
            }

            var identity = new ClaimsIdentity(claims, SchemeName);
            return new ClaimsPrincipal(identity);
        }
    }

    /// <summary>
    /// Sinaliza para o handler de autenticação se o ambiente atual permite o atalho de
    /// autenticação por headers (X-Tenant-Id / X-User-Id). Verdadeiro em Development e Testing;
    /// falso em Produção, onde apenas o token estruturado autentica.
    /// </summary>
    public interface IHostEnvironmentFlags
    {
        bool PermiteHeadersDeAutenticacao { get; }
    }

    public sealed class HostEnvironmentFlags : IHostEnvironmentFlags
    {
        public HostEnvironmentFlags(bool permiteHeadersDeAutenticacao)
        {
            PermiteHeadersDeAutenticacao = permiteHeadersDeAutenticacao;
        }

        public bool PermiteHeadersDeAutenticacao { get; }
    }
}
