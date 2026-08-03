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
    /// 1.08B — Job diário que RENOVA as assinaturas cujo ciclo venceu. Agrupa por tenant as assinaturas
    /// Ativas com <c>ProximaCobrancaEm &lt;= agora</c> e dispara o <see cref="ProcessarRenovacaoAssinaturasCommand"/>
    /// por tenant (gera a próxima fatura conforme a duração do plano + cobrança método-agnóstica). Idempotente.
    /// </summary>
    [DisallowConcurrentExecution]
    public class ProcessarRenovacaoAssinaturasJob : IJob
    {
        private readonly ContextGestaoClientes _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProcessarRenovacaoAssinaturasJob(
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

            var tenants = await _context.AssinaturasClientes
                .IgnoreQueryFilters()
                .Where(a => a.Status == AssinaturaStatus.Ativa
                            && a.ProximaCobrancaEm != null
                            && a.ProximaCobrancaEm <= hoje
                            && !a.Arquivada
                            && a.DeletadoEm == null)
                .Select(a => a.TenantId)
                .Distinct()
                .ToListAsync();

            foreach (var tenantId in tenants)
            {
                try
                {
                    var httpContext = new DefaultHttpContext();
                    httpContext.Items["TenantId"] = tenantId;
                    _httpContextAccessor.HttpContext = httpContext;

                    var result = await _mediator.Send(new ProcessarRenovacaoAssinaturasCommand(hoje));
                    if (!result.Sucesso)
                        Console.WriteLine($"[Quartz] Falha ao renovar assinaturas do Tenant {tenantId}: {string.Join(", ", result.Erros)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Quartz] [Erro] Exceção ao renovar assinaturas do Tenant {tenantId}: {ex.Message}");
                }
            }
        }
    }
}
