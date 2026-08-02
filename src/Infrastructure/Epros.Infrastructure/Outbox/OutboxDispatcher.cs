using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Shared.Application.Outbox;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Epros.Infrastructure.Outbox
{
    /// <summary>
    /// TRANSVERSAL T1 — DISPATCHER CENTRAL DE OUTBOX.
    ///
    /// Fecha a causa-raiz "Outbox publica mas não despacha/consome": lê as mensagens não-processadas
    /// de um <see cref="DbContext"/> (a tabela <c>outbox_messages</c> do schema daquele contexto),
    /// desserializa/roteia por <see cref="OutboxMessage.EventType"/> do catálogo
    /// (<see cref="CatalogoEventosIntegracao"/>) para os <see cref="IOutboxConsumer"/> registrados,
    /// aplica o efeito in-process e marca a mensagem como processada.
    ///
    /// Propriedades: idempotente (processa apenas <c>ProcessadoEm == null</c>), com retry limitado
    /// (<c>Tentativas &lt; <see cref="MaxTentativas"/></c>) e registro de erro por mensagem. Eventos
    /// conhecidos do catálogo sem consumidor de efeito caem no <see cref="IOutboxFallbackConsumer"/>
    /// (pendência de regra — nunca inventa efeito). Eventos fora do catálogo são deixados intactos
    /// (podem pertencer a um job legado específico).
    ///
    /// Reutilizável: um <see cref="OutboxDispatcherJob{TContext}"/> por schema/contexto que tenha
    /// eventos a rotear. NÃO substitui os jobs legados que já funcionam (Vendas/Financeiro/etc.);
    /// é registrado apenas para schemas sem processador (ex.: Imobiliaria, Concessionárias/DMS).
    /// </summary>
    public class OutboxDispatcher
    {
        public const int MaxTentativas = 5;

        private readonly IReadOnlyDictionary<string, List<IOutboxConsumer>> _consumersByType;
        private readonly IOutboxFallbackConsumer? _fallback;
        private readonly ILogger<OutboxDispatcher> _logger;

        public OutboxDispatcher(
            IEnumerable<IOutboxConsumer> consumers,
            ILogger<OutboxDispatcher> logger,
            IOutboxFallbackConsumer? fallback = null)
        {
            _consumersByType = (consumers ?? Enumerable.Empty<IOutboxConsumer>())
                .GroupBy(c => c.EventType, StringComparer.Ordinal)
                .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);
            _logger = logger;
            _fallback = fallback;
        }

        /// <summary>
        /// Processa as mensagens pendentes do <paramref name="outboxContext"/>. Retorna quantas foram
        /// marcadas como processadas (roteadas ou tratadas pelo fallback).
        /// </summary>
        public async Task<int> ProcessAsync(DbContext outboxContext, CancellationToken ct = default)
        {
            var set = outboxContext.Set<OutboxMessage>();

            var pendentes = await set
                .IgnoreQueryFilters()
                .Where(m => m.ProcessadoEm == null && m.Tentativas < MaxTentativas)
                .OrderBy(m => m.CriadoEm)
                .ToListAsync(ct);

            if (pendentes.Count == 0)
                return 0;

            var processadas = 0;

            foreach (var message in pendentes)
            {
                var tocou = false;

                try
                {
                    if (_consumersByType.TryGetValue(message.EventType, out var consumers))
                    {
                        foreach (var consumer in consumers)
                            await consumer.ConsumeAsync(message, ct);

                        message.MarcarProcessado();
                        processadas++;
                        tocou = true;
                    }
                    else if (_fallback != null && CatalogoEventosIntegracao.EhEventoConhecido(message.EventType))
                    {
                        await _fallback.HandleUnroutedAsync(message, ct);
                        message.MarcarProcessado();
                        processadas++;
                        tocou = true;
                    }
                    else
                    {
                        // Evento sem consumidor e fora do catálogo: pode pertencer a um job legado
                        // específico daquele schema. Deixa intacto (não incrementa tentativa).
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    message.RegistrarFalha(ex.Message);
                    tocou = true;
                    _logger.LogError(ex,
                        "[OutboxDispatcher] Falha ao processar mensagem {MessageId} ({EventType}, tenant {TenantId}): {Erro}",
                        message.Id, message.EventType, message.TenantId, ex.Message);
                }

                if (tocou)
                {
                    set.Update(message);
                    await outboxContext.SaveChangesAsync(ct);
                }
            }

            return processadas;
        }
    }
}
