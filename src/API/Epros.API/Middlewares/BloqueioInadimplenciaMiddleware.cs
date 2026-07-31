using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Epros.API.Middlewares
{
    public class BloqueioInadimplenciaMiddleware
    {
        private readonly RequestDelegate _next;

        public BloqueioInadimplenciaMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

            // Rotas liberadas para consulta e regularização de faturas
            if (path.Contains("/api/v1/public") || 
                path.Contains("/api/v1/aplicativo/assinaturas/faturas") ||
                path.Contains("/api/v1/aplicativo/usuarios/preferencias") ||
                path.Contains("/auth") || 
                path.Contains("/login"))
            {
                await _next(context);
                return;
            }

            // Resolve os serviços do escopo atual da requisição
            using (var scope = context.RequestServices.CreateScope())
            {
                var tenantProvider = scope.ServiceProvider.GetRequiredService<ITenantProvider>();
                var tenantId = tenantProvider.GetTenantId();

                // Ignora se for o tenant do sistema ou nulo
                if (!string.IsNullOrEmpty(tenantId) && tenantId != "system")
                {
                    var contextGestao = scope.ServiceProvider.GetRequiredService<ContextGestaoClientes>();

                    var cliente = await contextGestao.Clientes
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null);

                    if (cliente != null)
                    {
                        // 1.06 — prazo de tolerância por PLANO (antes era 15 hardcoded). Lê o valor
                        // do plano contratado; fallback 15 dias se ausente/nulo ou plano não resolvido.
                        int diasTolerancia = 15;
                        if (cliente.PlanoId != Guid.Empty)
                        {
                            var diasPlano = await contextGestao.Planos
                                .IgnoreQueryFilters()
                                .Where(p => p.Id == cliente.PlanoId && p.DeletadoEm == null)
                                .Select(p => p.DiasToleranciaInadimplencia)
                                .FirstOrDefaultAsync();
                            if (diasPlano.HasValue && diasPlano.Value > 0)
                            {
                                diasTolerancia = diasPlano.Value;
                            }
                        }

                        var dataLimite = DateTime.UtcNow.AddDays(-diasTolerancia);

                        var inadimplente = await contextGestao.Faturas
                            .IgnoreQueryFilters()
                            .AnyAsync(f => f.ClienteId == cliente.Id &&
                                           f.Status != FaturaStatus.Paga &&
                                           f.Status != FaturaStatus.Cancelada &&
                                           f.DataVencimento < dataLimite &&
                                           f.DeletadoEm == null);

                        if (inadimplente)
                        {
                            context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync("{\"error\": \"Acesso operacional bloqueado por inadimplência. Por favor, regularize as suas faturas vencidas na área de pagamentos.\"}");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
