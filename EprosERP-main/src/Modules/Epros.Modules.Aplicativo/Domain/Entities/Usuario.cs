using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public enum UsuarioStatus
    {
        Active = 1,
        Pending = 2,
        Disabled = 3,
        Suspended = 4
    }

    public enum UsuarioTipo
    {
        Company = 1,
        Team = 2,
        Client = 3
    }

    public class Usuario : EntidadeSaaSBase
    {
        /// <summary>Sequência de exibição por tenant (porte do campo SequenciaTenantId do legado Usuario).</summary>
        public long? SequenciaExibicao { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        /// <summary>Login de acesso (porte do campo Login do legado Usuario; distinto do Email).</summary>
        public string? Login { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string? Telefone { get; private set; }
        public bool MfaHabilitado { get; private set; }
        public UsuarioStatus Status { get; private set; }
        public UsuarioTipo Tipo { get; private set; }
        public bool ForcarTrocaSenha { get; private set; }
        public int AccessFailedCount { get; private set; }
        public DateTime? LockoutEnd { get; private set; }
        public string? ForgotPasswordToken { get; private set; }
        public DateTime? ForgotPasswordTokenExpiry { get; private set; }
        public string? ApiKey { get; private set; }
        public DateTime? ApiKeyCreated { get; private set; }
        public DateTime? ApiKeyExpiration { get; private set; }
        public DateTime? ApiKeyLastUsed { get; private set; }
        public int ApiKeyRateLimit { get; private set; } = 60;

        protected Usuario() { } // EF Core

        public Usuario(
            string tenantId,
            string nome,
            string email,
            string passwordHash,
            UsuarioTipo tipo,
            string criadoPor) 
            : base(tenantId, criadoPor)
        {
            if (string.IsNullOrWhiteSpace(nome))
                AddNotification(nameof(Nome), "O nome é obrigatório.");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                AddNotification(nameof(Email), "O e-mail informado é inválido.");
            if (string.IsNullOrWhiteSpace(passwordHash))
                AddNotification(nameof(PasswordHash), "A senha é obrigatória.");

            Nome = nome;
            Email = email.ToLowerInvariant().Trim();
            PasswordHash = passwordHash;
            Tipo = tipo;
            Status = UsuarioStatus.Active; // Inicializa como ativo
            MfaHabilitado = false;
            ForcarTrocaSenha = false;
            AccessFailedCount = 0;
            ApiKeyRateLimit = 60;
        }

        /// <summary>
        /// Define um novo hash de senha (já derivado por <see cref="Epros.Shared.Application.Contracts.IPasswordHasher"/>).
        /// A entidade nunca recebe nem armazena senha em texto puro; a verificação de igualdade com a
        /// senha anterior não é possível com hashes salgados e, portanto, não é feita aqui.
        /// </summary>
        public void AlterarSenha(string novoHash, string alteradoPor)
        {
            if (string.IsNullOrWhiteSpace(novoHash))
            {
                AddNotification(nameof(PasswordHash), "A nova senha não pode ser vazia.");
                return;
            }

            PasswordHash = novoHash;
            ForcarTrocaSenha = false;
            MarcarAlterado(alteradoPor);
        }

        public void RegistrarFalhaLogin(int limiteTentativas, TimeSpan tempoLockout)
        {
            AccessFailedCount++;
            if (AccessFailedCount >= limiteTentativas)
            {
                LockoutEnd = DateTime.UtcNow.Add(tempoLockout);
                Status = UsuarioStatus.Suspended; // Bloqueia a conta temporariamente
            }
        }

        public void ResetarFalhasLogin()
        {
            AccessFailedCount = 0;
            LockoutEnd = null;
            if (Status == UsuarioStatus.Suspended)
            {
                Status = UsuarioStatus.Active;
            }
        }

        public void Bloquear(string alteradoPor)
        {
            Status = UsuarioStatus.Disabled;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(string alteradoPor)
        {
            Status = UsuarioStatus.Active;
            MarcarAlterado(alteradoPor);
        }

        public void DefinirForcarTrocaSenha(bool forcar, string alteradoPor)
        {
            ForcarTrocaSenha = forcar;
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarTelefone(string? telefone, string alteradoPor)
        {
            Telefone = telefone;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Define/atualiza o login de acesso (porte do campo Login do legado).</summary>
        public void DefinirLogin(string? login, string alteradoPor)
        {
            if (!string.IsNullOrWhiteSpace(login) && login.Length > 20)
            {
                AddNotification(nameof(Login), "O campo Login deve ter no máximo 20 caracteres.");
                return;
            }

            Login = login;
            MarcarAlterado(alteradoPor);
        }

        public void GerarTokenRecuperacaoSenha(string token, TimeSpan validade)
        {
            ForgotPasswordToken = token;
            ForgotPasswordTokenExpiry = DateTime.UtcNow.Add(validade);
        }

        public void LimparTokenRecuperacaoSenha()
        {
            ForgotPasswordToken = null;
            ForgotPasswordTokenExpiry = null;
        }

        public void GerarApiKey(int rateLimit, int validadeDias, string alteradoPor)
        {
            ApiKey = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
            ApiKeyCreated = DateTime.UtcNow;
            ApiKeyExpiration = DateTime.UtcNow.AddDays(validadeDias);
            ApiKeyRateLimit = rateLimit > 0 ? rateLimit : 60;
            MarcarAlterado(alteradoPor);
        }

        public void RevogarApiKey(string alteradoPor)
        {
            ApiKey = null;
            ApiKeyCreated = null;
            ApiKeyExpiration = null;
            ApiKeyLastUsed = null;
            MarcarAlterado(alteradoPor);
        }

        public void AtualizarUltimoUsoApiKey()
        {
            ApiKeyLastUsed = DateTime.UtcNow;
        }
    }
}
