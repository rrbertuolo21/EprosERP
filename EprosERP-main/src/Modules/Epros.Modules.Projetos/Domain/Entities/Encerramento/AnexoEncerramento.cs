using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Encerramento
{
    /// <summary>
    /// Anexo formal do encerramento (documento GED). Origem: EF PRJ-ENC 11.4 (prj_enc_encerramento_anexo).
    /// ArquivoId obrigatorio (referencia ao GED).
    /// </summary>
    public class AnexoEncerramento : EntidadeSaaSBase
    {
        public Guid EncerramentoId { get; private set; }
        public Guid ArquivoId { get; private set; }

        protected AnexoEncerramento() { } // EF Core

        public AnexoEncerramento(
            Guid encerramentoId,
            Guid arquivoId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AnexoEncerramento>()
                .Requires()
                .AreNotEquals(encerramentoId, Guid.Empty, nameof(EncerramentoId), "O encerramento e obrigatorio. [Origem: AnexoEncerramento]")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo (GED) e obrigatorio. [Origem: AnexoEncerramento]"));

            EncerramentoId = encerramentoId;
            ArquivoId = arquivoId;
        }
    }
}
