using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class UpgradePlano : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public bool IsActive { get; private set; }
        public string OrderNo { get; private set; } = string.Empty;

        protected UpgradePlano() { } // EF Core

        public UpgradePlano(Guid planoId, bool isActive, string orderNo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<UpgradePlano>()
                .Requires()
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "PlanoId inválido.")
                .IsNotNullOrEmpty(orderNo, nameof(OrderNo), "Número do pedido (OrderNo) é obrigatório.")
                .HasMaxLen(orderNo, 100, nameof(OrderNo), "Número do pedido deve ter no máximo 100 caracteres.")
            );

            PlanoId = planoId;
            IsActive = isActive;
            OrderNo = orderNo;
        }

        public void Inativar(string alteradoPor)
        {
            IsActive = false;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            IsActive = true;
            MarcarAlterado(alteradoPor);
        }
    }
}
