using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_categoria_documento). Fidelidade campo a campo.</summary>
    public partial class WfmCategoriaDocumento : EntidadeSaaSBase
    {
        public string? TipoDocumento { get; private set; }
        public bool? Status { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected WfmCategoriaDocumento() { } // EF Core

        public WfmCategoriaDocumento(
            string? tipoDocumento,
            bool? status,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            TipoDocumento = tipoDocumento;
            Status = status;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmCategoriaDocumento>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
