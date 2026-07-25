using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-PLN — Histórico/auditoria do planejamento (prd_pln_planejamento_historico).</summary>
    public class PlanejamentoHistorico : EntidadeSaaSBase
    {
        public Guid PlanejamentoId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public string UsuarioId { get; private set; } = string.Empty;
        public string PayloadJson { get; private set; } = "{}";
        public string? Ip { get; private set; }
        public DateTime Timestamp { get; private set; }

        protected PlanejamentoHistorico() { } // EF Core

        public PlanejamentoHistorico(Guid planejamentoId, string acao, string usuarioId, string payloadJson, string tenantId, string criadoPor, string? ip = null)
            : base(tenantId, criadoPor)
        {
            PlanejamentoId = planejamentoId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson;
            Ip = ip;
            Timestamp = DateTime.UtcNow;

            AddNotifications(new Contract<PlanejamentoHistorico>()
                .Requires()
                .AreNotEquals(planejamentoId, Guid.Empty, nameof(PlanejamentoId), "O planejamento é obrigatório [Origem: PlanejamentoHistorico].")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação é obrigatória [Origem: PlanejamentoHistorico].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: PlanejamentoHistorico].")
            );
        }
    }
}
