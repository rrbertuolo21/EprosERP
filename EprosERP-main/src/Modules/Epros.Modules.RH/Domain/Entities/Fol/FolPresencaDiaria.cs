using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolPresencaDiaria : EntidadeSaaSBase
    {
        public DateTime Data { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public string Status { get; private set; } = string.Empty;
        public string? Narrativa { get; private set; }

        protected FolPresencaDiaria() { } // EF Core

        public FolPresencaDiaria(
            DateTime data,
            Guid colaboradorId,
            string status,
            string? narrativa,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Data = data;
            ColaboradorId = colaboradorId;
            Status = status;
            Narrativa = narrativa;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolPresencaDiaria>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
