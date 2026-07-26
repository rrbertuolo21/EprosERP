using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Armazem : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string? Pais { get; private set; }
        public string? Cidade { get; private set; }
        public string? Mobile { get; private set; }
        public string? Email { get; private set; }

        protected Armazem() { } // EF Core

        public Armazem(string nome, string? pais, string? cidade, string? mobile, string? email, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<Armazem>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do armazém é obrigatório.")
                .HasMaxLen(nome, 100, nameof(Nome), "Nome do armazém deve ter no máximo 100 caracteres.")
            );

            if (pais != null)
                AddNotifications(new Contract<Armazem>().Requires().HasMaxLen(pais, 100, nameof(Pais), "País deve ter no máximo 100 caracteres."));
            if (cidade != null)
                AddNotifications(new Contract<Armazem>().Requires().HasMaxLen(cidade, 100, nameof(Cidade), "Cidade deve ter no máximo 100 caracteres."));
            if (mobile != null)
                AddNotifications(new Contract<Armazem>().Requires().HasMaxLen(mobile, 30, nameof(Mobile), "Telefone/Celular deve ter no máximo 30 caracteres."));
            if (email != null)
                AddNotifications(new Contract<Armazem>().Requires().HasMaxLen(email, 150, nameof(Email), "E-mail deve ter no máximo 150 caracteres."));

            Nome = nome;
            Pais = pais;
            Cidade = cidade;
            Mobile = mobile;
            Email = email;
        }

        public void Atualizar(string nome, string? pais, string? cidade, string? mobile, string? email, string alteradoPor)
        {
            AddNotifications(new Contract<Armazem>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "Nome do armazém é obrigatório.")
                .HasMaxLen(nome, 100, nameof(Nome), "Nome do armazém deve ter no máximo 100 caracteres.")
            );

            if (pais != null)
                AddNotifications(new Contract<Armazem>().Requires().HasMaxLen(pais, 100, nameof(Pais), "País deve ter no máximo 100 caracteres."));
            if (cidade != null)
                AddNotifications(new Contract<Armazem>().Requires().HasMaxLen(cidade, 100, nameof(Cidade), "Cidade deve ter no máximo 100 caracteres."));
            if (mobile != null)
                AddNotifications(new Contract<Armazem>().Requires().HasMaxLen(mobile, 30, nameof(Mobile), "Telefone/Celular deve ter no máximo 30 caracteres."));
            if (email != null)
                AddNotifications(new Contract<Armazem>().Requires().HasMaxLen(email, 150, nameof(Email), "E-mail deve ter no máximo 150 caracteres."));

            if (IsValid)
            {
                Nome = nome;
                Pais = pais;
                Cidade = cidade;
                Mobile = mobile;
                Email = email;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
