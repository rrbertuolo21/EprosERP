using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.DMS.Domain.Entities
{
    public class PecaReposicao : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public string? FamiliaTecnica { get; private set; }
        public string? Criticidade { get; private set; }
        public string Status { get; private set; } = "Ativo";

        protected PecaReposicao() { } // EF Core

        public PecaReposicao(
            Guid produtoId,
            string? familiaTecnica,
            string? criticidade,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PecaReposicao>()
                .Requires()
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O produto é obrigatório.")
            );

            ProdutoId = produtoId;
            FamiliaTecnica = familiaTecnica;
            Criticidade = criticidade;
            Status = "Ativo";
        }

        public void Inativar(string usuario)
        {
            Status = "Inativo";
            MarcarAlterado(usuario);
        }
    }
}
