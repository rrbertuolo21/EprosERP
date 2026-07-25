using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Epros.API.Middlewares
{
    public class ModuloTenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ModuloTenantMiddleware> _logger;

        public ModuloTenantMiddleware(RequestDelegate next, ILogger<ModuloTenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower() ?? string.Empty;
            var tenantId = context.Items["TenantId"] as string ?? "tenant-padrao";

            // Regra demonstrativa de Entitlements (a ser acoplada com cache Valkey ou banco de dados GestaoClientes posteriormente)
            // Caso o tenant seja "tenant-teste-bloqueado" e tente acessar o módulo financeiro, bloqueamos o acesso.
            if (path.Contains("/api/v1/financas") && tenantId == "tenant-teste-bloqueado")
            {
                _logger.LogWarning("Acesso negado: Módulo Financeiro não habilitado para o Tenant {TenantId}.", tenantId);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"error\": \"Acesso Proibido: Módulo desabilitado para o plano atual do inquilino.\"}");
                return;
            }

            await _next(context);
        }
    }
}
