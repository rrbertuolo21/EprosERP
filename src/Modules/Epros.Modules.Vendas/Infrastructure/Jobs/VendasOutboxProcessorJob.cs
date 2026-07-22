using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Vendas.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class VendasOutboxProcessorJob : IJob
    {
        private readonly ContextVendas _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VendasOutboxProcessorJob(
            ContextVendas context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando VendasOutboxProcessorJob...");

            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => (m.EventType == "VendaFaturada" || m.EventType == "VendaCancelada") && m.ProcessadoEm == null && m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox de Vendas para processar.");
                return;
            }

            foreach (var message in messages)
            {
                // Configura o HttpContext com o tenant da mensagem para garantir isolamento
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    if (message.EventType == "VendaFaturada")
                    {
                        var payload = JsonSerializer.Deserialize<VendaFaturadaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new VendaFaturadaEventNotification(
                                VendaId: payload.VendaId,
                                TenantId: message.TenantId,
                                Total: payload.Total,
                                CriadoEm: payload.CriadoEm,
                                Itens: payload.Itens.Select(i => new VendaFaturadaItemNotification(i.ProdutoId, i.Quantidade, i.PrecoUnitario)).ToList(),
                                UserId: "system_outbox"
                            );

                            await _mediator.Publish(notification);
                        }
                    }
                    else if (message.EventType == "VendaCancelada")
                    {
                        var payload = JsonSerializer.Deserialize<VendaCanceladaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new VendaCanceladaEventNotification(
                                VendaId: payload.VendaId,
                                TenantId: message.TenantId,
                                Total: payload.Total,
                                CanceladoEm: payload.CanceladoEm,
                                Itens: payload.Itens.Select(i => new VendaCanceladaItemNotification(i.ProdutoId, i.Quantidade)).ToList(),
                                UserId: "system_outbox"
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

            Console.WriteLine("[Quartz] VendasOutboxProcessorJob concluído.");
        }

        private class VendaFaturadaPayload
        {
            public Guid VendaId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public DateTime CriadoEm { get; set; }
            public System.Collections.Generic.List<VendaFaturadaItemPayload> Itens { get; set; } = new();
        }

        private class VendaFaturadaItemPayload
        {
            public Guid ProdutoId { get; set; }
            public decimal Quantidade { get; set; }
            public decimal PrecoUnitario { get; set; }
        }

        private class VendaCanceladaPayload
        {
            public Guid VendaId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public DateTime CanceladoEm { get; set; }
            public System.Collections.Generic.List<VendaCanceladaItemPayload> Itens { get; set; } = new();
        }

        private class VendaCanceladaItemPayload
        {
            public Guid ProdutoId { get; set; }
            public decimal Quantidade { get; set; }
        }
    }
}
