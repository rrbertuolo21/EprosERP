using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Cotação de câmbio por moeda e data (EF FIN-CAM §10.4 cam_taxa_cambio).</summary>
    public class TaxaCambio : EntidadeSaaSBase
    {
        public Guid MoedaId { get; private set; }
        public DateTime DataTaxa { get; private set; }
        public decimal TaxaCompra { get; private set; }
        public decimal TaxaVenda { get; private set; }
        public EOrigemTaxaCambio? OrigemTaxa { get; private set; }
        public string? Observacao { get; private set; }

        protected TaxaCambio() { } // EF Core

        public TaxaCambio(Guid moedaId, DateTime dataTaxa, decimal taxaCompra, decimal taxaVenda,
            EOrigemTaxaCambio? origemTaxa, string? observacao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            MoedaId = moedaId;
            DataTaxa = dataTaxa;
            TaxaCompra = taxaCompra;
            TaxaVenda = taxaVenda;
            OrigemTaxa = origemTaxa;
            Observacao = observacao;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<TaxaCambio>()
                .Requires()
                .IsNotEmpty(MoedaId, nameof(MoedaId), "A moeda é obrigatória [Origem: TaxaCambio]")
                .IsGreaterThan(TaxaCompra, 0, nameof(TaxaCompra), "A taxa de compra deve ser maior que zero [Origem: TaxaCambio]")
                .IsGreaterThan(TaxaVenda, 0, nameof(TaxaVenda), "A taxa de venda deve ser maior que zero [Origem: TaxaCambio]")
            );
        }
    }
}
