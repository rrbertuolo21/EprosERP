using System;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Origem (UF) do combustível de um item de compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraItemCombustivelOrigem.
    /// </summary>
    public class CompraItemCombustivelOrigem : EntidadeSaaSBase
    {
        public Guid CompraItemCombustivelId { get; private set; }
        public int IndicadorImportacao { get; private set; }
        public EEstado UfOrigem { get; private set; }
        public decimal PercentualOrigem { get; private set; }

        // Navegação intra-módulo
        public CompraItemCombustivel? CompraItemCombustivel { get; private set; }

        protected CompraItemCombustivelOrigem() { } // EF Core

        public CompraItemCombustivelOrigem(Guid compraItemCombustivelId, int indicadorImportacao, EEstado ufOrigem, decimal percentualOrigem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraItemCombustivelId = compraItemCombustivelId;
            IndicadorImportacao = indicadorImportacao;
            UfOrigem = ufOrigem;
            PercentualOrigem = percentualOrigem;
        }

        public void Alterar(int indicadorImportacao, EEstado ufOrigem, decimal percentualOrigem, string usuario)
        {
            IndicadorImportacao = indicadorImportacao;
            UfOrigem = ufOrigem;
            PercentualOrigem = percentualOrigem;
            MarcarAlterado(usuario);
        }
    }
}
