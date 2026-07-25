using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Dados de combustível de um item de compra (ANP). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraItemCombustivel.
    /// </summary>
    public class CompraItemCombustivel : EntidadeSaaSBase
    {
        public Guid CompraItemId { get; private set; }
        public string? CodigoAnp { get; private set; }
        public string? DescricaoAnp { get; private set; }
        public decimal QuantidadeCombustivelFaturada { get; private set; }
        public EEstado UfConsumo { get; private set; }
        public decimal PercentualGlpDerivadoPetroleo { get; private set; }
        public decimal PercentualGasNaturalNacional { get; private set; }
        public decimal PercentualGasNaturalImportado { get; private set; }
        public decimal ValorPartida { get; private set; }

        // Navegação intra-módulo
        public ICollection<CompraItemCombustivelOrigem> Origens { get; private set; } = new List<CompraItemCombustivelOrigem>();
        public CompraItem? CompraItem { get; private set; }

        protected CompraItemCombustivel() { } // EF Core

        public CompraItemCombustivel(Guid compraItemId, string? codigoAnp, string? descricaoAnp, decimal quantidadeCombustivelFaturada, EEstado ufConsumo, decimal percentualGlpDerivadoPetroleo, decimal percentualGasNaturalNacional, decimal percentualGasNaturalImportado, decimal valorPartida, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraItemId = compraItemId;
            CodigoAnp = codigoAnp;
            DescricaoAnp = descricaoAnp;
            QuantidadeCombustivelFaturada = quantidadeCombustivelFaturada;
            UfConsumo = ufConsumo;
            PercentualGlpDerivadoPetroleo = percentualGlpDerivadoPetroleo;
            PercentualGasNaturalNacional = percentualGasNaturalNacional;
            PercentualGasNaturalImportado = percentualGasNaturalImportado;
            ValorPartida = valorPartida;
        }

        public void Alterar(string? codigoAnp, string? descricaoAnp, decimal quantidadeCombustivelFaturada, EEstado ufConsumo, decimal percentualGlpDerivadoPetroleo, decimal percentualGasNaturalNacional, decimal percentualGasNaturalImportado, decimal valorPartida, string usuario)
        {
            CodigoAnp = codigoAnp;
            DescricaoAnp = descricaoAnp;
            QuantidadeCombustivelFaturada = quantidadeCombustivelFaturada;
            UfConsumo = ufConsumo;
            PercentualGlpDerivadoPetroleo = percentualGlpDerivadoPetroleo;
            PercentualGasNaturalNacional = percentualGasNaturalNacional;
            PercentualGasNaturalImportado = percentualGasNaturalImportado;
            ValorPartida = valorPartida;
            MarcarAlterado(usuario);
        }

        public void AdicionarOrigens(int indicadorImportacao, EEstado ufOrigem, decimal percentualOrigem, string usuario)
        {
            Origens.Add(new CompraItemCombustivelOrigem(Id, indicadorImportacao, ufOrigem, percentualOrigem, TenantId, usuario));
        }

        public void AlterarOrigens(Guid compraItemCombustivelOrigemId, int indicadorImportacao, EEstado ufOrigem, decimal percentualOrigem, string usuario)
        {
            var origem = Origens.FirstOrDefault(o => o.Id == compraItemCombustivelOrigemId);
            origem?.Alterar(indicadorImportacao, ufOrigem, percentualOrigem, usuario);
        }

        public void DeletarOrigens(Guid compraItemCombustivelOrigemId, string usuario)
        {
            var origem = Origens.FirstOrDefault(o => o.Id == compraItemCombustivelOrigemId);
            origem?.Deletar(usuario);
        }
    }
}
