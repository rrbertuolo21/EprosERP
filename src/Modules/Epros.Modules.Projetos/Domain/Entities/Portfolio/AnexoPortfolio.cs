using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Portfolio
{
    /// <summary>
    /// Anexo do portfolio ou item (documento GED). Origem: EF PRJ-PRT 14.4 (prj_portfolio_anexo).
    /// PRJ-PRT-RN-023: anexos vinculados por documento formal (ArquivoId obrigatorio).
    /// </summary>
    public class AnexoPortfolio : EntidadeSaaSBase
    {
        public Guid PortfolioId { get; private set; }
        public Guid? ItemId { get; private set; }
        public Guid ArquivoId { get; private set; }
        public string? TipoAnexo { get; private set; }

        protected AnexoPortfolio() { } // EF Core

        public AnexoPortfolio(
            Guid portfolioId,
            Guid? itemId,
            Guid arquivoId,
            string? tipoAnexo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<AnexoPortfolio>()
                .Requires()
                .AreNotEquals(portfolioId, Guid.Empty, nameof(PortfolioId), "O portfolio e obrigatorio. [Origem: AnexoPortfolio]")
                .AreNotEquals(arquivoId, Guid.Empty, nameof(ArquivoId), "O arquivo (GED) e obrigatorio. [Origem: AnexoPortfolio]"));

            PortfolioId = portfolioId;
            ItemId = itemId;
            ArquivoId = arquivoId;
            TipoAnexo = tipoAnexo;
        }
    }
}
