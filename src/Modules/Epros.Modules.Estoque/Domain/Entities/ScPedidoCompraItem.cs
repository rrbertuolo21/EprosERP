using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item do pedido de compra (EF Sourcing e Compras §5.9 / §9.3 `sc_pedido_compra_item`).
    /// Valores de subtotal, desconto, frete, seguro, despesas, IPI, ICMS e total (fluxo §9.3 item 4).
    /// </summary>
    public class ScPedidoCompraItem : EntidadeSaaSBase
    {
        public Guid PedidoCompraId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorFrete { get; private set; }
        public decimal ValorSeguro { get; private set; }
        public decimal ValorOutrasDespesas { get; private set; }
        public decimal ValorIpi { get; private set; }
        public decimal ValorIcms { get; private set; }
        public decimal ValorTotal { get; private set; }

        // Navegação intra-módulo
        public ScPedidoCompra? PedidoCompra { get; private set; }
        public Produto? Produto { get; private set; }

        protected ScPedidoCompraItem() { } // EF Core

        public ScPedidoCompraItem(
            Guid pedidoCompraId, Guid produtoId, decimal quantidade, decimal valorUnitario,
            decimal valorDesconto, decimal valorFrete, decimal valorSeguro, decimal valorOutrasDespesas,
            decimal valorIpi, decimal valorIcms, decimal valorTotal, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PedidoCompraId = pedidoCompraId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            ValorDesconto = valorDesconto;
            ValorFrete = valorFrete;
            ValorSeguro = valorSeguro;
            ValorOutrasDespesas = valorOutrasDespesas;
            ValorIpi = valorIpi;
            ValorIcms = valorIcms;
            ValorTotal = valorTotal;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ScPedidoCompraItem>()
                .Requires()
                .AreNotEquals(ProdutoId, Guid.Empty, nameof(ProdutoId), "O produto do item do pedido é obrigatório [Origem: ScPedidoCompraItem]")
                .IsGreaterThan(Quantidade, 0, nameof(Quantidade), "A quantidade do item do pedido deve ser maior que zero [Origem: ScPedidoCompraItem]"));
        }
    }
}
