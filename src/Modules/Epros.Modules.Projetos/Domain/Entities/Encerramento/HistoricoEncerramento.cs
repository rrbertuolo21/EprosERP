using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Encerramento
{
    /// <summary>
    /// Trilha de auditoria do encerramento. Origem: EF PRJ-ENC 11.3 (prj_enc_encerramento_historico).
    /// RN-ENC-014: eventos/historico apenas apos commit. Registro imutavel.
    /// </summary>
    public class HistoricoEncerramento : EntidadeSaaSBase
    {
        public Guid EncerramentoId { get; private set; }
        public EAcaoEncerramento Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string? PayloadJson { get; private set; }
        public string? Ip { get; private set; }
        public DateTime Timestamp { get; private set; }

        protected HistoricoEncerramento() { } // EF Core

        public HistoricoEncerramento(
            Guid encerramentoId,
            EAcaoEncerramento acao,
            Guid usuarioId,
            string? payloadJson,
            string? ip,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<HistoricoEncerramento>()
                .Requires()
                .AreNotEquals(encerramentoId, Guid.Empty, nameof(EncerramentoId), "O encerramento e obrigatorio. [Origem: HistoricoEncerramento]"));

            EncerramentoId = encerramentoId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = payloadJson;
            Ip = ip;
            Timestamp = DateTime.UtcNow;
        }
    }
}
