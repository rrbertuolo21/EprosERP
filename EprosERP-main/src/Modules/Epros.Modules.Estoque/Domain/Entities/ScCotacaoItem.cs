using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Produto, quantidade e valores cotados por fornecedor (EF Sourcing e Compras §5.9 `sc_cotacao_item`).
    /// CotacaoFornecedorId vincula o item ao fornecedor cotado (fluxo §9.2 item 4).
    /// </summary>
    public class ScCotacaoItem : EntidadeSaaSBase
    {
        public Guid CotacaoId { get; private set; }
        public Guid? CotacaoFornecedorId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal? Quantidade { get; private set; }
        public decimal? ValorUnitario { get; private set; }
        public decimal? ValorDesconto { get; private set; }
        public decimal? ValorTotal { get; private set; }

        // Navegação intra-módulo
        public ScCotacao? Cotacao { get; private set; }
        public Produto? Produto { get; private set; }

        protected ScCotacaoItem() { } // EF Core

        public ScCotacaoItem(Guid cotacaoId, Guid? cotacaoFornecedorId, Guid produtoId, decimal? quantidade, decimal? valorUnitario, decimal? valorDesconto, decimal? valorTotal, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CotacaoId = cotacaoId;
            CotacaoFornecedorId = cotacaoFornecedorId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            ValorDesconto = valorDesconto;
            ValorTotal = valorTotal;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ScCotacaoItem>()
                .Requires()
                .AreNotEquals(ProdutoId, Guid.Empty, nameof(ProdutoId), "O produto do item cotado é obrigatório [Origem: ScCotacaoItem]"));
        }
    }
}
