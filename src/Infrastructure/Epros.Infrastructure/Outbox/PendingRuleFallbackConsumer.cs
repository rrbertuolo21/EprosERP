using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Outbox;
using Epros.Shared.Domain.Events;
using Microsoft.Extensions.Logging;

namespace Epros.Infrastructure.Outbox
{
    /// <summary>
    /// Fallback padrão do <see cref="OutboxDispatcher"/>: evento CONHECIDO do catálogo, roteado, mas sem
    /// consumidor de efeito registrado. Registra a pendência (WARNING) e permite ao dispatcher marcar a
    /// mensagem como processada para não reprocessar em loop.
    ///
    /// ⚠️ NÃO inventa efeito de negócio/fiscal. Cada evento aqui deve ser anotado em DECISOES-PENDENTES
    /// até ganhar um <see cref="IOutboxConsumer"/> real.
    /// </summary>
    public class PendingRuleFallbackConsumer : IOutboxFallbackConsumer
    {
        private readonly ILogger<PendingRuleFallbackConsumer> _logger;

        public PendingRuleFallbackConsumer(ILogger<PendingRuleFallbackConsumer> logger) => _logger = logger;

        public Task HandleUnroutedAsync(OutboxMessage message, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning(
                "[OutboxDispatcher] Evento '{EventType}' (tenant {TenantId}, msg {MessageId}) roteado SEM consumidor de efeito: PENDENTE DE REGRA. Registrar em DECISOES-PENDENTES.",
                message.EventType, message.TenantId, message.Id);
            return Task.CompletedTask;
        }
    }
}
