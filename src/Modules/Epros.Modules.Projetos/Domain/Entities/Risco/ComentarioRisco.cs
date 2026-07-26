using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Risco
{
    /// <summary>
    /// Comentario colaborativo do risco/issue. Origem: EF PRJ-RSK 11.3 (prj_risco_comentario / BugComment).
    /// RN-RSK-011: comentario obrigatorio e textual.
    /// </summary>
    public class ComentarioRisco : EntidadeSaaSBase
    {
        public Guid RiscoId { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string Comentario { get; private set; } = string.Empty;
        public DateTime DataHora { get; private set; }

        protected ComentarioRisco() { } // EF Core

        public ComentarioRisco(
            Guid riscoId,
            Guid usuarioId,
            string comentario,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ComentarioRisco>()
                .Requires()
                .AreNotEquals(riscoId, Guid.Empty, nameof(RiscoId), "O risco e obrigatorio. [Origem: ComentarioRisco]")
                .AreNotEquals(usuarioId, Guid.Empty, nameof(UsuarioId), "O autor do comentario e obrigatorio. [Origem: ComentarioRisco]")
                .IsNotNullOrEmpty(comentario, nameof(Comentario), "O comentario e obrigatorio. [Origem: ComentarioRisco]"));

            RiscoId = riscoId;
            UsuarioId = usuarioId;
            Comentario = comentario ?? string.Empty;
            DataHora = DateTime.UtcNow;
        }
    }
}
