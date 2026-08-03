using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item de uma devolução de compra (CD4 / EF DEVOLUCAO_DE_COMPRA §6.2 `com_devolucao_compra_item`).
    /// Referencia opcionalmente o item da compra de origem (`com_compra_item`) para viabilizar a checagem
    /// de quantidade (DEV-002). Valor total da linha = quantidade × valor unitário.
    /// </summary>
    public class DevolucaoCompraItem : EntidadeSaaSBase
    {
        public Guid DevolucaoId { get; private set; }
        public Guid? CompraItemOrigemId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal ValorTotal { get; private set; }

        public DevolucaoCompra? Devolucao { get; private set; }

        protected DevolucaoCompraItem() { } // EF Core

        public DevolucaoCompraItem(
            Guid devolucaoId,
            Guid? compraItemOrigemId,
            Guid produtoId,
            decimal quantidade,
            decimal valorUnitario,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            DevolucaoId = devolucaoId;
            CompraItemOrigemId = compraItemOrigemId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            ValorTotal = Math.Round(quantidade * valorUnitario, 2, MidpointRounding.AwayFromZero);
            Validar();
        }

        public void Validar()
        {
            Clear();
            if (ProdutoId == Guid.Empty)
                AddNotification("ProdutoId", "O produto do item é obrigatório [DEV-101] [Origem: DevolucaoCompraItem]");
            if (Quantidade <= 0)
                AddNotification("Quantidade", "A quantidade devolvida deve ser maior que zero [DEV-102] [Origem: DevolucaoCompraItem]");
            if (ValorUnitario < 0)
                AddNotification("ValorUnitario", "O valor unitário não pode ser negativo [DEV-103] [Origem: DevolucaoCompraItem]");
        }
    }
}
