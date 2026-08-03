using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    /// <summary>
    /// Perfil de suporte da Siser (landlord). Define que o portador — uma pessoa da Siser — pode
    /// abrir uma sessão de suporte em QUALQUER tenant cliente para ajudá-lo, sem precisar de
    /// membership por cliente. Formaliza a antiga impersonação como "acesso por perfil de suporte"
    /// (com auditoria obrigatória). Decisão do Rafael (1.04 — DECISOES_IMPLANTACAO_V1.md, item 2).
    /// </summary>
    public enum PerfilSuporteSiser
    {
        /// <summary>Sem perfil de suporte: não pode abrir sessão de suporte em cliente.</summary>
        Nenhum = 0,
        /// <summary>Suporte Técnico: acessa qualquer cliente para diagnóstico/correção técnica.</summary>
        SuporteTecnico = 1,
        /// <summary>Suporte Negócio: acessa qualquer cliente para apoio a regras/operação de negócio.</summary>
        SuporteNegocio = 2
    }

    public class UsuarioInterno : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Senha { get; private set; } = string.Empty;
        public Guid? CreatorId { get; private set; }
        public string? UniqueId { get; private set; }
        public string? Timezone { get; private set; }
        public bool PrimaryAdmin { get; private set; }

        /// <summary>Perfil de suporte da Siser (papel de sistema). Habilita acesso cross-tenant de suporte.</summary>
        public PerfilSuporteSiser PerfilSuporte { get; private set; }

        /// <summary>
        /// True se este usuário da Siser pode abrir sessão de suporte em um cliente:
        /// possui um perfil de suporte OU é o admin primário do landlord.
        /// </summary>
        public bool PodeDarSuporte => PerfilSuporte != PerfilSuporteSiser.Nenhum || PrimaryAdmin;

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
            string criadoPor,
            PerfilSuporteSiser perfilSuporte = PerfilSuporteSiser.Nenhum) : base(tenantId, criadoPor)
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
            PerfilSuporte = perfilSuporte;
        }

        /// <summary>Define/altera o perfil de suporte da Siser deste usuário (papel de sistema).</summary>
        public void DefinirPerfilSuporte(PerfilSuporteSiser perfilSuporte, string alteradoPor)
        {
            PerfilSuporte = perfilSuporte;
            MarcarAlterado(alteradoPor);
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
