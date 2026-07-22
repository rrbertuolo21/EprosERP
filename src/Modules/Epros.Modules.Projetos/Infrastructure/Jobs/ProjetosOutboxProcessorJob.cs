using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Epros.Modules.Projetos.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Epros.Modules.Projetos.Infrastructure.Jobs
{
    [DisallowConcurrentExecution]
    public class ProjetosOutboxProcessorJob : IJob
    {
        private readonly ContextProjetos _context;
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjetosOutboxProcessorJob(
            ContextProjetos context,
            IMediator mediator,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine("[Quartz] Iniciando ProjetosOutboxProcessorJob...");

            var messages = await _context.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => m.EventType == "ProjetoFaturado" && m.ProcessadoEm == null && m.Tentativas < 5)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync();

            if (!messages.Any())
            {
                Console.WriteLine("[Quartz] Nenhuma mensagem do outbox de Projetos para processar.");
                return;
            }

            foreach (var message in messages)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;

                try
                {
                    if (message.EventType == "ProjetoFaturado")
                    {
                        var payload = JsonSerializer.Deserialize<ProjetoFaturadoPayload>(message.Payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                        if (payload != null)
                        {
                            var notification = new ProjetoFaturadoEventNotification(
                                ProjetoId: payload.ProjetoId,
                                NomeProjeto: payload.NomeProjeto,
                                ClienteId: payload.ClienteId,
                                Milestone: payload.Milestone,
                                ValorFaturamento: payload.ValorFaturamento,
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
                    Console.WriteLine($"[Quartz] Erro ao processar mensagem {message.Id} de Projetos: {ex.Message}");
                }

                _context.OutboxMessages.Update(message);
                await _context.SaveChangesAsync();
            }

            Console.WriteLine("[Quartz] ProjetosOutboxProcessorJob concluído.");
        }

        private class ProjetoFaturadoPayload
        {
            public Guid ProjetoId { get; set; }
            public string NomeProjeto { get; set; } = string.Empty;
            public Guid ClienteId { get; set; }
            public decimal Milestone { get; set; }
            public decimal ValorFaturamento { get; set; }
        }
    }
}
