using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Manutencao.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class ManutencaoOutboxProcessorJob : IJob
    {
        private readonly ContextManutencao _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ManutencaoOutboxProcessorJob(
            ContextManutencao context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando ManutencaoOutboxProcessorJob...");

            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => m.EventType == "OrdemManutencaoConcluida" && m.ProcessadoEm == null && m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox de Manutencao para processar.");
                return;
            }

            foreach (var message in messages)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    if (message.EventType == "OrdemManutencaoConcluida")
                    {
                        var payload = JsonSerializer.Deserialize<OrdemManutencaoConcluidaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var pecas = payload.Pecas.Select(p => new OrdemManutencaoPecaDto(p.ProdutoId, p.Quantidade)).ToList();

                            var notification = new OrdemManutencaoConcluidaEventNotification(
                                OrdemManutencaoId: payload.OrdemManutencaoId,
                                EquipamentoId: payload.EquipamentoId,
                                Pecas: pecas,
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
                    Console.WriteLine($"[Quartz] Erro ao processar mensagem {message.Id} de Manutencao: {ex.Message}");
                }

                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] ManutencaoOutboxProcessorJob concluído.");
        }

        private class OrdemManutencaoConcluidaPayload
        {
            public Guid OrdemManutencaoId { get; set; }
            public Guid EquipamentoId { get; set; }
            public System.Collections.Generic.List<PecaPayload> Pecas { get; set; } = new();
            public string TenantId { get; set; } = string.Empty;
        }

        private class PecaPayload
        {
            public Guid ProdutoId { get; set; }
            public decimal Quantidade { get; set; }
        }
    }
}
