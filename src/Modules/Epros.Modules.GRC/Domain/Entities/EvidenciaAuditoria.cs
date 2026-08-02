using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GRC.Domain.Entities
{
    /// <summary>
    /// GRC-CIA — Evidência de auditoria (grc_cia_evidencia). D-CIA-02: prova de execução do teste
    /// e base para o encerramento de achado (RN-CIA-014 — evidência obrigatória no encerramento).
    /// Guarda referência ao arquivo (o binário vive no armazenamento/DMS), nunca o conteúdo.
    /// </summary>
    public class EvidenciaAuditoria : EntidadeSaaSBase
    {
        public Guid? AchadoId { get; private set; }
        public Guid? TesteControleId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? Descricao { get; private set; }

        protected EvidenciaAuditoria() { } // EF Core

        public EvidenciaAuditoria(
            Guid? achadoId,
            Guid? testeControleId,
            Guid arquivoId,
            string? descricao,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EvidenciaAuditoria>()
                .Requires()
                .IsTrue(arquivoId != Guid.Empty, nameof(ArquivoId), "O arquivo da evidencia e obrigatorio.")
                .IsTrue(achadoId != null || testeControleId != null, nameof(AchadoId),
                    "A evidencia deve estar vinculada a um achado ou teste de controle.")
            );

            AchadoId = achadoId;
            TesteControleId = testeControleId;
            ArquivoId = arquivoId;
            Descricao = descricao;
        }
    }
}
