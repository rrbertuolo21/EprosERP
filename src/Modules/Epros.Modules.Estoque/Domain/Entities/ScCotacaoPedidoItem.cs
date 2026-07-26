using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Relação entre item cotado e item do pedido de compra
    /// (EF Sourcing e Compras §4 `sc_cotacao_pedido_item`).
    /// </summary>
    public class ScCotacaoPedidoItem : EntidadeSaaSBase
    {
        public Guid CotacaoItemId { get; private set; }
        public Guid PedidoCompraItemId { get; private set; }

        protected ScCotacaoPedidoItem() { } // EF Core

        public ScCotacaoPedidoItem(Guid cotacaoItemId, Guid pedidoCompraItemId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CotacaoItemId = cotacaoItemId;
            PedidoCompraItemId = pedidoCompraItemId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ScCotacaoPedidoItem>()
                .Requires()
                .AreNotEquals(CotacaoItemId, Guid.Empty, nameof(CotacaoItemId), "O item cotado é obrigatório [Origem: ScCotacaoPedidoItem]")
                .AreNotEquals(PedidoCompraItemId, Guid.Empty, nameof(PedidoCompraItemId), "O item do pedido é obrigatório [Origem: ScCotacaoPedidoItem]"));
        }
    }
}
