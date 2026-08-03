using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Epros.Modules.GestaoClientes.Infrastructure.Data;

namespace Epros.API.Middlewares
{
    /// <summary>
    /// 1.06 — Entitlement de módulo REAL (substitui o stub demonstrativo). Autoriza a requisição
    /// cruzando a ROTA com a FLAG de módulo do plano do tenant: se a rota pertence a um módulo
    /// contratável e o plano do tenant NÃO possui a flag correspondente, responde 403 (módulo não
    /// contratado). Rotas de plataforma/auth/onboarding e cadastros base ficam sempre liberadas.
    ///
    /// GO-LIVE (registrado na MC): planos criados hoje nascem com as flags de módulo em <c>false</c>
    /// (ver CriarPlanoCommandHandler / construtor de Plano). Antes de ligar este gate em produção é
    /// preciso semear/atualizar os planos reais com as flags corretas, senão tenants legítimos com
    /// plano de flags falsas seriam bloqueados. Fail-safe atual: quando não há cliente/plano resolvível
    /// para o tenant, a requisição passa (mesma postura do InquilinoSaaSMiddleware).
    /// </summary>
    public class ModuloTenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ModuloTenantMiddleware> _logger;

        public ModuloTenantMiddleware(RequestDelegate next, ILogger<ModuloTenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Mapa explícito ROTA (prefixo) → FLAG de módulo do plano. A ordem importa: usamos o
        /// primeiro prefixo que casar. Só entram rotas cujo módulo é DONO da funcionalidade; tudo
        /// que não estiver aqui é tratado como plataforma/cadastro base e passa sem gate.
        /// OBS: não há rota de CRM hoje; a flag ModuloCrm fica sem ponto de consumo até existir
        /// um controller de CRM (registrado na MC como "ligar quando houver módulo CRM").
        /// </summary>
        private static readonly (string Prefixo, string Modulo, Func<ModulosPlanoInfo, bool> TemFlag)[] MapaRotaModulo =
            new (string, string, Func<ModulosPlanoInfo, bool>)[]
            {
                // Financeiro — financeiro é o dono; tesouraria/contas/contratos financeiros seguem o mesmo plano.
                ("/api/v1/financeiro", "Financeiro", m => m.ModuloFinanceiro),
                // RH.
                ("/api/v1/rh", "RH", m => m.ModuloRh),
                // Projetos.
                ("/api/v1/projetos", "Projetos", m => m.ModuloProjetos),
                // PDV / Vendas (todas as variantes vendas-*).
                ("/api/v1/vendas", "PDV/Vendas", m => m.ModuloPdv),
            };

        public async Task InvokeAsync(HttpContext context, ContextGestaoClientes contextGestao, IMemoryCache memoryCache)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
            var tenantId = context.Items["TenantId"] as string;

            // Plataforma/dev/sistema: nunca barram por módulo.
            if (string.IsNullOrEmpty(tenantId) || tenantId == "system" || tenantId == "tenant-padrao")
            {
                await _next(context);
                return;
            }

            // Endpoints [AllowAnonymous] (auth, health, swagger, webhooks, instalação) não têm módulo a barrar.
            var permiteAnonimo = context.GetEndpoint()?.Metadata
                .GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() != null;
            if (permiteAnonimo)
            {
                await _next(context);
                return;
            }

            // A rota pertence a algum módulo contratável?
            var regra = MapaRotaModulo.FirstOrDefault(r => path.StartsWith(r.Prefixo, StringComparison.OrdinalIgnoreCase));
            if (regra.Prefixo == null)
            {
                await _next(context); // rota de plataforma/cadastro base
                return;
            }

            // Resolve as flags de módulo do plano do tenant (cacheado 60s por tenant).
            var info = await memoryCache.GetOrCreateAsync($"tenant_modulos:{tenantId}", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);

                var cliente = await contextGestao.Clientes
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.TenantId == tenantId && c.Ativo && c.DeletadoEm == null);

                if (cliente == null || cliente.PlanoId == Guid.Empty)
                {
                    return null; // fail-safe: sem cliente/plano, não impõe gate
                }

                return await contextGestao.Planos
                    .IgnoreQueryFilters()
                    .Where(p => p.Id == cliente.PlanoId && p.Ativo && p.DeletadoEm == null)
                    .Select(p => new ModulosPlanoInfo
                    {
                        ModuloCrm = p.ModuloCrm,
                        ModuloProjetos = p.ModuloProjetos,
                        ModuloRh = p.ModuloRh,
                        ModuloFinanceiro = p.ModuloFinanceiro,
                        ModuloPdv = p.ModuloPdv
                    })
                    .FirstOrDefaultAsync();
            });

            // Fail-safe: plano não resolvido → não barra (outras camadas de status/inadimplência tratam).
            if (info == null)
            {
                await _next(context);
                return;
            }

            if (!regra.TemFlag(info))
            {
                _logger.LogWarning("Acesso negado por entitlement: módulo {Modulo} não contratado pelo Tenant {TenantId} (rota {Rota}).",
                    regra.Modulo, tenantId, path);
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    $"{{\"error\": \"Módulo não contratado: o módulo '{regra.Modulo}' não está incluído no plano atual do inquilino.\", \"code\": \"modulo_nao_contratado\"}}");
                return;
            }

            await _next(context);
        }

        private sealed class ModulosPlanoInfo
        {
            public bool ModuloCrm { get; set; }
            public bool ModuloProjetos { get; set; }
            public bool ModuloRh { get; set; }
            public bool ModuloFinanceiro { get; set; }
            public bool ModuloPdv { get; set; }
        }
    }
}
