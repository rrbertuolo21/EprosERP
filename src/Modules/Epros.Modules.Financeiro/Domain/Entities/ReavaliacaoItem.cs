using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Item de reavaliação cambial de um título (EF FIN-CAM §10.7 cam_reavaliacao_item).</summary>
    public class ReavaliacaoItem : EntidadeSaaSBase
    {
        public Guid ReavaliacaoId { get; private set; }
        public Guid MoedaId { get; private set; }
        public string? TituloTipo { get; private set; }
        public Guid? TituloId { get; private set; }
        public Guid TaxaCambioId { get; private set; }
        public decimal ValorOriginalMoeda { get; private set; }
        public decimal ValorReavaliadoBase { get; private set; }
        public decimal ValorVariacao { get; private set; }
        public bool Contabilizado { get; private set; }

        public ReavaliacaoTitulo Reavaliacao { get; private set; } = null!;

        protected ReavaliacaoItem() { } // EF Core

        public ReavaliacaoItem(Guid reavaliacaoId, Guid moedaId, string? tituloTipo, Guid? tituloId, Guid taxaCambioId,
            decimal valorOriginalMoeda, decimal valorReavaliadoBase, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ReavaliacaoId = reavaliacaoId;
            MoedaId = moedaId;
            TituloTipo = tituloTipo;
            TituloId = tituloId;
            TaxaCambioId = taxaCambioId;
            ValorOriginalMoeda = valorOriginalMoeda;
            ValorReavaliadoBase = valorReavaliadoBase;
            ValorVariacao = valorReavaliadoBase - valorOriginalMoeda;
            Contabilizado = false;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ReavaliacaoItem>()
                .Requires()
                .IsNotEmpty(MoedaId, nameof(MoedaId), "A moeda do item é obrigatória [Origem: ReavaliacaoItem]")
                .IsNotEmpty(TaxaCambioId, nameof(TaxaCambioId), "A taxa de câmbio aplicada é obrigatória [Origem: ReavaliacaoItem]")
            );
        }
    }
}
