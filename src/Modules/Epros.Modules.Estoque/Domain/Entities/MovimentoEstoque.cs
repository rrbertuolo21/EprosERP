using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    public class MovimentoEstoque : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public string Tipo { get; private set; } = string.Empty; // Entrada, Saida
        public string Historico { get; private set; } = string.Empty;

        protected MovimentoEstoque() { } // EF Core

        public MovimentoEstoque(
            Guid produtoId,
            decimal quantidade,
            string tipo,
            string historico,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<MovimentoEstoque>()
                .Requires()
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O ID do produto é obrigatório.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade de movimentação deve ser maior que zero.")
                .IsNotNullOrEmpty(tipo, nameof(Tipo), "O Tipo de movimentação é obrigatório.")
            );

            ProdutoId = produtoId;
            Quantidade = quantidade;
            Tipo = tipo;
            Historico = historico;
        }
    }
}
