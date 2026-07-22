using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Movimento manual de estoque (entrada/saída lançada manualmente). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Estoque.EstoqueMovimentoManual.
    /// </summary>
    public class EstoqueMovimentoManual : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public ETipoEstoque TipoEstoque { get; private set; }
        public ETipoMovimento TipoMovimento { get; private set; }
        public decimal QuantidadeMovimentada { get; private set; }
        public decimal ValorUnitario { get; private set; }

        // Navegação intra-módulo
        public Produto? Produto { get; private set; }
        public FatoGeradorEstoque? FatoGeradorEstoque { get; private set; }

        protected EstoqueMovimentoManual() { } // EF Core

        public EstoqueMovimentoManual(Guid produtoId, ETipoEstoque tipoEstoque, ETipoMovimento tipoMovimento, decimal quantidadeMovimentada, decimal valorUnitario, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            TipoEstoque = tipoEstoque;
            TipoMovimento = tipoMovimento;
            QuantidadeMovimentada = quantidadeMovimentada;
            ValorUnitario = valorUnitario;
        }

        public void Validar() { }

        public void Alterar(Guid produtoId, ETipoEstoque tipoEstoque, ETipoMovimento tipoMovimento, decimal quantidadeMovimentada, decimal valorUnitario, string usuario)
        {
            ProdutoId = produtoId;
            TipoEstoque = tipoEstoque;
            TipoMovimento = tipoMovimento;
            QuantidadeMovimentada = quantidadeMovimentada;
            ValorUnitario = valorUnitario;
            MarcarAlterado(usuario);
        }
    }
}
