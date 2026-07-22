using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class SessaoUsuario : EntidadeSaaSBase, IHardDeletable
    {
        public Guid UsuarioId { get; private set; }
        public string TokenSessao { get; private set; } = string.Empty;
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
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            if (usuarioId == Guid.Empty)
                AddNotification(nameof(UsuarioId), "O ID do usuário é obrigatório.");
            if (string.IsNullOrWhiteSpace(tokenSessao))
                AddNotification(nameof(TokenSessao), "O token de sessão é obrigatório.");

            UsuarioId = usuarioId;
            TokenSessao = tokenSessao;
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
