using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Outbox;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Epros.Modules.Projetos.Application.Outbox
{
    /// <summary>
    /// TRANSVERSAL T1 — CONSUMIDOR REAL do Outbox de PROJETOS (schema "projetos"), roteado pelo
    /// DISPATCHER CENTRAL (<see cref="Epros.Infrastructure.Outbox.OutboxDispatcher"/>) que substituiu o
    /// job por-módulo <c>ProjetosOutboxProcessorJob</c> (que só drenava "ProjetoFaturado" e deixava
    /// <c>prj.orcamento.baseline_congelada</c> morrer na fila).
    ///
    /// Efeito PRESERVADO 1:1 do job legado: publica <see cref="ProjetoFaturadoEventNotification"/> via
    /// MediatR — o <c>ProjetoFaturadoFinanceiroHandler</c> gera o título em Contas a Receber (modelo FIEL)
    /// + lançamento contábil (TEC-8). Comportamento idêntico ao anterior; o guard de idempotência é o
    /// <c>ProcessadoEm</c> da própria mensagem (não reprocessa retry).
    ///
    /// TENANT EXPLÍCITO: o dispatcher central NÃO manipula HttpContext, então este consumidor isola o
    /// tenant da mensagem no <see cref="IHttpContextAccessor"/> ANTES de publicar (idêntico ao job legado),
    /// para que o <c>ContextFinanceiro</c> resolvido pelos handlers aplique o tenant correto.
    /// </summary>
    public class ProjetoFaturadoConsumer : IOutboxConsumer
    {
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjetoFaturadoConsumer(IMediator mediator, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        // Mantém a string legada publicada ("ProjetoFaturado" == CatalogoEventosIntegracao.Vendas.ProjetoFaturado).
        public string EventType => CatalogoEventosIntegracao.Vendas.ProjetoFaturado;

        public async Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            // Isola o tenant da mensagem (o dispatcher não seta HttpContext).
            if (_httpContextAccessor != null)
            {
                var httpContext = new DefaultHttpContext();
                httpContext.Items["TenantId"] = message.TenantId;
                _httpContextAccessor.HttpContext = httpContext;
            }

            var payload = JsonSerializer.Deserialize<ProjetoFaturadoPayload>(message.Payload, JsonOpts);
            if (payload == null) return;

            var notification = new ProjetoFaturadoEventNotification(
                ProjetoId: payload.ProjetoId,
                NomeProjeto: payload.NomeProjeto,
                ClienteId: payload.ClienteId,
                Milestone: payload.Milestone,
                ValorFaturamento: payload.ValorFaturamento,
                TenantId: message.TenantId);

            await _mediator.Publish(notification, cancellationToken);
        }

        private sealed class ProjetoFaturadoPayload
        {
            public Guid ProjetoId { get; set; }
            public string NomeProjeto { get; set; } = string.Empty;
            public Guid ClienteId { get; set; }
            public decimal Milestone { get; set; }
            public decimal ValorFaturamento { get; set; }
        }
    }
}
