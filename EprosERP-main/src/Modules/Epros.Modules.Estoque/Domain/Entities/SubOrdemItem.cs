using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item da ordem de subcontratação (EF Subcontratação §7.2 `sub_ordem_item`). Produto/material e
    /// quantidade planejada. ProdutoId é referência externa por FK Guid. Modelo proposto por autoria (§16).
    /// </summary>
    public class SubOrdemItem : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal QuantidadePlanejada { get; private set; }
        public string? Unidade { get; private set; }
        public string? OperacaoTerceirizada { get; private set; }

        // Navegação intra-módulo
        public SubOrdem? Ordem { get; private set; }

        protected SubOrdemItem() { } // EF Core

        public SubOrdemItem(Guid ordemId, Guid produtoId, decimal quantidadePlanejada, string? unidade, string? operacaoTerceirizada, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            ProdutoId = produtoId;
            QuantidadePlanejada = quantidadePlanejada;
            Unidade = unidade;
            OperacaoTerceirizada = operacaoTerceirizada;
            Validar();
        }

        public void Validar()
        {
            Clear();
            if (ProdutoId == Guid.Empty)
                AddNotification("ProdutoId", "O produto/material é obrigatório [SUB-003] [Origem: SubOrdemItem]");
            if (QuantidadePlanejada <= 0m)
                AddNotification("QuantidadePlanejada", "A quantidade planejada deve ser maior que zero [Origem: SubOrdemItem]");
        }
    }
}
