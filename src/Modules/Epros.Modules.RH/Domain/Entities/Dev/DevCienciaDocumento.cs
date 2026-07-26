using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-DEV, tabela rh_dev_ciencia_documento). Fidelidade campo a campo.</summary>
    public partial class DevCienciaDocumento : EntidadeSaaSBase
    {
        public Guid ColaboradorId { get; private set; }
        public Guid DocumentoId { get; private set; }
        public string? Status { get; private set; }
        public string? Observacao { get; private set; }
        public DateTime? ReconhecidoEm { get; private set; }
        public Guid? AtribuidoPor { get; private set; }

        protected DevCienciaDocumento() { } // EF Core

        public DevCienciaDocumento(
            Guid colaboradorId,
            Guid documentoId,
            string? status,
            string? observacao,
            DateTime? reconhecidoEm,
            Guid? atribuidoPor,
            string tenantId,
            string criadoPor
            )
            : base(tenantId, criadoPor)
        {
            ColaboradorId = colaboradorId;
            DocumentoId = documentoId;
            Status = status;
            Observacao = observacao;
            ReconhecidoEm = reconhecidoEm;
            AtribuidoPor = atribuidoPor;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<DevCienciaDocumento>().Requires();
            contract.AreNotEquals(ColaboradorId, Guid.Empty, nameof(ColaboradorId), "O campo ColaboradorId e obrigatorio.");
            contract.AreNotEquals(DocumentoId, Guid.Empty, nameof(DocumentoId), "O campo DocumentoId e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
