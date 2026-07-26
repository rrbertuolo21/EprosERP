using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.RH.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.RH.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class RHOutboxProcessorJob : IJob
    {
        private readonly ContextRH _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RHOutboxProcessorJob(
            ContextRH context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando RHOutboxProcessorJob...");

            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => m.EventType == "FolhaProcessada" && m.ProcessadoEm == null && m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox de RH para processar.");
                return;
            }

            foreach (var message in messages)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    if (message.EventType == "FolhaProcessada")
                    {
                        var payload = JsonSerializer.Deserialize<FolhaProcessadaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new FolhaProcessadaEventNotification(
                                FolhaPagamentoId: payload.FolhaPagamentoId,
                                ColaboradorId: payload.ColaboradorId,
                                NomeColaborador: payload.NomeColaborador,
                                CpfColaborador: payload.CpfColaborador,
                                MesCompetencia: payload.MesCompetencia,
                                AnoCompetencia: payload.AnoCompetencia,
                                SalarioLiquido: payload.SalarioLiquido,
                                TenantId: message.TenantId
                            );

                            await _mediator.Publish(notification);
                        }
                    }

                    message.MarcarProcessado();
                }
                catch (Exception ex)
                {
                    message.RegistrarFalha(ex.Message);
                    Console.WriteLine($"[Quartz] Erro ao processar mensagem {message.Id} de RH: {ex.Message}");
                }

                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] RHOutboxProcessorJob concluído.");
        }

        private class FolhaProcessadaPayload
        {
            public Guid FolhaPagamentoId { get; set; }
            public Guid ColaboradorId { get; set; }
            public string NomeColaborador { get; set; } = string.Empty;
            public string CpfColaborador { get; set; } = string.Empty;
            public int MesCompetencia { get; set; }
            public int AnoCompetencia { get; set; }
            public decimal SalarioLiquido { get; set; }
        }
    }
}
