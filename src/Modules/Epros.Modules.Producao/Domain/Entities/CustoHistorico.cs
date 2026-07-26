using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-CST — Histórico/auditoria de custos (prd_cst_historico).</summary>
    public class CustoHistorico : EntidadeSaaSBase
    {
        public Guid CustoProducaoId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public string UsuarioId { get; private set; } = string.Empty;
        public string PayloadJson { get; private set; } = "{}";
        public string? IpOrigem { get; private set; }
        public DateTime DataEvento { get; private set; }

        protected CustoHistorico() { } // EF Core

        public CustoHistorico(Guid custoProducaoId, string acao, string usuarioId, string payloadJson, string tenantId, string criadoPor, string? ipOrigem = null)
            : base(tenantId, criadoPor)
        {
            CustoProducaoId = custoProducaoId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson;
            IpOrigem = ipOrigem;
            DataEvento = DateTime.UtcNow;

            AddNotifications(new Contract<CustoHistorico>()
                .Requires()
                .AreNotEquals(custoProducaoId, Guid.Empty, nameof(CustoProducaoId), "O custo de produção é obrigatório [Origem: CustoHistorico].")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação é obrigatória [Origem: CustoHistorico].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: CustoHistorico].")
            );
        }
    }
}
