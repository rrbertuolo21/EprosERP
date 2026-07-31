using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.GestaoClientes.Infrastructure.Jobs
{
    /// <summary>
    /// 1.08A — Job diário que encerra os trials expirados (fecha o ciclo 1.07). Agrupa por tenant os
    /// trials com <c>TrialAte &lt; agora</c> ainda não convertidos e dispara o
    /// <see cref="EncerrarTrialsExpiradosCommand"/> por tenant (gera fatura + cobrança + transição de status).
    /// Idempotente (o handler marca <c>TrialConvertidoEm</c>).
    /// </summary>
    [DisallowConcurrentExecution]
    public class EncerrarTrialsExpiradosJob : IJob
    {
        private readonly ContextGestaoClientes _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EncerrarTrialsExpiradosJob(
            ContextGestaoClientes context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var hoje = DateTime.UtcNow;

            var tenantsComTrialExpirado = await _context.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.Status == AssinaturaStatus.Ativa
                            && a.TrialAte != null
                            && a.TrialAte < hoje
                            && a.TrialConvertidoEm == null
                            && !a.Arquivada
                            && a.DeletadoEm == null)
                .Select(a => a.TenantId)
                .Distinct()
                .ToListAsync();

            foreach (var tenantId in tenantsComTrialExpirado)
            {
                try
                {
                    var httpContext = new DefaultHttpContext();
                    httpContext.Items["TenantId"] = tenantId;
                    _httpContextAccessor.HttpContext = httpContext;

                    var result = await _mediator.Send(new EncerrarTrialsExpiradosCommand(hoje));
                    if (!result.Sucesso)
                        Console.WriteLine($"[Quartz] Falha ao encerrar trials do Tenant {tenantId}: {string.Join(", ", result.Erros)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Quartz] [Erro] Exceção ao encerrar trials do Tenant {tenantId}: {ex.Message}");
                }
            }
        }
    }
}
