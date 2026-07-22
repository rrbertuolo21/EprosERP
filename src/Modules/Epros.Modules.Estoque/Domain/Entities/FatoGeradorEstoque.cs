using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Fato gerador de uma movimentação de estoque (venda, compra ou movimento manual).
    /// Porte fiel do legado Epros.ERP.Domain.Entities.Estoque.FatoGeradorEstoque.
    /// VendaId referencia o módulo de Vendas por FK Guid (sem navegação cruzada); CompraId e
    /// EstoqueMovimentoManualId são intra-módulo.
    /// </summary>
    public class FatoGeradorEstoque : EntidadeSaaSBase
    {
        public Guid? VendaId { get; private set; }
        public Guid? CompraId { get; private set; }
        public Guid? EstoqueMovimentoManualId { get; private set; }
        public EOrigem Origem { get; private set; }

        // Navegação intra-módulo
        public ICollection<ProdutoFichaEstoqueEntrada> ProdutoFichaEstoqueEntradas { get; private set; } = new List<ProdutoFichaEstoqueEntrada>();
        public ICollection<ProdutoFichaEstoqueSaida> ProdutoFichaEstoqueSaidas { get; private set; } = new List<ProdutoFichaEstoqueSaida>();
        public Compra? Compra { get; private set; }
        public EstoqueMovimentoManual? EstoqueMovimentoManual { get; private set; }

        protected FatoGeradorEstoque() { } // EF Core

        public FatoGeradorEstoque(Guid? vendaId, Guid? compraId, Guid? estoqueMovimentoManualId, EOrigem origem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            CompraId = compraId;
            EstoqueMovimentoManualId = estoqueMovimentoManualId;
            Origem = origem;
        }

        public void Validar() { }
    }
}
