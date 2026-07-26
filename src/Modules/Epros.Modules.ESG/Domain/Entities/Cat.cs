using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Comunicacao de Acidente de Trabalho (EF GESTAO_AMBIENTAL_EHS 11.2). Unica por TenantId+NumeroCat quando informado.</summary>
    public class Cat : EntidadeSaaSBase
    {
        public Guid? IncidenteId { get; private set; }
        public Guid? IdFolhaPpp { get; private set; }
        public string? NumeroCat { get; private set; }
        public DateTime? DataAfastamento { get; private set; }
        public DateTime? DataRegistro { get; private set; }

        protected Cat() { } // EF Core

        public Cat(
            Guid? incidenteId,
            Guid? idFolhaPpp,
            string? numeroCat,
            DateTime? dataAfastamento,
            DateTime? dataRegistro,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            IncidenteId = incidenteId;
            IdFolhaPpp = idFolhaPpp;
            NumeroCat = numeroCat;
            DataAfastamento = dataAfastamento?.Date;
            DataRegistro = dataRegistro?.Date;
        }
    }
}
