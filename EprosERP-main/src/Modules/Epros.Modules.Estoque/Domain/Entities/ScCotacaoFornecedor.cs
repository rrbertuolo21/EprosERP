using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Participação de um fornecedor na cotação (EF Sourcing e Compras §5.9 / §9.2 `sc_cotacao_fornecedor`).
    /// FornecedorId é FK Guid (fornecedor é referência externa — sem navegação cruzada).
    /// Subtotal, desconto e total por fornecedor (fluxo §9.2 item 3).
    /// </summary>
    public class ScCotacaoFornecedor : EntidadeSaaSBase
    {
        public Guid CotacaoId { get; private set; }
        public Guid FornecedorId { get; private set; }
        public string PrazoEntrega { get; private set; } = string.Empty;
        public string CondicoesPagamento { get; private set; } = string.Empty;
        public decimal Subtotal { get; private set; }
        public decimal Desconto { get; private set; }
        public decimal Total { get; private set; }

        // Navegação intra-módulo
        public ScCotacao? Cotacao { get; private set; }

        protected ScCotacaoFornecedor() { } // EF Core

        public ScCotacaoFornecedor(Guid cotacaoId, Guid fornecedorId, string prazoEntrega, string condicoesPagamento, decimal subtotal, decimal desconto, decimal total, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CotacaoId = cotacaoId;
            FornecedorId = fornecedorId;
            PrazoEntrega = prazoEntrega ?? string.Empty;
            CondicoesPagamento = condicoesPagamento ?? string.Empty;
            Subtotal = subtotal;
            Desconto = desconto;
            Total = total;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ScCotacaoFornecedor>()
                .Requires()
                .AreNotEquals(FornecedorId, Guid.Empty, nameof(FornecedorId), "O fornecedor da cotação é obrigatório [SC-045] [Origem: ScCotacaoFornecedor]"));
        }
    }
}
