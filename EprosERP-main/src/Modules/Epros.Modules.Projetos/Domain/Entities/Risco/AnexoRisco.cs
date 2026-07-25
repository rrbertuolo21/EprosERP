using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Risco
{
    /// <summary>
    /// Evidencia/anexo do risco/issue (documento GED). Origem: EF PRJ-RSK 11.5 (prj_risco_anexo).
    /// ArquivoId obrigatorio; TipoDocumento opcional (evidencia, plano, laudo, imagem, outro).
    /// </summary>
    public class AnexoRisco : EntidadeSaaSBase
    {
        public Guid RiscoId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoDocumento { get; private set; }

        protected AnexoRisco() { } // EF Core

        public AnexoRisco(
            Guid riscoId,
            Guid arquivoId,
            string? tipoDocumento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AnexoRisco>()
                .Requires()
                .AreNotEquals(riscoId, Guid.Empty, nameof(RiscoId), "O risco e obrigatorio. [Origem: AnexoRisco]")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo (GED) e obrigatorio. [Origem: AnexoRisco]"));

            RiscoId = riscoId;
            ArquivoId = arquivoId;
            TipoDocumento = tipoDocumento;
        }
    }
}
