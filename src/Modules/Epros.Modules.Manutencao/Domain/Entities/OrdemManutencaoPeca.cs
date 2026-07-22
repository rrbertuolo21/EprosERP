using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Manutencao.Domain.Entities
{
    public class OrdemManutencaoPeca : EntidadeSaaSBase
    {
        public Guid OrdemManutencaoId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }

        protected OrdemManutencaoPeca() { } // EF Core

        public OrdemManutencaoPeca(
            Guid ordemManutencaoId,
            Guid produtoId,
            decimal quantidade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<OrdemManutencaoPeca>()
                .Requires()
                .AreNotEquals(ordemManutencaoId, Guid.Empty, nameof(OrdemManutencaoId), "O ID da ordem de manutenção é obrigatório.")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O ID do produto (peça) é obrigatório.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A quantidade deve ser maior que zero.")
            );

            OrdemManutencaoId = ordemManutencaoId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
        }
    }
}
