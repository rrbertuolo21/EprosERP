using System;
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

            // Fallback para Header X-Tenant-Id (facilidade de desenvolvimento/testes locais)
            if (string.IsNullOrEmpty(tenantId))
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

            // 3. Enriquecer Serilog log context com o tenantId para rastreamento centralizado
            using (LogContext.PushProperty("TenantId", tenantId))
            {
                await _next(context);
            }
        }
    }
}
