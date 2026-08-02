using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Domain.Events;

namespace Epros.Shared.Application.Outbox
{
    /// <summary>
    /// TRANSVERSAL T1 — Consumidor in-process de UM tipo de evento do Outbox (catálogo
    /// <see cref="CatalogoEventosIntegracao"/>). O <see cref="OutboxDispatcher"/> central roteia cada
    /// mensagem não-processada por <see cref="OutboxMessage.EventType"/> para os consumidores registrados
    /// e marca a mensagem como processada.
    ///
    /// Contrato de isolamento: o consumidor DEVE ser idempotente e explícito quanto ao tenant
    /// (usar <see cref="OutboxMessage.TenantId"/> nas consultas com IgnoreQueryFilters e ao criar entidades),
    /// pois o dispatcher não manipula HttpContext.
    /// </summary>
    public interface IOutboxConsumer
    {
        /// <summary>Tipo de evento do catálogo que este consumidor trata (ex.: "imo.aluguel.cobranca_gerada").</summary>
        string EventType { get; }

        /// <summary>Aplica o efeito de negócio da mensagem. Idempotente.</summary>
        Task ConsumeAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Fallback opcional acionado pelo <see cref="OutboxDispatcher"/> quando uma mensagem é de um
    /// evento CONHECIDO do catálogo mas não há <see cref="IOutboxConsumer"/> registrado para ele.
    /// A implementação padrão apenas registra a pendência (nunca inventa regra de negócio/fiscal).
    /// </summary>
    public interface IOutboxFallbackConsumer
    {
        Task HandleUnroutedAsync(OutboxMessage message, CancellationToken cancellationToken = default);
    }
}
