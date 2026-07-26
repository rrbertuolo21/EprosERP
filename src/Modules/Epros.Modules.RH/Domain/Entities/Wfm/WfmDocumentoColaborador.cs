using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-WFM, tabela rh_wfm_documento_colaborador). Fidelidade campo a campo.</summary>
    public partial class WfmDocumentoColaborador : EntidadeSaaSBase
    {
        public Guid? ColaboradorId { get; private set; }
        public Guid? TipoDocumentoId { get; private set; }
        public string? ArquivoReferencia { get; private set; }
        public DateTime? DataEnvio { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public Guid? OwnerId { get; private set; }

        protected WfmDocumentoColaborador() { } // EF Core

        public WfmDocumentoColaborador(
            Guid? colaboradorId,
            Guid? tipoDocumentoId,
            string? arquivoReferencia,
            DateTime? dataEnvio,
            Guid? criadoPorId,
            Guid? ownerId,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            TipoDocumentoId = tipoDocumentoId;
            ArquivoReferencia = arquivoReferencia;
            DataEnvio = dataEnvio;
            CriadoPorId = criadoPorId;
            OwnerId = ownerId;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<WfmDocumentoColaborador>().Requires();
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
