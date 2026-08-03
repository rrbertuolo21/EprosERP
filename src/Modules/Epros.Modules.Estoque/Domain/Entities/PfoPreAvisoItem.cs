using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>Item do pré-aviso de embarque (EF Portal do Fornecedor §15.7 `pfo_pre_aviso_item`).</summary>
    public class PfoPreAvisoItem : EntidadeSaaSBase
    {
        public Guid PreAvisoId { get; private set; }
        public Guid PedidoCompraItemId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public decimal? QuantidadePrevista { get; private set; }
        public string? Lote { get; private set; }

        public PfoPreAvisoEmbarque? PreAviso { get; private set; }

        protected PfoPreAvisoItem() { }

        public PfoPreAvisoItem(Guid preAvisoId, Guid pedidoCompraItemId, Guid? produtoId, decimal? quantidadePrevista, string? lote, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PreAvisoId = preAvisoId;
            PedidoCompraItemId = pedidoCompraItemId;
            ProdutoId = produtoId;
            QuantidadePrevista = quantidadePrevista;
            Lote = lote;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PfoPreAvisoItem>()
                .Requires()
                .IsNotEmpty(PedidoCompraItemId, nameof(PedidoCompraItemId), "O item do pedido é obrigatório no pré-aviso [Origem: PfoPreAvisoItem]"));
        }
    }
}
