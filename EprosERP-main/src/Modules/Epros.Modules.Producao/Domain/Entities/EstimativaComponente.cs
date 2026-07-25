using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>PRD-EST — Componente técnico, tempos, taxas e custos simulados (prd_est_componente).</summary>
    public class EstimativaComponente : EntidadeSaaSBase
    {
        public Guid EstimativaId { get; private set; }
        public string TipoComponente { get; private set; } = string.Empty;
        public Guid? ReferenciaId { get; private set; }
        public decimal? Quantidade { get; private set; }
        public decimal? TempoEstimado { get; private set; }
        public decimal? TaxaEstimada { get; private set; }
        public decimal? CustoPrevisto { get; private set; }
        public string? Observacao { get; private set; }

        protected EstimativaComponente() { } // EF Core

        public EstimativaComponente(
            Guid estimativaId,
            string tipoComponente,
            string tenantId,
            string criadoPor,
            Guid? referenciaId = null,
            decimal? quantidade = null,
            decimal? tempoEstimado = null,
            decimal? taxaEstimada = null,
            decimal? custoPrevisto = null,
            string? observacao = null)
            : base(tenantId, criadoPor)
        {
            EstimativaId = estimativaId;
            TipoComponente = tipoComponente;
            ReferenciaId = referenciaId;
            Quantidade = quantidade;
            TempoEstimado = tempoEstimado;
            TaxaEstimada = taxaEstimada;
            CustoPrevisto = custoPrevisto;
            Observacao = observacao;

            AddNotifications(new Contract<EstimativaComponente>()
                .Requires()
                .AreNotEquals(estimativaId, Guid.Empty, nameof(EstimativaId), "A estimativa é obrigatória [Origem: EstimativaComponente].")
                .IsNotNullOrEmpty(tipoComponente, nameof(TipoComponente), "O tipo do componente é obrigatório [Origem: EstimativaComponente].")
            );
        }
    }
}
