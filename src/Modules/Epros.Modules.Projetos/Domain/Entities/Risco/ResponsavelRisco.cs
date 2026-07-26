using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Risco
{
    /// <summary>
    /// Vinculo N:N entre risco/issue e responsaveis (assigned_to). Origem: EF PRJ-RSK 10.3 / 11.1 (assigned_to, min 1).
    /// </summary>
    public class ResponsavelRisco : EntidadeSaaSBase
    {
        public Guid RiscoId { get; private set; }
        public Guid UsuarioId { get; private set; }

        protected ResponsavelRisco() { } // EF Core

        public ResponsavelRisco(
            Guid riscoId,
            Guid usuarioId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ResponsavelRisco>()
                .Requires()
                .AreNotEquals(riscoId, Guid.Empty, nameof(RiscoId), "O risco e obrigatorio. [Origem: ResponsavelRisco]")
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O responsavel e obrigatorio. [Origem: ResponsavelRisco]"));

            RiscoId = riscoId;
            UsuarioId = usuarioId;
        }
    }
}
