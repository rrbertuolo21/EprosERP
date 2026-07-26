using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Projeto : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;

        protected Projeto() { } // EF Core

        public Projeto(string nome, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Projeto>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do projeto é obrigatório.")
                .HasMaxLen(nome, 150, nameof(Nome), "Nome do projeto deve ter no máximo 150 caracteres.")
            );

            Nome = nome;
        }

        public void Atualizar(string nome, string alteradoPor)
        {
            AddNotifications(new Contract<Projeto>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do projeto é obrigatório.")
                .HasMaxLen(nome, 150, nameof(Nome), "Nome do projeto deve ter no máximo 150 caracteres.")
            );

            if (IsValid)
            {
                Nome = nome;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
