using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Portfolio
{
    /// <summary>
    /// Trilha de auditoria do portfolio. Origem: EF PRJ-PRT 14.3 (prj_portfolio_historico).
    /// PRJ-PRT-RN-012: alteracoes relevantes geram auditoria com usuario/data/antes-depois. Registro imutavel.
    /// </summary>
    public class HistoricoPortfolio : EntidadeSaaSBase
    {
        public Guid PortfolioId { get; private set; }
        public Guid? ItemId { get; private set; }
        public EAcaoPortfolio Acao { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string? PayloadJson { get; private set; }
        public string? Ip { get; private set; }
        public string? Motivo { get; private set; }
        public DateTime OcorridoEm { get; private set; }

        protected HistoricoPortfolio() { } // EF Core

        public HistoricoPortfolio(
            Guid portfolioId,
            Guid? itemId,
            EAcaoPortfolio acao,
            Guid usuarioId,
            string? payloadJson,
            string? motivo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<HistoricoPortfolio>()
                .Requires()
                .AreNotEquals(portfolioId, Guid.Empty, nameof(PortfolioId), "O portfolio e obrigatorio. [Origem: HistoricoPortfolio]"));

            PortfolioId = portfolioId;
            ItemId = itemId;
            Acao = acao;
            UsuarioId = usuarioId;
            PayloadJson = payloadJson;
            Motivo = motivo;
            OcorridoEm = DateTime.UtcNow;
        }
    }
}
