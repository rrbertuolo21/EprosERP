using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.Producao.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Producao.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class ProducaoOutboxProcessorJob : IJob
    {
        private readonly ContextProducao _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProducaoOutboxProcessorJob(
            ContextProducao context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando ProducaoOutboxProcessorJob...");

            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => m.EventType == "OrdemProducaoEncerrada" && m.ProcessadoEm == null && m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox de Producao para processar.");
                return;
            }

            foreach (var message in messages)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    if (message.EventType == "OrdemProducaoEncerrada")
                    {
                        var payload = JsonSerializer.Deserialize<OrdemProducaoEncerradaPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var insumos = payload.InsumosConsumidos.Select(i => new InsumoConsumidoNotification(i.InsumoSku, i.QuantidadeConsumida)).ToList();

                            var notification = new OrdemProducaoEncerradaEventNotification(
                                OrdemProducaoId: payload.OrdemProducaoId,
                                Codigo: payload.Codigo,
                                ProdutoAcabadoSku: payload.ProdutoAcabadoSku,
                                QuantidadeProduzida: payload.QuantidadeProduzida,
                                QuantidadeRefugada: payload.QuantidadeRefugada,
                                TenantId: message.TenantId,
                                InsumosConsumidos: insumos
                            );

                            await _mediator.Publish(notification);
                        }
                    }

                    message.MarcarProcessado();
                }
                catch (Exception ex)
                {
                    message.MarcarProcessado(); // para evitar loop de falha no teste/execução, ou marque falha
                    message.RegistrarFalha(ex.Message);
                    Console.WriteLine($"[Quartz] Erro ao processar mensagem {message.Id} de Producao: {ex.Message}");
                }

                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] ProducaoOutboxProcessorJob concluído.");
        }

        private class OrdemProducaoEncerradaPayload
        {
            public Guid OrdemProducaoId { get; set; }
            public string Codigo { get; set; } = string.Empty;
            public string ProdutoAcabadoSku { get; set; } = string.Empty;
            public decimal QuantidadeProduzida { get; set; }
            public decimal QuantidadeRefugada { get; set; }
            public List<InsumoConsumidoItemPayload> InsumosConsumidos { get; set; } = new();
        }

        private class InsumoConsumidoItemPayload
        {
            public string InsumoSku { get; set; } = string.Empty;
            public decimal QuantidadeConsumida { get; set; }
        }
    }
}
