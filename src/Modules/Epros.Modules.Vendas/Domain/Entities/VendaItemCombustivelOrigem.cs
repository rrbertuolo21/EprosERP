using System;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaItemCombustivelOrigem. FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaItemCombustivelOrigem : EntidadeSaaSBase
    {
        public Guid VendaItemCombustivelId { get; private set; }
        public int IndicadorImportacao { get; private set; }
        public EEstado UfOrigem { get; private set; }
        public decimal PercentualOrigem { get; private set; }

        // Navegação intra-módulo
        public VendaItemCombustivel VendaItemCombustivel { get; private set; } = null!;

        protected VendaItemCombustivelOrigem() { } // EF Core

        public VendaItemCombustivelOrigem(Guid vendaItemCombustivelId, int indicadorImportacao, EEstado ufOrigem, decimal percentualOrigem,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaItemCombustivelId = vendaItemCombustivelId;
            IndicadorImportacao = indicadorImportacao;
            UfOrigem = ufOrigem;
            PercentualOrigem = percentualOrigem;
        }

        public void Alterar(Guid vendaItemCombustivelId, int indicadorImportacao, EEstado ufOrigem, decimal percentualOrigem, string alteradoPor)
        {
            VendaItemCombustivelId = vendaItemCombustivelId;
            IndicadorImportacao = indicadorImportacao;
            UfOrigem = ufOrigem;
            PercentualOrigem = percentualOrigem;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaItemCombustivelOrigem.Duplicar (novo Id/FK).</summary>
        public VendaItemCombustivelOrigem Duplicar(Guid novoCombustivelId, string criadoPor)
            => new(novoCombustivelId, IndicadorImportacao, UfOrigem, PercentualOrigem, TenantId, criadoPor);
    }
}
