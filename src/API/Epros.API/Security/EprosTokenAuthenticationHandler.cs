using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Epros.Shared.Security;

namespace Epros.API.Security
{
    /// <summary>
    /// Esquema de autenticação nativo do EprosERP.
    ///
    /// A autenticação real da plataforma (enquanto o Keycloak/JWT real não é o mecanismo ativo)
    /// usa JWTs assinados (HS256, com expiração) emitidos por <c>AutenticarUsuarioCommandHandler</c>
    /// via <see cref="Epros.Shared.Security.IEprosTokenService"/> — token básico ou completo.
    ///
    /// Este handler valida esses tokens (assinatura + expiração + emissor/audiência) via
    /// <c>IEprosTokenService.Validar</c> e materializa um <see cref="ClaimsPrincipal"/> autenticado
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
        private readonly IEprosTokenService _tokenService;
        private readonly IMemoryCache _cache;

        public EprosTokenAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            IHostEnvironmentFlags env,
            IEprosTokenService tokenService,
            IMemoryCache cache)
            : base(options, logger, encoder)
        {
            _env = env;
            _tokenService = tokenService;
            _cache = cache;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // 1. Autenticação via token estruturado (mecanismo de produção)
            var authHeader = Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader))
            {
                var token = authHeader.Replace("Bearer ", "").Trim();
                var principal = _tokenService.Validar(token);
                if (principal != null)
                {
                    // Revogação de sessão (logout — REG-013): se o usuário fez logout depois de este
                    // token ser emitido, rejeita. Fonte de verdade rápida em cache (por usuário);
                    // cobre qualquer token (básico/completo) emitido antes do marco de logout.
                    if (TokenRevogadoPorLogout(principal))
                    {
                        return Task.FromResult(AuthenticateResult.Fail("Sessão encerrada (logout)."));
                    }

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

        /// <summary>
        /// True se existe um marco de logout para o usuário (cache) posterior à emissão do token —
        /// i.e., o usuário deslogou depois que este token foi emitido, logo o token está revogado.
        /// </summary>
        private bool TokenRevogadoPorLogout(ClaimsPrincipal principal)
        {
            var usuarioId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId))
            {
                return false;
            }

            if (!_cache.TryGetValue(RevogacaoSessao.ChaveCache(usuarioId), out DateTime logoutEm))
            {
                return false;
            }

            // Instante de emissão do token: claim "iat" (fallback "nbf"), em segundos Unix (UTC).
            var iatClaim = principal.FindFirst("iat")?.Value ?? principal.FindFirst("nbf")?.Value;
            if (long.TryParse(iatClaim, out var iatSeconds))
            {
                var emitidoEm = DateTimeOffset.FromUnixTimeSeconds(iatSeconds).UtcDateTime;
                return emitidoEm < logoutEm;
            }

            // Sem instante de emissão legível: mais seguro revogar (usuário fez logout).
            return true;
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
