using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class SessaoUsuario : EntidadeSaaSBase, IHardDeletable
    {
        public Guid UsuarioId { get; private set; }
        public string TokenSessao { get; private set; } = string.Empty;
        /// <summary>Identificador (claim jti) do token emitido para esta sessão — liga a sessão ao JWT (REG-013).</summary>
        public Guid Jti { get; private set; }
        public string IpAddress { get; private set; } = string.Empty;
        public string UserAgent { get; private set; } = string.Empty;
        public DateTime Expiracao { get; private set; }
        public bool Revogado { get; private set; }

        protected SessaoUsuario() { } // EF Core

        public SessaoUsuario(
            string tenantId,
            Guid usuarioId,
            string tokenSessao,
            string ipAddress,
            string userAgent,
            DateTime expiracao,
            string criadoPor,
            Guid jti = default)
            : base(tenantId, criadoPor)
        {
            if (usuarioId == Guid.Empty)
                AddNotification(nameof(UsuarioId), "O ID do usuário é obrigatório.");
            if (string.IsNullOrWhiteSpace(tokenSessao))
                AddNotification(nameof(TokenSessao), "O token de sessão é obrigatório.");

            UsuarioId = usuarioId;
            TokenSessao = tokenSessao;
            Jti = jti;
            IpAddress = ipAddress ?? string.Empty;
            UserAgent = userAgent ?? string.Empty;
            Expiracao = expiracao;
            Revogado = false;
        }

        public void Revogar(string alteradoPor)
        {
            Revogado = true;
            MarcarAlterado(alteradoPor);
        }

        public bool EstaValida()
        {
            return !Revogado && Expiracao > DateTime.UtcNow && EstaAtivo();
        }
    }
}
