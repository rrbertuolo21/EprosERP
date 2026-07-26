using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item solicitado na requisição (EF Sourcing e Compras §5.9 `sc_requisicao_item`).
    /// ItemCotado é indicador funcional preservado do material.
    /// </summary>
    public class ScRequisicaoItem : EntidadeSaaSBase
    {
        public Guid RequisicaoId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal? Quantidade { get; private set; }
        public decimal? QuantidadeCotada { get; private set; }
        public string ItemCotado { get; private set; } = string.Empty;

        // Navegação intra-módulo
        public ScRequisicao? Requisicao { get; private set; }
        public Produto? Produto { get; private set; }

        protected ScRequisicaoItem() { } // EF Core

        public ScRequisicaoItem(Guid requisicaoId, Guid produtoId, decimal? quantidade, decimal? quantidadeCotada, string itemCotado, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            RequisicaoId = requisicaoId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            QuantidadeCotada = quantidadeCotada;
            ItemCotado = itemCotado ?? string.Empty;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ScRequisicaoItem>()
                .Requires()
                .AreNotEquals(ProdutoId, Guid.Empty, nameof(ProdutoId), "O produto do item de requisição é obrigatório [Origem: ScRequisicaoItem]"));
        }
    }
}
