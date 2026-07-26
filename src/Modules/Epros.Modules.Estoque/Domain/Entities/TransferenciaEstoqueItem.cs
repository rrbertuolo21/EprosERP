using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item transferido de uma transferência de estoque (EF Movimentação Manual e Ajustes §15.7).
    /// </summary>
    public class TransferenciaEstoqueItem : EntidadeSaaSBase
    {
        public Guid TransferenciaEstoqueId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal? ValorUnitario { get; private set; }
        public string? Lote { get; private set; }
        public DateTime? DataValidade { get; private set; }

        // Navegação intra-módulo
        public TransferenciaEstoque? Transferencia { get; private set; }
        public Produto? Produto { get; private set; }

        protected TransferenciaEstoqueItem() { } // EF Core

        public TransferenciaEstoqueItem(Guid transferenciaEstoqueId, Guid produtoId, decimal quantidade, decimal? valorUnitario, string? lote, DateTime? dataValidade, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TransferenciaEstoqueId = transferenciaEstoqueId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            Lote = lote;
            DataValidade = dataValidade;
        }

        public void Validar() { }
    }
}
