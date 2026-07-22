using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Aplicativo.Infrastructure.Data;

namespace Epros.API.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiKeyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IMemoryCache memoryCache)
        {
            if (context.Request.Headers.TryGetValue("X-API-Key", out var extractedApiKey))
            {
                var apiKey = extractedApiKey.ToString();
                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Não autorizado: Chave de API inválida ou ausente.\"}");
                    return;
                }

                // Cria escopo para obter o DbContext
                using var scope = context.RequestServices.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ContextAplicativo>();

                var usuario = await dbContext.Usuarios
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(u => u.ApiKey == apiKey && u.DeletadoEm == null);

                if (usuario == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Não autorizado: Chave de API inválida ou inexistente.\"}");
                    return;
                }

                // Verifica a expiração (política de rotação periódica)
                if (usuario.ApiKeyExpiration.HasValue && usuario.ApiKeyExpiration.Value < DateTime.UtcNow)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Não autorizado: Chave de API expirada. Rotação periódica obrigatória é necessária.\"}");
                    return;
                }

                // Limite dinâmico de Rate Limit por minuto
                var limit = usuario.ApiKeyRateLimit > 0 ? usuario.ApiKeyRateLimit : 60;
                var currentMinute = DateTime.UtcNow.ToString("yyyyMMddHHmm");
                var cacheKey = $"ratelimit:{apiKey}:{currentMinute}";

                var requestCount = memoryCache.GetOrCreate(cacheKey, entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2); // Evita memory leaks limpando rapidamente
                    return 0;
                });

                if (requestCount >= limit)
                {
                    context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"error\": \"Limite de requisições excedido para esta API Key (Rate Limit). Tente novamente no próximo minuto.\"}");
                    return;
                }

                // Incrementa a contagem de requisições
                memoryCache.Set(cacheKey, requestCount + 1, TimeSpan.FromMinutes(2));

                // Atualiza a data de último uso no banco
                usuario.AtualizarUltimoUsoApiKey();
                await dbContext.SaveChangesAsync();

                // Define as Claims de identidade no contexto
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Name, usuario.Nome),
                    new Claim("tenantId", usuario.TenantId)
                };

                var identity = new ClaimsIdentity(claims, "ApiKey");
                var principal = new ClaimsPrincipal(identity);
                context.User = principal;

                // Define o tenantId nos itens da requisição para o InquilinoSaaSMiddleware
                context.Items["TenantId"] = usuario.TenantId;
            }

            await _next(context);
        }
    }
}
