using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Qualidade.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class QualidadeOutboxProcessorJob : IJob
    {
        private readonly ContextQualidade _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QualidadeOutboxProcessorJob(
            ContextQualidade context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando QualidadeOutboxProcessorJob...");

            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => m.EventType == "InspecaoReprovada" && m.ProcessadoEm == null && m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox de Qualidade para processar.");
                return;
            }

            foreach (var message in messages)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    if (message.EventType == "InspecaoReprovada")
                    {
                        var payload = JsonSerializer.Deserialize<InspecaoReprovadaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new InspecaoReprovadaEventNotification(
                                InspecaoLoteId: payload.InspecaoLoteId,
                                TenantId: message.TenantId,
                                Sku: payload.Sku,
                                NomeProduto: payload.NomeProduto,
                                QuantidadeLote: payload.QuantidadeLote,
                                DataInspecao: payload.DataInspecao,
                                Responsavel: payload.Responsavel
                            );

                            await _mediator.Publish(notification);
                        }
                    }

                    message.MarcarProcessado();
                }
                catch (Exception ex)
                {
                    message.RegistrarFalha(ex.Message);
                    Console.WriteLine($"[Quartz] Erro ao processar mensagem {message.Id}: {ex.Message}");
                }

                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] QualidadeOutboxProcessorJob concluído.");
        }

        private class InspecaoReprovadaPayload
        {
            public Guid InspecaoLoteId { get; set; }
            public string Sku { get; set; } = string.Empty;
            public string NomeProduto { get; set; } = string.Empty;
            public decimal QuantidadeLote { get; set; }
            public DateTime? DataInspecao { get; set; }
            public string Responsavel { get; set; } = string.Empty;
        }
    }
}
