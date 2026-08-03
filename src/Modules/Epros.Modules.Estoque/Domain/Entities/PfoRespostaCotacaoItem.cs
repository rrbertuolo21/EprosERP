using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>Item da resposta de cotação (EF Portal do Fornecedor §15.5 `pfo_resposta_cotacao_item`).</summary>
    public class PfoRespostaCotacaoItem : EntidadeSaaSBase
    {
        public Guid RespostaCotacaoId { get; private set; }
        public Guid? ItemOrigemId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public decimal? Quantidade { get; private set; }
        public decimal? ValorUnitario { get; private set; }
        public int? PrazoEntregaDias { get; private set; }

        public PfoRespostaCotacao? Resposta { get; private set; }

        protected PfoRespostaCotacaoItem() { }

        public PfoRespostaCotacaoItem(Guid respostaCotacaoId, Guid? itemOrigemId, Guid? produtoId, decimal? quantidade, decimal? valorUnitario, int? prazoEntregaDias, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            RespostaCotacaoId = respostaCotacaoId;
            ItemOrigemId = itemOrigemId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            ValorUnitario = valorUnitario;
            PrazoEntregaDias = prazoEntregaDias;
        }
    }
}
