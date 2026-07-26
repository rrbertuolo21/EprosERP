using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Epros.Shared.Application.Contracts;

namespace Epros.API.Middlewares
{
    public class AuditMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ICurrentUser currentUser, ITenantProvider tenantProvider)
        {
            var request = context.Request;
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userId = currentUser.GetUserId() ?? "anonymous";
            var tenantId = tenantProvider.GetTenantId();
            var path = request.Path;
            var method = request.Method;

            // Gera log estruturado no Serilog com rótulo AUDIT_TRAIL (coletado pelo Grafana Loki)
            // Em conformidade com LGPD Art. 37 e PCI DSS Req. 10.
            _logger.LogInformation(
                "AUDIT_TRAIL: Usuario={UserId} | Tenant={TenantId} | IP={Ip} | Metodo={Method} | Rota={Path} | Data={Timestamp}",
                userId, tenantId, ipAddress, method, path, DateTime.UtcNow);

            await _next(context);
        }
    }
}
