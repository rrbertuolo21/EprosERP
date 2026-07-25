using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item de ajuste de estoque (EF Movimentação Manual e Ajustes §15.11).
    /// Quantidade positiva é tratada como entrada; negativa como saída controlada (EF §7.8).
    /// FichaEntradaId referencia a ficha impactada quando aplicável.
    /// </summary>
    public class AjusteEstoqueItem : EntidadeSaaSBase
    {
        public Guid AjusteEstoqueId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public string? Lote { get; private set; }
        public Guid? FichaEntradaId { get; private set; }

        // Navegação intra-módulo
        public AjusteEstoque? Ajuste { get; private set; }
        public Produto? Produto { get; private set; }

        protected AjusteEstoqueItem() { } // EF Core

        public AjusteEstoqueItem(Guid ajusteEstoqueId, Guid produtoId, decimal quantidade, decimal valorUnitario, string? lote, Guid? fichaEntradaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AjusteEstoqueId = ajusteEstoqueId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            Lote = lote;
            FichaEntradaId = fichaEntradaId;
        }

        public void Validar() { }
    }
}
