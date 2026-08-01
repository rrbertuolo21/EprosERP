using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class UsuarioInterno : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Senha { get; private set; } = string.Empty;
        public Guid? CreatorId { get; private set; }
        public string? UniqueId { get; private set; }
        public string? Timezone { get; private set; }
        public bool PrimaryAdmin { get; private set; }

        protected UsuarioInterno() { } // EF Core

        public UsuarioInterno(
            string nome,
            string email,
            string senha,
            Guid? creatorId,
            string? uniqueId,
            string? timezone,
            bool primaryAdmin,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<UsuarioInterno>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do usuário interno é obrigatório")
                .IsNotNullOrEmpty(email, nameof(Email), "O email do usuário interno é obrigatório")
                .IsEmail(email, nameof(Email), "O formato do email é inválido")
                .IsNotNullOrEmpty(senha, nameof(Senha), "A senha do usuário interno é obrigatória")
            );

            if (senha != null && senha.Length < 8)
            {
                AddNotification(nameof(Senha), "A senha deve ter no mínimo 8 caracteres"); // REG-014
            }

            Nome = nome;
            Email = email;
            Senha = senha ?? string.Empty;
            CreatorId = creatorId;
            UniqueId = uniqueId;
            Timezone = timezone;
            PrimaryAdmin = primaryAdmin;
        }

        public void Atualizar(string nome, string email, string? timezone, string alteradoPor)
        {
            AddNotifications(new Contract<UsuarioInterno>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do usuário interno é obrigatório")
                .IsNotNullOrEmpty(email, nameof(Email), "O email do usuário interno é obrigatório")
                .IsEmail(email, nameof(Email), "O formato do email é inválido")
            );

            if (IsValid)
            {
                Nome = nome;
                Email = email;
                Timezone = timezone;
                MarcarAlterado(alteradoPor);
            }
        }

        public void AlterarSenha(string novaSenha, string alteradoPor)
        {
            AddNotifications(new Contract<UsuarioInterno>()
                .Requires()
                .IsNotNullOrEmpty(novaSenha, nameof(Senha), "A senha do usuário interno é obrigatória")
            );

            if (novaSenha != null && novaSenha.Length < 8)
            {
                AddNotification(nameof(Senha), "A senha deve ter no mínimo 8 caracteres");
            }

            if (IsValid)
            {
                Senha = novaSenha ?? string.Empty;
                MarcarAlterado(alteradoPor);
            }
        }

        public void TornarAdminPrincipal(string alteradoPor)
        {
            PrimaryAdmin = true;
            MarcarAlterado(alteradoPor);
        }
    }
}
