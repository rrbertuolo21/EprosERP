using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaItemCombustivel. FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaItemCombustivel : EntidadeSaaSBase
    {
        public Guid VendaItemId { get; private set; }
        public string? CodigoAnp { get; private set; }
        public string? DescricaoAnp { get; private set; }
        public decimal QuantidadeCombustivelFaturada { get; private set; }
        public EEstado UfConsumo { get; private set; }
        public decimal PercentualGlpDerivadoPetroleo { get; private set; }
        public decimal PercentualGasNaturalNacional { get; private set; }
        public decimal PercentualGasNaturalImportado { get; private set; }
        public decimal ValorPartida { get; private set; }

        private readonly List<VendaItemCombustivelOrigem> _origens = new();
        public IReadOnlyCollection<VendaItemCombustivelOrigem> Origens => _origens.AsReadOnly();

        // Navegação intra-módulo
        public VendaItem VendaItem { get; private set; } = null!;

        protected VendaItemCombustivel() { } // EF Core

        public VendaItemCombustivel(Guid vendaItemId, string? codigoAnp, string? descricaoAnp, decimal quantidadeCombustivelFaturada, EEstado ufConsumo, decimal percentualGlpDerivadoPetroleo, decimal percentualGasNaturalNacional, decimal percentualGasNaturalImportado, decimal valorPartida,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaItemId = vendaItemId;
            CodigoAnp = codigoAnp;
            DescricaoAnp = descricaoAnp;
            QuantidadeCombustivelFaturada = quantidadeCombustivelFaturada;
            UfConsumo = ufConsumo;
            PercentualGlpDerivadoPetroleo = percentualGlpDerivadoPetroleo;
            PercentualGasNaturalNacional = percentualGasNaturalNacional;
            PercentualGasNaturalImportado = percentualGasNaturalImportado;
            ValorPartida = valorPartida;
        }

        public void Alterar(string? codigoAnp, string? descricaoAnp, decimal quantidadeCombustivelFaturada, EEstado ufConsumo, decimal percentualGlpDerivadoPetroleo, decimal percentualGasNaturalNacional, decimal percentualGasNaturalImportado, decimal valorPartida, string alteradoPor)
        {
            CodigoAnp = codigoAnp;
            DescricaoAnp = descricaoAnp;
            QuantidadeCombustivelFaturada = quantidadeCombustivelFaturada;
            UfConsumo = ufConsumo;
            PercentualGlpDerivadoPetroleo = percentualGlpDerivadoPetroleo;
            PercentualGasNaturalNacional = percentualGasNaturalNacional;
            PercentualGasNaturalImportado = percentualGasNaturalImportado;
            ValorPartida = valorPartida;
            MarcarAlterado(alteradoPor);
        }

        public void AdicionarOrigens(int indicadorImportacao, EEstado ufOrigem, decimal percentualOrigem, string tenantId, string criadoPor)
        {
            _origens.Add(new VendaItemCombustivelOrigem(Id, indicadorImportacao, ufOrigem, percentualOrigem, tenantId, criadoPor));
        }

        public void AlterarOrigens(Guid vendaItemCombustivelOrigemId, int indicadorImportacao, EEstado ufOrigem, decimal percentualOrigem, string alteradoPor)
        {
            var origem = _origens.FirstOrDefault(o => o.Id == vendaItemCombustivelOrigemId);
            origem?.Alterar(Id, indicadorImportacao, ufOrigem, percentualOrigem, alteradoPor);
        }

        public void DeletarOrigens(Guid vendaItemCombustivelOrigemId, string userId)
        {
            var origem = _origens.FirstOrDefault(o => o.Id == vendaItemCombustivelOrigemId);
            origem?.Deletar(userId);
        }

        /// <summary>Porte fiel de VendaItemCombustivel.Duplicar (novo Id/FK, incl. origens).</summary>
        public VendaItemCombustivel Duplicar(Guid novoItemId, string criadoPor)
        {
            var clone = new VendaItemCombustivel(novoItemId, CodigoAnp, DescricaoAnp, QuantidadeCombustivelFaturada, UfConsumo,
                PercentualGlpDerivadoPetroleo, PercentualGasNaturalNacional, PercentualGasNaturalImportado, ValorPartida, TenantId, criadoPor);
            foreach (var origem in _origens)
                clone._origens.Add(origem.Duplicar(clone.Id, criadoPor));
            return clone;
        }
    }
}
