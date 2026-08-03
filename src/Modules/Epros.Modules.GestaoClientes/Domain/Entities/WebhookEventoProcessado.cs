using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// 1.06 — Registro de idempotência de webhook de pagamento (MercadoPago/PIX). Guarda o id de
    /// evento/pagamento já processado para que uma reentrega do mesmo webhook seja ignorada (dedupe),
    /// evitando baixa/processamento duplicado. Entidade de plataforma (IGlobalEntity): o webhook chega
    /// sem contexto de tenant HTTP e é processado sob o tenant "system".
    /// </summary>
    public class WebhookEventoProcessado : EntidadeSaaSBase, IGlobalEntity
    {
        /// <summary>Provedor do webhook (ex.: "mercadopago").</summary>
        public string Provedor { get; private set; } = string.Empty;

        /// <summary>Id do evento/pagamento no provedor — chave natural de deduplicação.</summary>
        public string EventoId { get; private set; } = string.Empty;

        /// <summary>Ação do webhook (ex.: payment.created / payment.updated).</summary>
        public string? Acao { get; private set; }

        public DateTime ProcessadoEm { get; private set; }

        protected WebhookEventoProcessado() { } // EF Core

        public WebhookEventoProcessado(string provedor, string eventoId, string? acao, string criadoPor)
            : base("system", criadoPor)
        {
            AddNotifications(new Contract<WebhookEventoProcessado>()
                .Requires()
                .IsNotNullOrEmpty(provedor, nameof(Provedor), "Provedor do webhook é obrigatório")
                .IsNotNullOrEmpty(eventoId, nameof(EventoId), "Id do evento do webhook é obrigatório")
            );

            Provedor = provedor;
            EventoId = eventoId;
            Acao = acao;
            ProcessadoEm = DateTime.UtcNow;
        }
    }
}
