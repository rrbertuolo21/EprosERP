using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Categoria : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public DateTime? AddedDate { get; private set; }
        public string? Image { get; private set; }
        public bool Ativo { get; private set; }

        protected Categoria() { } // EF Core

        public Categoria(string nome, DateTime? addedDate, string? image, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Categoria>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome da categoria é obrigatório.")
                .HasMaxLen(nome, 100, nameof(Nome), "Nome da categoria deve ter no máximo 100 caracteres.")
            );

            if (image != null)
            {
                AddNotifications(new Contract<Categoria>()
                    .Requires()
                    .HasMaxLen(image, 500, nameof(Image), "O link ou dados da imagem deve ter no máximo 500 caracteres.")
                );
            }

            Nome = nome;
            AddedDate = addedDate ?? DateTime.UtcNow;
            Image = image;
            Ativo = true;
        }

        public void Atualizar(string nome, string? image, string alteradoPor)
        {
            AddNotifications(new Contract<Categoria>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome da categoria é obrigatório.")
                .HasMaxLen(nome, 100, nameof(Nome), "Nome da categoria deve ter no máximo 100 caracteres.")
            );

            if (image != null)
            {
                AddNotifications(new Contract<Categoria>()
                    .Requires()
                    .HasMaxLen(image, 500, nameof(Image), "O link ou dados da imagem deve ter no máximo 500 caracteres.")
                );
            }

            if (IsValid)
            {
                Nome = nome;
                Image = image;
                MarcarAlterado(alteradoPor);
            }
        }

        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }
    }
}
