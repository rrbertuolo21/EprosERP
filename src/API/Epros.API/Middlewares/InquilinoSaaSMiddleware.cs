using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Serilog.Context;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.API.Middlewares
{
    public class InquilinoSaaSMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<InquilinoSaaSMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public InquilinoSaaSMiddleware(RequestDelegate next, ILogger<InquilinoSaaSMiddleware> logger, IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context, ContextGestaoClientes contextGestaoClientes, IMemoryCache memoryCache)
        {
            // 1. Resolver Tenant
            string? tenantId = null;

            // Tenta obter do usuário autenticado (claim tenantId — token estruturado ou JWT)
            if (context.User.Identity?.IsAuthenticated == true)
            {
                tenantId = context.User.FindFirst("tenantId")?.Value;
            }

            // SEGURANÇA (fechamento do "gato"): o fallback de tenant por header X-Tenant-Id é uma
            // conveniência EXCLUSIVA de desenvolvimento local. No runtime deployado o tenant vem
            // sempre da claim do token assinado (acima); nenhum header controla o inquilino.
            if (string.IsNullOrEmpty(tenantId) && _environment.IsDevelopment())
            {
                if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerTenantId))
                {
                    tenantId = headerTenantId.ToString();
                }
            }

            // S2: o fallback anônimo "tenant-padrao" é aceitável APENAS em desenvolvimento.
            // Em produção, ausência de tenant/usuário resolvido é uma requisição não autenticada -> 401,
            // EXCETO em endpoints marcados [AllowAnonymous] (login/auth, health, swagger, webhooks),
            // que legitimamente não têm inquilino. Usa-se a mesma metadata que a camada de autorização.
            if (string.IsNullOrEmpty(tenantId))
            {
                if (_environment.IsDevelopment())
                {
                    tenantId = "tenant-padrao";
                }
                else
                {
                    var permiteAnonimo = context.GetEndpoint()?.Metadata
                        .GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;

                    if (!permiteAnonimo)
                    {
                        _logger.LogWarning("Requisição sem tenant resolvido em ambiente '{Env}': rejeitada com 401.", _environment.EnvironmentName);
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync("{\"error\": \"Não autorizado: contexto de inquilino (tenant) ausente ou inválido.\"}");
                        return;
                    }
                }
            }

            // 2. Colocar nos itens do HttpContext para o TenantProvider
            context.Items["TenantId"] = tenantId;

            // 2.1. Resolver se é Demo
            bool isDemo = false;

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var isDemoClaim = context.User.FindFirst("isDemo")?.Value ??
                                  context.User.FindFirst("is_demo")?.Value;
                if (!string.IsNullOrEmpty(isDemoClaim) && bool.TryParse(isDemoClaim, out var parsedIsDemo))
                {
                    isDemo = parsedIsDemo;
                }
            }

            if (!isDemo)
            {
                if (context.Request.Headers.TryGetValue("X-Tenant-Is-Demo", out var headerIsDemo))
                {
                    if (bool.TryParse(headerIsDemo.ToString(), out var parsedIsDemo))
                    {
                        isDemo = parsedIsDemo;
                    }
                }
            }

            if (!isDemo && !string.IsNullOrEmpty(tenantId))
            {
                var cacheKey = $"tenant_is_demo:{tenantId}";
                isDemo = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                    var cli = await contextGestaoClientes.Clientes
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.DeletadoEm == null);
                    return cli?.IsDemo ?? false;
                });
            }

            context.Items["IsDemo"] = isDemo;

            // 2.2. Gating por StatusSaaS (REG-021 / 1.05). Bloqueia/limita a operação conforme o
            // ciclo de assinatura do tenant, ANTES de alcançar qualquer controller.
            if (await BloqueadoPorStatusSaaSAsync(context, tenantId, contextGestaoClientes, memoryCache))
            {
                return; // resposta já escrita
            }

            // 3. Enriquecer Serilog log context com o tenantId para rastreamento centralizado
            using (LogContext.PushProperty("TenantId", tenantId))
            {
                await _next(context);
            }
        }

        /// <summary>
        /// Aplica o gating por StatusSaaS do tenant (REG-021). Retorna true se a requisição foi
        /// bloqueada (resposta já escrita). Regras (decididas com o Rafael, 1.05):
        ///   - Ativo / TrialGratuito: opera normalmente.
        ///   - AguardandoPagamento: opera (regra dos 15 dias tratada no BloqueioInadimplenciaMiddleware).
        ///   - Cancelado / Falha: operação bloqueada; somente-leitura (GET/HEAD) liberada por 30 dias
        ///     a partir da data do cancelamento para exportar dados/faturas; depois, bloqueio total.
        ///   - SemAssinatura: só onboarding/contratação (ERP bloqueado até assinar).
        /// Rotas de autenticação, públicas, de instalação e de regularização/assinatura ficam sempre
        /// liberadas para o tenant poder pagar, reativar e exportar. Endpoints [AllowAnonymous] e o
        /// tenant "system" são ignorados.
        /// </summary>
        private static async Task<bool> BloqueadoPorStatusSaaSAsync(
            HttpContext context, string? tenantId, ContextGestaoClientes contextGestao, IMemoryCache memoryCache)
        {
            if (string.IsNullOrEmpty(tenantId) || tenantId == "system" || tenantId == "tenant-padrao")
            {
                return false;
            }

            // Endpoints públicos (auth, health, swagger, instalação, webhooks) não têm inquilino a barrar.
            var permiteAnonimo = context.GetEndpoint()?.Metadata
                .GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;
            if (permiteAnonimo)
            {
                return false;
            }

            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            // Rotas sempre liberadas: autenticação e regularização/saída (pagar, ver/gerir assinatura
            // e faturas, preferências do usuário). Sem elas o tenant bloqueado não conseguiria reativar.
            bool RotaRegularizacaoOuAuth() =>
                path.Contains("/auth") ||
                path.Contains("/login") ||
                path.Contains("/logout") ||
                path.Contains("/api/v1/public") ||
                path.Contains("/api/v1/installation") ||
                path.Contains("/api/v1/aplicativo/assinaturas") ||
                path.Contains("/api/v1/aplicativo/usuarios/preferencias");

            // Rotas de onboarding/contratação (para tenant SemAssinatura escolher e assinar um plano).
            bool RotaOnboarding() =>
                path.Contains("/onboarding") ||
                path.Contains("/contratacao") ||
                path.Contains("/api/v1/plataforma/planos");

            // Status do tenant (cacheado 60s para não onerar cada request).
            var cacheKey = $"tenant_status_saas:{tenantId}";
            var status = await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                var cli = await contextGestao.Clientes
                    .IgnoreQueryFilters()
                    .Where(c => c.TenantId == tenantId && c.DeletadoEm == null)
                    .Select(c => new StatusSaaSInfo
                    {
                        Existe = true,
                        Status = c.StatusSaaS,
                        Desde = c.StatusSaaSAtualizadoEm ?? c.AlteradoEm ?? c.CriadoEm
                    })
                    .FirstOrDefaultAsync();
                return cli ?? new StatusSaaSInfo { Existe = false };
            });

            // Sem registro de Cliente para o tenant: não há status a impor aqui (outras camadas tratam).
            if (status == null || !status.Existe)
            {
                return false;
            }

            var ehSomenteLeitura = HttpMethods.IsGet(context.Request.Method)
                || HttpMethods.IsHead(context.Request.Method)
                || HttpMethods.IsOptions(context.Request.Method);

            switch (status.Status)
            {
                case Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS.Ativo:
                case Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS.TrialGratuito:
                case Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS.AguardandoPagamento:
                    return false;

                case Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS.SemAssinatura:
                    if (RotaRegularizacaoOuAuth() || RotaOnboarding())
                    {
                        return false;
                    }
                    await EscreverBloqueioAsync(context, StatusCodes.Status403Forbidden,
                        "sem_assinatura",
                        "Nenhum plano contratado. Assine um plano para liberar o ERP.");
                    return true;

                case Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS.Cancelado:
                case Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS.Falha:
                    if (RotaRegularizacaoOuAuth())
                    {
                        return false; // pagar/reativar/exportar faturas sempre liberado
                    }

                    var dentroJanelaExportacao = (DateTime.UtcNow - status.Desde) <= TimeSpan.FromDays(30);
                    if (dentroJanelaExportacao && ehSomenteLeitura)
                    {
                        return false; // somente-leitura liberada por 30 dias para exportar dados
                    }

                    var msg = dentroJanelaExportacao
                        ? "Assinatura cancelada: operação bloqueada. Apenas leitura/exportação está liberada por 30 dias. Regularize para reativar."
                        : "Assinatura cancelada há mais de 30 dias: acesso bloqueado. Regularize para reativar o ERP.";
                    await EscreverBloqueioAsync(context, StatusCodes.Status403Forbidden, "assinatura_cancelada", msg);
                    return true;

                default:
                    return false;
            }
        }

        private static async Task EscreverBloqueioAsync(HttpContext context, int statusCode, string codigo, string mensagem)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            var payload = System.Text.Json.JsonSerializer.Serialize(new { error = mensagem, code = codigo });
            await context.Response.WriteAsync(payload);
        }

        private sealed class StatusSaaSInfo
        {
            public bool Existe { get; set; }
            public Epros.Modules.GestaoClientes.Domain.Entities.StatusSaaS Status { get; set; }
            public DateTime Desde { get; set; }
        }
    }
}
