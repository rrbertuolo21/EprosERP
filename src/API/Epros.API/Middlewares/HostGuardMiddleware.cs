using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Epros.API.Middlewares
{
    /// <summary>
    /// Host-guard do Landlord (fechamento do módulo APLICATIVO — 1.04, topologia de endereçamento).
    ///
    /// Defesa em profundidade sobre o gate real (1.11: <c>tenant=system</c> + <c>perfilId=interno</c>):
    /// as rotas do painel do Landlord (Siser) só respondem no HOST próprio do Landlord
    /// (ex.: <c>painel.siser…</c> / <c>admin.epros…</c>); num host de cliente elas devolvem <b>404</b>
    /// (não revelam a superfície). Opcionalmente, num host do Landlord as rotas de cliente/tenant
    /// também devolvem 404 (inverso), quando <c>Hosts:Cliente</c> estiver configurado.
    ///
    /// FAIL-SAFE (dev/test): se <c>Hosts:Landlord</c> NÃO estiver configurado (dev local, testes),
    /// o middleware NÃO bloqueia nada — o gate efetivo continua sendo <c>perfilId=interno</c> (1.11).
    /// Isso preserva a suíte de testes (que bate nesses endpoints sem host configurado).
    /// ⚠️ Em produção <c>Hosts:Landlord</c> DEVE ser configurado; sem ele o host-guard fica inativo.
    /// </summary>
    public class HostGuardMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HostGuardMiddleware> _logger;
        private readonly HostGuardOptions _options;

        /// <summary>
        /// Superfície de rotas do Landlord (fonte única). Prefixos comparados case-insensitive.
        /// Cobre: SuperAdmin (inclui restaurar), landlord/suporte, governança de versão (super-admin),
        /// relatórios financeiros do landlord, faturas do landlord e instalação/bootstrap.
        /// </summary>
        private static readonly string[] PrefixosLandlord = new[]
        {
            "/api/v1/plataforma/superadmin",
            "/api/v1/plataforma/landlord",
            "/api/v1/landlord",
            "/api/v1/super-admin",
            "/api/v1/plataforma/relatorios-financeiros",
            "/api/v1/plataforma/faturas",
            "/api/v1/installation",
        };

        public HostGuardMiddleware(
            RequestDelegate next,
            ILogger<HostGuardMiddleware> logger,
            IOptions<HostGuardOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var hostsLandlord = _options.Landlord ?? Array.Empty<string>();

            // FAIL-SAFE: sem Hosts:Landlord configurado, o guard é inativo (preserva dev/test e os
            // 1034 testes que batem nas rotas landlord sem host). O gate real (1.11) segue valendo.
            if (hostsLandlord.Length == 0)
            {
                await _next(context);
                return;
            }

            var host = context.Request.Host.Host; // hostname sem porta
            var path = context.Request.Path.Value ?? string.Empty;

            var ehHostLandlord = hostsLandlord.Any(h => string.Equals(h, host, StringComparison.OrdinalIgnoreCase));
            var ehRotaLandlord = PrefixosLandlord.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

            // 1) Rota do Landlord fora do host do Landlord -> 404 (não revela a superfície).
            if (ehRotaLandlord && !ehHostLandlord)
            {
                _logger.LogWarning(
                    "Host-guard: rota Landlord '{Path}' bloqueada no host '{Host}' (fora de Hosts:Landlord). 404.",
                    path, host);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            // 2) Inverso (opcional): rota de cliente/tenant NÃO responde no host do Landlord.
            //    Só ativa se Hosts:Cliente estiver configurado (opt-in explícito).
            var hostsCliente = _options.Cliente ?? Array.Empty<string>();
            if (ehHostLandlord && !ehRotaLandlord && hostsCliente.Length > 0 && !EhRotaInfra(path))
            {
                _logger.LogWarning(
                    "Host-guard: rota de cliente '{Path}' bloqueada no host do Landlord '{Host}'. 404.",
                    path, host);
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            await _next(context);
        }

        /// <summary>
        /// Rotas de infraestrutura sempre liberadas (health, swagger) para não travar sonda/diagnóstico
        /// no host do Landlord quando o guard inverso está ativo.
        /// </summary>
        private static bool EhRotaInfra(string path) =>
            path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Configuração do host-guard (seção <c>Hosts</c> do appsettings/ambiente).
    /// <c>Hosts:Landlord</c> — hostnames do painel do Landlord (Siser). Vazio = guard inativo (fail-safe).
    /// <c>Hosts:Cliente</c> — hostnames de cliente (opcional; ativa o guard inverso).
    /// </summary>
    public class HostGuardOptions
    {
        public const string SecaoConfig = "Hosts";

        public string[] Landlord { get; set; } = Array.Empty<string>();
        public string[] Cliente { get; set; } = Array.Empty<string>();
    }
}
