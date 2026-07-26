using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class HistoricoLogin : EntidadeSaaSBase
    {
        public Guid? UsuarioId { get; private set; }
        public string EmailTentado { get; private set; } = string.Empty;
        public string IpAddress { get; private set; } = string.Empty;
        public string UserAgent { get; private set; } = string.Empty;
        public bool Sucesso { get; private set; }
        public string Detalhes { get; private set; } = string.Empty;
        public DateTime DataHora { get; private set; }

        protected HistoricoLogin() { } // EF Core

        public HistoricoLogin(
            string tenantId,
            Guid? usuarioId,
            string emailTentado,
            string ipAddress,
            string userAgent,
            bool sucesso,
            string detalhes,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            UsuarioId = usuarioId;
            EmailTentado = emailTentado ?? string.Empty;
            IpAddress = ipAddress ?? string.Empty;
            UserAgent = userAgent ?? string.Empty;
            Sucesso = sucesso;
            Detalhes = detalhes ?? string.Empty;
            DataHora = DateTime.UtcNow;
        }
    }
}
