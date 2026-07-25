using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-ESC — Histórico/auditoria da programação (prd_esc_historico). ESC-REG-014.</summary>
    public class EscHistorico : EntidadeSaaSBase
    {
        public Guid ProgramacaoId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public string UsuarioId { get; private set; } = string.Empty;
        public string PayloadJson { get; private set; } = "{}";
        public EStatusWorkflowProducao? StatusAnterior { get; private set; }
        public EStatusWorkflowProducao? StatusNovo { get; private set; }
        public string? IpOrigem { get; private set; }
        public DateTime DataEvento { get; private set; }

        protected EscHistorico() { } // EF Core

        public EscHistorico(
            Guid programacaoId,
            string acao,
            string usuarioId,
            string payloadJson,
            string tenantId,
            string criadoPor,
            EStatusWorkflowProducao? statusAnterior = null,
            EStatusWorkflowProducao? statusNovo = null,
            string? ipOrigem = null)
            : base(tenantId, criadoPor)
        {
            ProgramacaoId = programacaoId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson;
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            IpOrigem = ipOrigem;
            DataEvento = DateTime.UtcNow;

            AddNotifications(new Contract<EscHistorico>()
                .Requires()
                .AreNotEquals(programacaoId, Guid.Empty, nameof(ProgramacaoId), "A programação é obrigatória [Origem: EscHistorico].")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação é obrigatória [Origem: EscHistorico].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: EscHistorico].")
            );
        }
    }
}
