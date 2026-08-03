using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Aplicativo.Infrastructure.Jobs
{
    /// <summary>
    /// Verifica agendas de workflow ativas e enfileira jobs vencidos, por tenant, evitando duplicidade
    /// (WF-CA-011). Espelha o padrão de isolamento por tenant do <see cref="AplicativoOutboxProcessorJob"/>.
    /// [Origem: EF WORKFLOW §7.4 / §8.3]
    /// Registro no Program.cs (Quartz) é passe serializado — ver pendências do submódulo.
    /// </summary>
    [DisallowConcurrentExecution]
    public class WorkflowAgendamentoJob : IJob
    {
        private readonly ContextAplicativo _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WorkflowAgendamentoJob(
            ContextAplicativo context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var agora = DateTime.UtcNow;

            System.Collections.Generic.List<string> tenants;
            try
            {
                tenants = await _context.WfAgendamentos
                    .IgnoreQueryFilters()
                    .Where(a => a.DeletadoEm == null && a.Ativo && a.ProximaExecucaoEm != null && a.ProximaExecucaoEm <= agora)
                    .Select(a => a.TenantId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (System.Data.Common.DbException ex) when (ex.SqlState == "42P01")
            {
                // Tabelas do subsistema de Workflow (wf_*) ainda não provisionadas no banco
                // (drift: entidades no modelo sem CreateTable — corrigir com migration própria).
                // Sem elas não há agenda a processar: pula esta execução em vez de estourar em loop.
                return;
            }

            foreach (var tenantId in tenants)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = tenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    await _mediator.Send(new EnfileirarJobsAgendadosCommand(agora));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Quartz] WorkflowAgendamentoJob erro no tenant {tenantId}: {ex.Message}");
                }
            }
        }
    }
}
