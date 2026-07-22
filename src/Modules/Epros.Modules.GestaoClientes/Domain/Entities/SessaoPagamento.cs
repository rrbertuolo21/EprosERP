using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class SessaoPagamento : EntidadeSaaSBase
    {
        public string GatewayRef { get; private set; } = string.Empty;
        public SessaoPagamentoStatus Status { get; private set; } = SessaoPagamentoStatus.Pending;
        public Guid AssinaturaId { get; private set; }
        public Guid PedidoId { get; private set; }

        protected SessaoPagamento() { } // EF Core

        public SessaoPagamento(
            string gatewayRef,
            Guid assinaturaId,
            Guid pedidoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<SessaoPagamento>()
                .Requires()
                .IsNotNullOrEmpty(gatewayRef, nameof(GatewayRef), "Referência do gateway é obrigatória")
                .AreNotEquals(assinaturaId, Guid.Empty, nameof(AssinaturaId), "AssinaturaId é obrigatória")
                .AreNotEquals(pedidoId, Guid.Empty, nameof(PedidoId), "PedidoId é obrigatório")
            );

            GatewayRef = gatewayRef;
            AssinaturaId = assinaturaId;
            PedidoId = pedidoId;
            Status = SessaoPagamentoStatus.Pending;
        }

        public void Completar(string alteradoPor)
        {
            Status = SessaoPagamentoStatus.Completed;
            MarcarAlterado(alteradoPor);
        }
    }
}
