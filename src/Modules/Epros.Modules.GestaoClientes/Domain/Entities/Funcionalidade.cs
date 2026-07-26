using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Catálogo global de funcionalidades exibíveis (APP-CAT 11.3). Sem tenant (IGlobalEntity).</summary>
    public class Funcionalidade : EntidadeSaaSBase, IGlobalEntity
    {
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        protected Funcionalidade() { } // EF Core

        public Funcionalidade(string title, string description, string criadoPor)
            : base("system", criadoPor)
        {
            Title = title;
            Description = description;
            Validar();
        }

        public void Alterar(string title, string description, string alteradoPor)
        {
            Title = title;
            Description = description;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Funcionalidade>()
                .Requires()
                .IsNotNullOrEmpty(Title, nameof(Title), "Title é obrigatório [Origem: Funcionalidade]")
                .HasMaxLen(Title ?? string.Empty, 150, nameof(Title), "Title deve ter no máximo 150 caracteres [Origem: Funcionalidade]")
                .IsNotNullOrEmpty(Description, nameof(Description), "Description é obrigatória [Origem: Funcionalidade]")
                .HasMaxLen(Description ?? string.Empty, 1000, nameof(Description), "Description deve ter no máximo 1000 caracteres [Origem: Funcionalidade]")
            );
        }
    }
}
