using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Fato gerador de uma movimentação de estoque (movimento manual, venda, compra, ajuste...).
    /// Porte fiel do legado Epros.ERP.Domain.Entities.Estoque.FatoGeradorEstoque, expandido conforme
    /// a EF Movimentação Manual e Ajustes §15.3 (origem normalizada e documentos relacionados).
    /// VendaId referencia o módulo de Vendas por FK Guid (sem navegação cruzada); CompraId e
    /// EstoqueMovimentoManualId são intra-módulo.
    /// </summary>
    public class FatoGeradorEstoque : EntidadeSaaSBase
    {
        public Guid? VendaId { get; private set; }
        public Guid? CompraId { get; private set; }
        public Guid? EstoqueMovimentoManualId { get; private set; }
        public EOrigemFatoGeradorEstoque Origem { get; private set; }

        // Documentos relacionados (EF §15.3) — FKs Guid sem navegação (integração/outros módulos).
        public Guid? DocumentoEntradaId { get; private set; }
        public Guid? DocumentoSaidaId { get; private set; }
        public Guid? DocumentoConsumidorEntradaId { get; private set; }
        public Guid? DocumentoConsumidorSaidaId { get; private set; }
        public string? ReferenciaExterna { get; private set; }

        // Navegação intra-módulo
        public ICollection<ProdutoFichaEstoqueEntrada> ProdutoFichaEstoqueEntradas { get; private set; } = new List<ProdutoFichaEstoqueEntrada>();
        public ICollection<ProdutoFichaEstoqueSaida> ProdutoFichaEstoqueSaidas { get; private set; } = new List<ProdutoFichaEstoqueSaida>();
        public Compra? Compra { get; private set; }
        public EstoqueMovimentoManual? EstoqueMovimentoManual { get; private set; }

        protected FatoGeradorEstoque() { } // EF Core

        public FatoGeradorEstoque(Guid? vendaId, Guid? compraId, Guid? estoqueMovimentoManualId, EOrigemFatoGeradorEstoque origem, string tenantId, string criadoPor,
            Guid? documentoEntradaId = null, Guid? documentoSaidaId = null, Guid? documentoConsumidorEntradaId = null, Guid? documentoConsumidorSaidaId = null, string? referenciaExterna = null)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            CompraId = compraId;
            EstoqueMovimentoManualId = estoqueMovimentoManualId;
            Origem = origem;
            DocumentoEntradaId = documentoEntradaId;
            DocumentoSaidaId = documentoSaidaId;
            DocumentoConsumidorEntradaId = documentoConsumidorEntradaId;
            DocumentoConsumidorSaidaId = documentoConsumidorSaidaId;
            ReferenciaExterna = referenciaExterna;
        }

        public void Validar() { }
    }
}
