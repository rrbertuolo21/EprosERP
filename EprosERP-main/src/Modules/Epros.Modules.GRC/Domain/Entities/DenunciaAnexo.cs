using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-DEN — Anexo (grc_den_anexo). Evidencia, documento ou arquivo vinculado ao caso ou
    /// resposta. Fiel a EF_13_GRC_INVESTIGACOES_E_DENUNCIAS_V1 (secao 11.6).
    /// RN-DEN-016: anexos de investigacao sao sigilosos por padrao.
    /// </summary>
    public class DenunciaAnexo : EntidadeSaaSBase
    {
        public Guid DenunciaId { get; private set; }
        public Guid? RespostaId { get; private set; }
        public Guid ArquivoId { get; private set; } // arquivo no GED
        public bool Sigiloso { get; private set; }
        public Guid? CriadoPorId { get; private set; }
        public DateTime DataCriacao { get; private set; }

        protected DenunciaAnexo() { } // EF Core

        public DenunciaAnexo(
            Guid denunciaId,
            Guid? respostaId,
            Guid arquivoId,
            bool sigiloso,
            Guid? criadoPorId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<DenunciaAnexo>()
                .Requires()
                .IsTrue(denunciaId != Guid.Empty, nameof(DenunciaId), "A denúncia do anexo é obrigatória.")
                .IsTrue(arquivoId != Guid.Empty, nameof(ArquivoId), "O arquivo do anexo é obrigatório.")
            );

            DenunciaId = denunciaId;
            RespostaId = respostaId;
            ArquivoId = arquivoId;
            Sigiloso = sigiloso;
            CriadoPorId = criadoPorId;
            DataCriacao = DateTime.UtcNow;
        }
    }
}
