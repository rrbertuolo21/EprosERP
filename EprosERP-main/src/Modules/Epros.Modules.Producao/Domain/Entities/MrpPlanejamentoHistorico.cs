using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-MRP — Histórico/auditoria do ciclo (prd_mrp_planejamento_historico). RN-MRP-008.</summary>
    public class MrpPlanejamentoHistorico : EntidadeSaaSBase
    {
        public Guid PlanejamentoId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public string UsuarioId { get; private set; } = string.Empty;
        public string PayloadJson { get; private set; } = "{}";
        public EStatusWorkflowProducao? StatusAnterior { get; private set; }
        public EStatusWorkflowProducao? StatusNovo { get; private set; }
        public string? Ip { get; private set; }
        public DateTime Timestamp { get; private set; }

        protected MrpPlanejamentoHistorico() { } // EF Core

        public MrpPlanejamentoHistorico(
            Guid planejamentoId,
            string acao,
            string usuarioId,
            string payloadJson,
            string tenantId,
            string criadoPor,
            EStatusWorkflowProducao? statusAnterior = null,
            EStatusWorkflowProducao? statusNovo = null,
            string? ip = null)
            : base(tenantId, criadoPor)
        {
            PlanejamentoId = planejamentoId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson;
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            Ip = ip;
            Timestamp = DateTime.UtcNow;

            AddNotifications(new Contract<MrpPlanejamentoHistorico>()
                .Requires()
                .AreNotEquals(planejamentoId, Guid.Empty, nameof(PlanejamentoId), "O planejamento é obrigatório [Origem: MrpPlanejamentoHistorico].")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação é obrigatória [Origem: MrpPlanejamentoHistorico].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: MrpPlanejamentoHistorico].")
            );
        }
    }
}
