using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class PersonalAccessToken : EntidadeSaaSBase
    {
        public Guid UsuarioId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public string TokenHash { get; private set; } = string.Empty;
        public DateTime Expiracao { get; private set; }
        public DateTime? UltimoUso { get; private set; }
        public bool Revogado { get; private set; }

        protected PersonalAccessToken() { } // EF Core

        public PersonalAccessToken(
            string tenantId,
            Guid usuarioId,
            string nome,
            string tokenHash,
            DateTime expiracao,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (usuarioId == Guid.Empty)
                AddNotification(nameof(UsuarioId), "O ID do usuário é obrigatório.");
            if (string.IsNullOrWhiteSpace(nome))
                AddNotification(nameof(Nome), "O nome do token é obrigatório.");
            if (string.IsNullOrWhiteSpace(tokenHash))
                AddNotification(nameof(TokenHash), "O hash do token é obrigatório.");

            UsuarioId = usuarioId;
            Nome = nome;
            TokenHash = tokenHash;
            Expiracao = expiracao;
            Revogado = false;
        }

        public void RegistrarUso()
        {
            UltimoUso = DateTime.UtcNow;
        }

        public void Revogar(string alteradoPor)
        {
            Revogado = true;
            MarcarAlterado(alteradoPor);
        }

        public bool EstaAtivoEValido()
        {
            return !Revogado && Expiracao > DateTime.UtcNow && EstaAtivo();
        }
    }
}
