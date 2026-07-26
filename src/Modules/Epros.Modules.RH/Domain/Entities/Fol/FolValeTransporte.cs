using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolValeTransporte : EntidadeSaaSBase
    {
        public Guid ItinerarioTransporteId { get; private set; }
        public Guid ColaboradorId { get; private set; }
        public int? Quantidade { get; private set; }
        public decimal? PercentualDesconto { get; private set; }

        protected FolValeTransporte() { } // EF Core

        public FolValeTransporte(
            Guid itinerarioTransporteId,
            Guid colaboradorId,
            int? quantidade,
            decimal? percentualDesconto,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            ItinerarioTransporteId = itinerarioTransporteId;
            ColaboradorId = colaboradorId;
            Quantidade = quantidade;
            PercentualDesconto = percentualDesconto;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolValeTransporte>().Requires();
            contract.AreNotEquals(ItinerarioTransporteId, Guid.Empty, nameof(ItinerarioTransporteId), "O campo ItinerarioTransporteId e obrigatorio.");
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
