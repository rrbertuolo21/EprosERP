using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-MES — Histórico/auditoria da execução (prd_mes_historico). MES-REG-030.</summary>
    public class MesHistorico : EntidadeSaaSBase
    {
        public Guid OrdemId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public EStatusOrdemMes? StatusAnterior { get; private set; }
        public EStatusOrdemMes? StatusNovo { get; private set; }
        public string UsuarioId { get; private set; } = string.Empty;
        public string? OrigemIp { get; private set; }
        public string ConteudoJson { get; private set; } = "{}";
        public DateTime RegistradoEm { get; private set; }

        protected MesHistorico() { } // EF Core

        public MesHistorico(
            Guid ordemId,
            string acao,
            string usuarioId,
            string conteudoJson,
            string tenantId,
            string criadoPor,
            EStatusOrdemMes? statusAnterior = null,
            EStatusOrdemMes? statusNovo = null,
            string? origemIp = null)
            : base(tenantId, criadoPor)
        {
            OrdemId = ordemId;
            Acao = acao;
            UsuarioId = usuarioId;
            ConteudoJson = string.IsNullOrWhiteSpace(conteudoJson) ? "{}" : conteudoJson;
            StatusAnterior = statusAnterior;
            StatusNovo = statusNovo;
            OrigemIp = origemIp;
            RegistradoEm = DateTime.UtcNow;

            AddNotifications(new Contract<MesHistorico>()
                .Requires()
                .AreNotEquals(ordemId, Guid.Empty, nameof(OrdemId), "A ordem é obrigatória [Origem: MesHistorico].")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação é obrigatória [Origem: MesHistorico].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: MesHistorico].")
            );
        }
    }
}
