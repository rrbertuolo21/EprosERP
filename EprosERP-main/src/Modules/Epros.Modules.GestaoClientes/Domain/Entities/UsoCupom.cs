using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class UsoCupom : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public Guid CupomId { get; private set; }
        public Guid PedidoId { get; private set; }

        protected UsoCupom() { } // EF Core

        public UsoCupom(
            Guid clienteId,
            Guid cupomId,
            Guid pedidoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<UsoCupom>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .AreNotEquals(cupomId, Guid.Empty, nameof(CupomId), "CupomId é obrigatório")
                .AreNotEquals(pedidoId, Guid.Empty, nameof(PedidoId), "PedidoId é obrigatório")
            );

            ClienteId = clienteId;
            CupomId = cupomId;
            PedidoId = pedidoId;
        }
    }
}
