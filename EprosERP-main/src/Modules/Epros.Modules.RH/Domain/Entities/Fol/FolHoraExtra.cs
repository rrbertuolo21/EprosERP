using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-FOL). Fidelidade campo a campo.</summary>
    public partial class FolHoraExtra : EntidadeSaaSBase
    {
        public string? Titulo { get; private set; }
        public Guid? ColaboradorId { get; private set; }
        public int? TotalDias { get; private set; }
        public decimal? Horas { get; private set; }
        public decimal? Taxa { get; private set; }
        public DateTime? DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public string? Notas { get; private set; }
        public string? Status { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected FolHoraExtra() { } // EF Core

        public FolHoraExtra(
            string? titulo,
            Guid? colaboradorId,
            int? totalDias,
            decimal? horas,
            decimal? taxa,
            DateTime? dataInicio,
            DateTime? dataFim,
            string? notas,
            string? status,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Titulo = titulo;
            ColaboradorId = colaboradorId;
            TotalDias = totalDias;
            Horas = horas;
            Taxa = taxa;
            DataInicio = dataInicio;
            DataFim = dataFim;
            Notas = notas;
            Status = status;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<FolHoraExtra>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
