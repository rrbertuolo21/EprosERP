using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class BannedIp : EntidadeSaaSBase
    {
        public string IpAddress { get; private set; } = string.Empty;
        public DateTime Expiracao { get; private set; }
        public string Motivo { get; private set; } = string.Empty;

        protected BannedIp() { } // EF Core

        public BannedIp(
            string ipAddress,
            DateTime expiracao,
            string motivo,
            string criadoPor)
            : base("system", criadoPor) // Sempre associado ao escopo global/system
        {
            if (string.IsNullOrWhiteSpace(ipAddress))
                AddNotification(nameof(IpAddress), "O endereço IP é obrigatório.");

            IpAddress = ipAddress.Trim();
            Expiracao = expiracao;
            Motivo = motivo ?? string.Empty;
        }

        public bool EstaExpirado()
        {
            return DateTime.UtcNow >= Expiracao;
        }
    }
}
