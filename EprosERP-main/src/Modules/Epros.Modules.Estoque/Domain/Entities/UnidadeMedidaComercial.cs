using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    public class UnidadeMedidaComercial : EntidadeSaaSBase
    {
        public string UnidadeMedida { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public decimal Fator { get; private set; }
        // Campo do legado: agrupador de produtos (opcional no novo modelo SaaS).
        public Guid? ProdutoGrupoId { get; private set; }

        // Navegação intra-módulo
        public ProdutoGrupo? ProdutoGrupo { get; private set; }

        // EF Core
        protected UnidadeMedidaComercial() { }

        public UnidadeMedidaComercial(string unidadeMedida, string descricao, decimal fator, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<UnidadeMedidaComercial>()
                .Requires()
                .IsNotNullOrEmpty(unidadeMedida, nameof(UnidadeMedida), "A unidade de medida é obrigatória.")
                .IsLowerOrEqualsThan(unidadeMedida?.Length ?? 0, 6, nameof(UnidadeMedida), "O campo UnidadeMedida deve ter no máximo 6 caracteres [Origem: UnidadeMedidaComercial]")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição é obrigatória.")
                .IsLowerOrEqualsThan(descricao?.Length ?? 0, 30, nameof(Descricao), "O campo Descricao deve ter no máximo 30 caracteres [Origem: UnidadeMedidaComercial]")
                .IsGreaterThan(fator, 0, nameof(Fator), "O fator deve ser maior que zero [Origem: UnidadeMedidaComercial]")
            );

            UnidadeMedida = unidadeMedida ?? string.Empty;
            Descricao = descricao ?? string.Empty;
            Fator = fator;
        }

        public UnidadeMedidaComercial(string unidadeMedida, string descricao, decimal fator, Guid? produtoGrupoId, string tenantId, string criadoPor)
            : this(unidadeMedida, descricao, fator, tenantId, criadoPor)
        {
            ProdutoGrupoId = produtoGrupoId;
        }

        public void Alterar(string unidadeMedida, string descricao, decimal fator, string usuario)
        {
            Alterar(unidadeMedida, descricao, fator, ProdutoGrupoId, usuario);
        }

        public void Alterar(string unidadeMedida, string descricao, decimal fator, Guid? produtoGrupoId, string usuario)
        {
            AddNotifications(new Contract<UnidadeMedidaComercial>()
                .Requires()
                .IsNotNullOrEmpty(unidadeMedida, nameof(UnidadeMedida), "A unidade de medida é obrigatória.")
                .IsLowerOrEqualsThan(unidadeMedida?.Length ?? 0, 6, nameof(UnidadeMedida), "O campo UnidadeMedida deve ter no máximo 6 caracteres [Origem: UnidadeMedidaComercial]")
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição é obrigatória.")
                .IsLowerOrEqualsThan(descricao?.Length ?? 0, 30, nameof(Descricao), "O campo Descricao deve ter no máximo 30 caracteres [Origem: UnidadeMedidaComercial]")
                .IsGreaterThan(fator, 0, nameof(Fator), "O fator deve ser maior que zero [Origem: UnidadeMedidaComercial]")
            );

            if (IsValid)
            {
                UnidadeMedida = unidadeMedida ?? string.Empty;
                Descricao = descricao ?? string.Empty;
                Fator = fator;
                ProdutoGrupoId = produtoGrupoId;
                MarcarAlterado(usuario);
            }
        }
    }
}
