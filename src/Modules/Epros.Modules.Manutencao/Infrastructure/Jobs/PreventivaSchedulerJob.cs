using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Manutencao.Infrastructure.Jobs
{
    /// <summary>
    /// MAN-PRV — D7: scheduler recorrente que aciona o motor de vencimento por tenant.
    /// Descobre os tenants com plano preventivo ATIVO (ignora o filtro de tenant), define o contexto de
    /// cada tenant e delega ao AvaliarVencimentoPreventivaCommand (motor sob demanda) — o mesmo caminho
    /// do endpoint manual. Reusa o padrao do ManutencaoOutboxProcessorJob.
    /// </summary>
    [DisallowConcurrentExecution]
    public class PreventivaSchedulerJob : IJob
    {
        private readonly ContextManutencao _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PreventivaSchedulerJob(ContextManutencao context, IMediator mediator, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando PreventivaSchedulerJob...");

            var tenants = await _context.PlanosPreventivos
                .IgnoreQueryFilters()
                .Where(p => p.Status == EStatusRegistroManutencao.Ativo && p.DeletadoEm == null)
                .Select(p => p.TenantId)
                .Distinct()
                .ToListAsync();

            if (!tenants.Any())
            {
                Console.WriteLine("[Quartz] Nenhum tenant com plano preventivo ativo.");
                return;
            }

            foreach (var tenantId in tenants)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = tenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    var result = await _mediator.Send(new AvaliarVencimentoPreventivaCommand(null, DateTime.UtcNow, null));
                    Console.WriteLine($"[Quartz] Preventiva tenant {tenantId}: {result.Mensagem}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Quartz] Erro no vencimento da preventiva (tenant {tenantId}): {ex.Message}");
                }
            }

            Console.WriteLine("[Quartz] PreventivaSchedulerJob concluido.");
        }
    }
}
