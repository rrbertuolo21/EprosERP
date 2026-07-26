using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-BOM — Histórico/auditoria da estrutura (prd_bom_historico). BOM-REG-030.</summary>
    public class BomHistorico : EntidadeSaaSBase
    {
        public Guid EstruturaId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public EStatusWorkflowProducao? StatusAnterior { get; private set; }
        public EStatusWorkflowProducao? StatusNovo { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public string? OrigemIp { get; private set; }
        public string ConteudoJson { get; private set; } = "{}";
        public DateTime RegistradoEm { get; private set; }

        protected BomHistorico() { } // EF Core

        public BomHistorico(
            Guid estruturaId,
            string acao,
            string usuarioId,
            string conteudoJson,
            string tenantId,
            string criadoPor,
            EStatusWorkflowProducao? statusAnterior = null,
            EStatusWorkflowProducao? statusNovo = null,
            string? origemIp = null)
            : base(tenantId, criadoPor)
        {
            EstruturaId = estruturaId;
            Acao = acao;
            UsuarioId = usuarioId;
            ConteudoJson = string.IsNullOrWhiteSpace(conteudoJson) ? "{}" : conteudoJson;
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            OrigemIp = origemIp;
            RegistradoEm = DateTime.UtcNow;

            AddNotifications(new Contract<BomHistorico>()
                .Requires()
                .AreNotEquals(estruturaId, Guid.Empty, nameof(EstruturaId), "A estrutura é obrigatória [Origem: BomHistorico].")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação é obrigatória [Origem: BomHistorico].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: BomHistorico].")
            );
        }
    }
}
