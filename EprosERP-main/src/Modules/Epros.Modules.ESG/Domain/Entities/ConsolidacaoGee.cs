using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.ESG.Domain.Entities
{
    /// <summary>Totais dimensionais por inventario, evitando dupla contagem (EF PEGADA_DE_CARBONO 11.2 Consolidacao).</summary>
    public class ConsolidacaoGee : EntidadeSaaSBase
    {
        public Guid InventarioId { get; private set; }
        public string Dimensao { get; private set; } = string.Empty; // escopo/categoria/unidade
        public decimal TotalCO2e { get; private set; }
        public DateTime GeradoEm { get; private set; }

        protected ConsolidacaoGee() { } // EF Core

        public ConsolidacaoGee(
            Guid inventarioId,
            string dimensao,
            decimal totalCO2e,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            InventarioId = inventarioId;
            Dimensao = dimensao;
            TotalCO2e = totalCO2e;
            GeradoEm = DateTime.UtcNow;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ConsolidacaoGee>()
                .Requires()
                .AreNotEquals(InventarioId, Guid.Empty, nameof(InventarioId), "O inventario e obrigatorio. [Origem: ConsolidacaoGee]")
                .IsNotNullOrEmpty(Dimensao, nameof(Dimensao), "A dimensao e obrigatoria. [Origem: ConsolidacaoGee]"));
        }
    }
}
