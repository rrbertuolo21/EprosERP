using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-DEN — Categoria de denuncia (grc_den_categoria). Classifica denuncias e organiza
    /// filtros, paineis e SLA. Fiel a EF_13_GRC_INVESTIGACOES_E_DENUNCIAS_V1 (secao 11.1).
    /// Preserva name, description, color, is_active, creator_id.
    /// </summary>
    public class DenunciaCategoria : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty; // origem material: name
        public string? Descricao { get; private set; } // origem material: description
        public string? Cor { get; private set; } // origem material: color
        public bool Ativa { get; private set; } // origem material: is_active
        public Guid? CriadorId { get; private set; } // origem material: creator_id

        protected DenunciaCategoria() { } // EF Core

        public DenunciaCategoria(
            string nome,
            string? descricao,
            string? cor,
            Guid? criadorId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DenunciaCategoria>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da categoria é obrigatório.")
            );

            Nome = nome;
            Descricao = descricao;
            Cor = cor;
            CriadorId = criadorId;
            Ativa = true;
        }

        public void Alterar(string nome, string? descricao, string? cor, string usuario)
        {
            AddNotifications(new Contract<DenunciaCategoria>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome da categoria é obrigatório.")
            );
            if (!IsValid) return;

            Nome = nome;
            Descricao = descricao;
            Cor = cor;
            MarcarAlterado(usuario);
        }

        /// <summary>RN-DEN-004: categoria inativa nao pode ser usada em nova denuncia.</summary>
        public void Inativar(string usuario)
        {
            Ativa = false;
            MarcarAlterado(usuario);
        }

        public void Reativar(string usuario)
        {
            Ativa = true;
            MarcarAlterado(usuario);
        }
    }
}
