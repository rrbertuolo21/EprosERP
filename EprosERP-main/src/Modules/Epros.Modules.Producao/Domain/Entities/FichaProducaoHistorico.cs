using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-GOS — Histórico da ficha de produção (prd_gos_ficha_historico).</summary>
    public class FichaProducaoHistorico : EntidadeSaaSBase
    {
        public Guid FichaProducaoId { get; private set; }
        public string Acao { get; private set; } = string.Empty;
        public string UsuarioId { get; private set; } = string.Empty;
        public DateTime DataHora { get; private set; }
        public string? Ip { get; private set; }
        public string? Conteudo { get; private set; }
        public string? Motivo { get; private set; }

        protected FichaProducaoHistorico() { } // EF Core

        public FichaProducaoHistorico(
            Guid fichaProducaoId,
            string acao,
            string usuarioId,
            string tenantId,
            string criadoPor,
            string? ip = null,
            string? conteudo = null,
            string? motivo = null)
            : base(tenantId, criadoPor)
        {
            FichaProducaoId = fichaProducaoId;
            Acao = acao;
            UsuarioId = usuarioId;
            DataHora = DateTime.UtcNow;
            Ip = ip;
            Conteudo = conteudo;
            Motivo = motivo;

            AddNotifications(new Contract<FichaProducaoHistorico>()
                .Requires()
                .AreNotEquals(fichaProducaoId, Guid.Empty, nameof(FichaProducaoId), "A ficha de produção é obrigatória [Origem: FichaProducaoHistorico].")
                .IsNotNullOrEmpty(acao, nameof(Acao), "A ação é obrigatória [Origem: FichaProducaoHistorico].")
                .IsNotNullOrEmpty(usuarioId, nameof(UsuarioId), "O usuário é obrigatório [Origem: FichaProducaoHistorico].")
            );
        }
    }
}
