using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    public class MarcaProduto : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        // Campo do legado: agrupador de produtos (opcional no novo modelo SaaS).
        public Guid? ProdutoGrupoId { get; private set; }

        // Navegação intra-módulo
        public ProdutoGrupo? ProdutoGrupo { get; private set; }

        // EF Core
        protected MarcaProduto() { }

        public MarcaProduto(string descricao, string tenantId, string criadoPor)
            : this(descricao, null, tenantId, criadoPor)
        {
        }

        public MarcaProduto(string descricao, Guid? produtoGrupoId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<MarcaProduto>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição da marca é obrigatória.")
                .IsLowerOrEqualsThan(descricao?.Length ?? 0, 150, nameof(Descricao), "A descrição deve ter no máximo 150 caracteres [Origem: MarcaProduto]")
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
            AddNotifications(new Contract<MarcaProduto>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição da marca é obrigatória.")
                .IsLowerOrEqualsThan(descricao?.Length ?? 0, 150, nameof(Descricao), "A descrição deve ter no máximo 150 caracteres [Origem: MarcaProduto]")
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
