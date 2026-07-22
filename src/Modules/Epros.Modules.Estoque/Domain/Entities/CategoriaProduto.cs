using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    public class CategoriaProduto : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        // Campo do legado: agrupador de produtos (opcional no novo modelo SaaS).
        public Guid? ProdutoGrupoId { get; private set; }

        // Navegação intra-módulo
        public ProdutoGrupo? ProdutoGrupo { get; private set; }

        // EF Core
        protected CategoriaProduto() { }

        public CategoriaProduto(string descricao, string tenantId, string criadoPor)
            : this(descricao, null, tenantId, criadoPor)
        {
        }

        public CategoriaProduto(string descricao, Guid? produtoGrupoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<CategoriaProduto>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição da categoria é obrigatória.")
                .IsLowerOrEqualsThan(descricao?.Length ?? 0, 150, nameof(Descricao), "A descrição deve ter no máximo 150 caracteres [Origem: CategoriaProduto]")
            );

            Descricao = descricao ?? string.Empty;
            ProdutoGrupoId = produtoGrupoId;
        }

        public void Alterar(string descricao, string usuario)
        {
            Alterar(descricao, ProdutoGrupoId, usuario);
        }

        public void Alterar(string descricao, Guid? produtoGrupoId, string usuario)
        {
            AddNotifications(new Contract<CategoriaProduto>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição da categoria é obrigatória.")
                .IsLowerOrEqualsThan(descricao?.Length ?? 0, 150, nameof(Descricao), "A descrição deve ter no máximo 150 caracteres [Origem: CategoriaProduto]")
            );

            if (IsValid)
            {
                Descricao = descricao ?? string.Empty;
                ProdutoGrupoId = produtoGrupoId;
                MarcarAlterado(usuario);
            }
        }
    }
}
