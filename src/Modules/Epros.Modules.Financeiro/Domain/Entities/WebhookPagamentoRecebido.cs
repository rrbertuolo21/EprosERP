using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Registro de um webhook de pagamento recebido (EF FIN-SF — estrutura de baixa por webhook).
    /// É a trilha de idempotência/dedup: a chave natural (gateway x id de evento externo) impede
    /// processar o mesmo aviso duas vezes. Guarda o resultado (baixa da fatura por nosso número) e o
    /// motivo quando não processa (assinatura inválida, fatura não localizada, duplicado).
    /// </summary>
    public class WebhookPagamentoRecebido : EntidadeSaaSBase
    {
        public Guid GatewayPagamentoId { get; private set; }
        /// <summary>Id do evento no provedor — chave de dedup (idempotência) por gateway.</summary>
        public string EventoExternoId { get; private set; } = string.Empty;
        public string? TipoEvento { get; private set; }
        public long? NossoNumero { get; private set; }
        public decimal? Valor { get; private set; }
        public DateTime? DataPagamento { get; private set; }
        public EStatusWebhookPagamento Status { get; private set; }
        public Guid? FaturaCobrancaId { get; private set; }
        public string? Detalhe { get; private set; }
        public DateTime RecebidoEm { get; private set; }
        public DateTime? ProcessadoEm { get; private set; }

        protected WebhookPagamentoRecebido() { } // EF Core

        public WebhookPagamentoRecebido(Guid gatewayPagamentoId, string eventoExternoId, string? tipoEvento,
            long? nossoNumero, decimal? valor, DateTime? dataPagamento, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<WebhookPagamentoRecebido>()
                .Requires()
                .AreNotEquals(gatewayPagamentoId, Guid.Empty, nameof(GatewayPagamentoId), "O gateway do webhook é obrigatório.")
                .IsNotNullOrEmpty(eventoExternoId, nameof(EventoExternoId), "O id do evento externo é obrigatório (dedup)."));

            GatewayPagamentoId = gatewayPagamentoId;
            EventoExternoId = eventoExternoId;
            TipoEvento = tipoEvento;
            NossoNumero = nossoNumero;
            Valor = valor;
            DataPagamento = dataPagamento;
            Status = EStatusWebhookPagamento.Recebido;
            RecebidoEm = DateTime.UtcNow;
        }

        public void MarcarProcessado(Guid faturaId, string usuario)
        {
            Status = EStatusWebhookPagamento.Processado;
            FaturaCobrancaId = faturaId;
            ProcessadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void MarcarFalha(EStatusWebhookPagamento status, string? detalhe, string usuario)
        {
            Status = status;
            Detalhe = detalhe;
            ProcessadoEm = DateTime.UtcNow;
            MarcarAlterado(usuario);
        }
    }
}
