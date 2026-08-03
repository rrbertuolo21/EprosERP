using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Outbox;
using Epros.Shared.Domain.Events;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Epros.Modules.Vendas.Application.Outbox
{
    // ============================================================================
    // TRANSVERSAL T1 — CONSUMIDORES REAIS do Outbox de VENDAS (schema "vendas"),
    // roteados pelo DISPATCHER CENTRAL que substituiu o VendasOutboxProcessorJob
    // (que drenava só VendaFaturada/VendaCancelada e deixava PedidoEcommerceParaVenda,
    // DemandaPlanejadaPublicada e ven.ExpedicaoConfirmada morrerem na fila).
    //
    // Efeito PRESERVADO 1:1 do job legado: publicam a mesma notificação MediatR (fan-out
    // Financeiro/Fiscal + baixa de estoque). O guard de idempotência é o ProcessadoEm da
    // própria mensagem. TENANT EXPLÍCITO: o dispatcher central NÃO manipula HttpContext, então
    // os consumidores isolam o tenant da mensagem no IHttpContextAccessor ANTES de publicar
    // (idêntico ao job legado), para que os contextos resolvidos pelos handlers apliquem o
    // tenant correto.
    //
    // ⚠️ ANTI-DUPLA-CONTAGEM: NÃO há consumidor de baixa de estoque para ExpedicaoConfirmada —
    // VendaFaturada já baixa o estoque; expedição roteia ao fallback (pendência de regra) para
    // não recontar a saída.
    // ============================================================================

    internal static class VendasOutboxJson
    {
        public static readonly JsonSerializerOptions Opts = new() { PropertyNameCaseInsensitive = true };
    }

    /// <summary>
    /// Consome <c>VendaFaturada</c>: publica <see cref="VendaFaturadaEventNotification"/> (Financeiro + Fiscal
    /// + baixa de estoque), preservando o efeito do job legado.
    /// </summary>
    public class VendaFaturadaConsumer : IOutboxConsumer
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VendaFaturadaConsumer(IMediator mediator, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public string EventType => CatalogoEventosIntegracao.Vendas.VendaFaturada;

        public async Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            IsolarTenant(_httpContextAccessor, message.TenantId);

            var payload = JsonSerializer.Deserialize<VendaFaturadaPayload>(message.Payload, VendasOutboxJson.Opts);
            if (payload == null) return;

            var notification = new VendaFaturadaEventNotification(
                VendaId: payload.VendaId,
                TenantId: message.TenantId,
                Total: payload.Total,
                CriadoEm: payload.CriadoEm,
                Itens: (payload.Itens ?? new List<VendaFaturadaItemPayload>())
                    .Select(i => new VendaFaturadaItemNotification(i.ProdutoId, i.Quantidade, i.PrecoUnitario)).ToList(),
                UserId: "system_outbox");

            await _mediator.Publish(notification, cancellationToken);
        }

        internal static void IsolarTenant(IHttpContextAccessor accessor, string tenantId)
        {
            if (accessor == null) return;
            var httpContext = new DefaultHttpContext();
            httpContext.Items["TenantId"] = tenantId;
            accessor.HttpContext = httpContext;
        }

        private sealed class VendaFaturadaPayload
        {
            public Guid VendaId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public DateTime CriadoEm { get; set; }
            public List<VendaFaturadaItemPayload> Itens { get; set; } = new();
        }

        private sealed class VendaFaturadaItemPayload
        {
            public Guid ProdutoId { get; set; }
            public decimal Quantidade { get; set; }
            public decimal PrecoUnitario { get; set; }
        }
    }

    /// <summary>
    /// Consome <c>VendaCancelada</c>: publica <see cref="VendaCanceladaEventNotification"/> (estorno),
    /// preservando o efeito do job legado.
    /// </summary>
    public class VendaCanceladaConsumer : IOutboxConsumer
    {
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public VendaCanceladaConsumer(IMediator mediator, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _httpContextAccessor = httpContextAccessor;
        }

        public string EventType => CatalogoEventosIntegracao.Vendas.VendaCancelada;

        public async Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            VendaFaturadaConsumer.IsolarTenant(_httpContextAccessor, message.TenantId);

            var payload = JsonSerializer.Deserialize<VendaCanceladaPayload>(message.Payload, VendasOutboxJson.Opts);
            if (payload == null) return;

            var notification = new VendaCanceladaEventNotification(
                VendaId: payload.VendaId,
                TenantId: message.TenantId,
                Total: payload.Total,
                CanceladoEm: payload.CanceladoEm,
                Itens: (payload.Itens ?? new List<VendaCanceladaItemPayload>())
                    .Select(i => new VendaCanceladaItemNotification(i.ProdutoId, i.Quantidade)).ToList(),
                UserId: "system_outbox");

            await _mediator.Publish(notification, cancellationToken);
        }

        private sealed class VendaCanceladaPayload
        {
            public Guid VendaId { get; set; }
            public string TenantId { get; set; } = string.Empty;
            public decimal Total { get; set; }
            public DateTime CanceladoEm { get; set; }
            public List<VendaCanceladaItemPayload> Itens { get; set; } = new();
        }

        private sealed class VendaCanceladaItemPayload
        {
            public Guid ProdutoId { get; set; }
            public decimal Quantidade { get; set; }
        }
    }
}
