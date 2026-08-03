using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class UsoCupom : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public Guid CupomId { get; private set; }
        // 1.08E — o uso pode ser de um PEDIDO inicial OU de uma FATURA de ciclo (cupom recorrente).
        // Exatamente um dos dois é preenchido.
        public Guid? PedidoId { get; private set; }
        public Guid? FaturaId { get; private set; }

        protected UsoCupom() { } // EF Core

        /// <summary>Uso do cupom no PEDIDO inicial (fluxo de contratação).</summary>
        public UsoCupom(
            Guid clienteId,
            Guid cupomId,
            Guid pedidoId,
            string tenantId,
            string criadoPor)
            : this(clienteId, cupomId, pedidoId, null, tenantId, criadoPor)
        {
        }

        /// <summary>1.08E — Uso do cupom RECORRENTE na fatura de um ciclo de renovação (sem pedido).</summary>
        public static UsoCupom ParaFatura(
            Guid clienteId,
            Guid cupomId,
            Guid faturaId,
            string tenantId,
            string criadoPor)
            => new UsoCupom(clienteId, cupomId, null, faturaId, tenantId, criadoPor);

        private UsoCupom(
            Guid clienteId,
            Guid cupomId,
            Guid? pedidoId,
            Guid? faturaId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<UsoCupom>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .AreNotEquals(cupomId, Guid.Empty, nameof(CupomId), "CupomId é obrigatório")
            );

            var temPedido = pedidoId.HasValue && pedidoId.Value != Guid.Empty;
            var temFatura = faturaId.HasValue && faturaId.Value != Guid.Empty;
            if (!temPedido && !temFatura)
            {
                AddNotification(nameof(PedidoId), "O uso do cupom exige um pedido ou uma fatura de referência.");
            }

            ClienteId = clienteId;
            CupomId = cupomId;
            PedidoId = temPedido ? pedidoId : null;
            FaturaId = temFatura ? faturaId : null;
        }
    }
}
