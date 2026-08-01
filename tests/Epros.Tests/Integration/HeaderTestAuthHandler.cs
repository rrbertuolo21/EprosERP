using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Epros.Tests.Integration
{
    /// <summary>
    /// Handler de autenticação injetado SOMENTE no host de teste (WebApplicationFactory).
    ///
    /// Substitui, nos testes de integração, o antigo atalho de header que existia no runtime
    /// (removido para fechar o "gato"). Aqui a identidade é injetada pelo host de teste — a partir
    /// dos headers X-Tenant-Id / X-User-Id / X-Empresa-Id / perfilId — e produz o MESMO shape de
    /// claims que o token assinado de produção (tenantId, NameIdentifier, empresaId, perfilId),
    /// para que a FallbackPolicy e o pipeline (InquilinoSaaSMiddleware, AbacFilter) funcionem sem
    /// nenhum caminho de header no binário deployado.
    ///
    /// É registrado sobre o MESMO nome de esquema do runtime (EprosToken) via remap de HandlerType
    /// na factory de teste, então a autorização (que autentica pelo esquema "EprosToken") o utiliza.
    /// </summary>
    public class HeaderTestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string TenantHeader = "X-Tenant-Id";
        public const string UserHeader = "X-User-Id";
        public const string EmpresaHeader = "X-Empresa-Id";
        public const string PerfilHeader = "X-Perfil-Id";
        public const string PrimaryAdminHeader = "X-Primary-Admin";
        public const string PerfilSuporteHeader = "X-Perfil-Suporte";

        public HeaderTestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue(TenantHeader, out var headerTenant) ||
                string.IsNullOrWhiteSpace(headerTenant.ToString()))
            {
                // Sem tenant no host de teste = requisição anônima -> a FallbackPolicy responde 401.
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var tenantId = headerTenant.ToString();
            var userId = Request.Headers.TryGetValue(UserHeader, out var headerUser) &&
                         !string.IsNullOrWhiteSpace(headerUser.ToString())
                ? headerUser.ToString()
                : "system";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim("tenantId", tenantId)
            };

            if (Request.Headers.TryGetValue(EmpresaHeader, out var headerEmpresa) &&
                !string.IsNullOrWhiteSpace(headerEmpresa.ToString()))
            {
                claims.Add(new Claim("empresaId", headerEmpresa.ToString()));
            }

            // perfilId="interno" identifica o operador interno da Siser (curto-circuito do AbacFilter).
            var ehInterno = false;
            if (Request.Headers.TryGetValue(PerfilHeader, out var headerPerfil) &&
                !string.IsNullOrWhiteSpace(headerPerfil.ToString()))
            {
                var perfil = headerPerfil.ToString();
                claims.Add(new Claim("perfilId", perfil));
                ehInterno = string.Equals(perfil, "interno", System.StringComparison.OrdinalIgnoreCase);
            }

            // 1.11: para o operador interno, injeta a FAIXA de suporte (primaryAdmin/perfilSuporte)
            // no MESMO shape do token de produção. Default: PrimaryAdmin (Lord pleno) quando não
            // especificado, para não travar fluxos de teste que só querem "acesso de operador Siser".
            if (ehInterno)
            {
                var primaryAdmin = !Request.Headers.TryGetValue(PrimaryAdminHeader, out var hPrimary) ||
                                   string.IsNullOrWhiteSpace(hPrimary.ToString()) ||
                                   string.Equals(hPrimary.ToString(), "true", System.StringComparison.OrdinalIgnoreCase);
                claims.Add(new Claim("primaryAdmin", primaryAdmin ? "true" : "false"));

                var perfilSuporte = Request.Headers.TryGetValue(PerfilSuporteHeader, out var hSup) &&
                                    !string.IsNullOrWhiteSpace(hSup.ToString())
                    ? hSup.ToString()
                    : "Nenhum";
                claims.Add(new Claim("perfilSuporte", perfilSuporte));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(principal, Scheme.Name)));
        }
    }
}
