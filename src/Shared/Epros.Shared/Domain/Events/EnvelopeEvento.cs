using System;

namespace Epros.Shared.Domain.Events
{
    /// <summary>
    /// TRANSVERSAL T2 — envelope padrão de evento de integração (TC-01).
    /// Metadados de rastreabilidade que acompanham todo evento publicado no Outbox:
    /// idempotência (<see cref="MessageId"/>), correlação/causação de fluxo, tenant e ator.
    /// O <see cref="EventType"/> deve pertencer ao <see cref="CatalogoEventosIntegracao"/>.
    /// </summary>
    public sealed record EnvelopeEvento(
        Guid MessageId,
        string EventType,
        string TenantId,
        string? CorrelationId,
        string? CausationId,
        string? ActorId,
        DateTime OcorridoEm,
        string SchemaVersao)
    {
        /// <summary>
        /// Cria um envelope novo com <see cref="MessageId"/> e timestamp gerados, versão do catálogo
        /// corrente. Quando <paramref name="correlationId"/> é nulo, o próprio MessageId inicia a cadeia.
        /// </summary>
        public static EnvelopeEvento Novo(
            string eventType,
            string tenantId,
            string? actorId = null,
            string? correlationId = null,
            string? causationId = null)
        {
            var messageId = Guid.NewGuid();
            return new EnvelopeEvento(
                MessageId: messageId,
                EventType: eventType,
                TenantId: tenantId,
                CorrelationId: correlationId ?? messageId.ToString(),
                CausationId: causationId,
                ActorId: actorId,
                OcorridoEm: DateTime.UtcNow,
                SchemaVersao: CatalogoEventosIntegracao.Versao);
        }
    }
}
