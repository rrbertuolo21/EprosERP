using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-EST — Histórico/auditoria da estimativa (prd_est_historico).</summary>
    public class EstimativaHistorico : EntidadeSaaSBase
    {
        public Guid EstimativaId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public string UsuarioId { get; private set; } = string.Empty;
        public string PayloadJson { get; private set; } = "{}";
        public string? IpOrigem { get; private set; }
        public DateTime DataEvento { get; private set; }

        protected EstimativaHistorico() { } // EF Core

        public EstimativaHistorico(Guid estimativaId, string acao, string usuarioId, string payloadJson, string tenantId, string criadoPor, string? ipOrigem = null)
            : base(tenantId, criadoPor)
        {
            EstimativaId = estimativaId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson;
            IpOrigem = ipOrigem;
            DataEvento = DateTime.UtcNow;

            AddNotifications(new Contract<EstimativaHistorico>()
                .Requires()
                .AreNotEquals(estimativaId, Guid.Empty, nameof(EstimativaId), "A estimativa é obrigatória [Origem: EstimativaHistorico].")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação é obrigatória [Origem: EstimativaHistorico].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: EstimativaHistorico].")
            );
        }
    }
}
