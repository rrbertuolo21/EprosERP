using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Histórico de reajuste de valor (venda/compra) de um produto. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Cadastros.Produtos.ProdutoHistoricoReajuste.
    /// </summary>
    public class ProdutoHistoricoReajuste : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public string? CodigoProduto { get; private set; }
        public decimal ValorAntigo { get; private set; }
        public ETipoReajuste Tipo { get; private set; }
        public decimal Fator { get; private set; }
        public decimal ValorFixo { get; private set; }
        public decimal ValorNovo { get; private set; }
        public string? Motivo { get; private set; }

        // Navegação intra-módulo
        public Produto? Produto { get; private set; }

        protected ProdutoHistoricoReajuste() { } // EF Core

        public ProdutoHistoricoReajuste(Guid produtoId, string? codigoProduto, decimal valorAntigo, ETipoReajuste tipo, decimal fator, decimal valorFixo, decimal valorNovo, string? motivo, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            CodigoProduto = codigoProduto;
            ValorAntigo = valorAntigo;
            Tipo = tipo;
            Fator = fator;
            ValorFixo = valorFixo;
            ValorNovo = valorNovo;
            Motivo = motivo;
        }
    }
}
