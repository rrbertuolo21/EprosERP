using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.RH.Domain.Entities
{
    /// <summary>Entidade portada da EF (submodulo RH-SSO). Fidelidade campo a campo.</summary>
    public partial class SsoDocumentoPrograma : EntidadeSaaSBase
    {
        public string TipoDocumento { get; private set; } = string.Empty;
        public string Titulo { get; private set; } = string.Empty;
        public DateTime? DataInicioVigencia { get; private set; }
        public DateTime? DataFimVigencia { get; private set; }
        public Guid? DocumentoId { get; private set; }
        public string Status { get; private set; } = string.Empty;

        protected SsoDocumentoPrograma() { } // EF Core

        public SsoDocumentoPrograma(
            string tipoDocumento,
            string titulo,
            DateTime? dataInicioVigencia,
            DateTime? dataFimVigencia,
            Guid? documentoId,
            string status,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            TipoDocumento = tipoDocumento;
            Titulo = titulo;
            DataInicioVigencia = dataInicioVigencia;
            DataFimVigencia = dataFimVigencia;
            DocumentoId = documentoId;
            Status = status;
            Validar();
        }

        public void Validar()
        {
            var contract = new Contract<SsoDocumentoPrograma>().Requires();
            contract.IsNotNullOrEmpty(TipoDocumento, nameof(TipoDocumento), "O campo TipoDocumento e obrigatorio.");
            contract.IsNotNullOrEmpty(Titulo, nameof(Titulo), "O campo Titulo e obrigatorio.");
            contract.IsNotNullOrEmpty(Status, nameof(Status), "O campo Status e obrigatorio.");
            AddNotifications(contract);
        }

        public void MarcarAtualizado(string usuario) => MarcarAlterado(usuario);
    }
}
