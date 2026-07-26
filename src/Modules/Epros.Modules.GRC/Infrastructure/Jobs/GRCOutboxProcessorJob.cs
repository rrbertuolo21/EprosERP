using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.GRC.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.GRC.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class GRCOutboxProcessorJob : IJob
    {
        private readonly ContextGRC _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GRCOutboxProcessorJob(
            ContextGRC context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando GRCOutboxProcessorJob...");

            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => m.EventType == "DenunciaProcedente" && m.ProcessadoEm == null && m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox de GRC para processar.");
                return;
            }

            foreach (var message in messages)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    if (message.EventType == "DenunciaProcedente")
                    {
                        var payload = JsonSerializer.Deserialize<DenunciaProcedentePayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new DenunciaProcedenteEventNotification(
                                DenunciaId: payload.DenunciaId,
                                Relato: payload.Relato,
                                ParecerFinal: payload.ParecerFinal,
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
                    Console.WriteLine($"[Quartz] Erro ao processar mensagem {message.Id} de GRC: {ex.Message}");
                }

                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] GRCOutboxProcessorJob concluído.");
        }

        private class DenunciaProcedentePayload
        {
            public Guid DenunciaId { get; set; }
            public string Relato { get; set; } = string.Empty;
            public string ParecerFinal { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;
        }
    }
}
