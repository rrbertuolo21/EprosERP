using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MRP — Necessidade de material calculada pelo motor MRP (prd_mrp_necessidade — PD21/PD3).
    /// Netting: NecessidadeLiquida = max(0, Bruta + EstoqueSeguranca − Disponibilidade − RecebimentosProgramados).
    /// </summary>
    public class MrpNecessidade : EntidadeSaaSBase
    {
        public Guid PlanejamentoId { get; private set; }
        public Guid ItemId { get; private set; }
        public int Nivel { get; private set; }
        public DateTime DataReferencia { get; private set; }
        public decimal NecessidadeBruta { get; private set; }
        public decimal Disponibilidade { get; private set; }
        public decimal RecebimentosProgramados { get; private set; }
        public decimal EstoqueSeguranca { get; private set; }
        public decimal NecessidadeLiquida { get; private set; }

        protected MrpNecessidade() { } // EF Core

        public MrpNecessidade(
            Guid planejamentoId,
            Guid itemId,
            int nivel,
            DateTime dataReferencia,
            decimal necessidadeBruta,
            decimal disponibilidade,
            decimal recebimentosProgramados,
            decimal estoqueSeguranca,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanejamentoId = planejamentoId;
            ItemId = itemId;
            Nivel = nivel;
            DataReferencia = dataReferencia;
            NecessidadeBruta = necessidadeBruta;
            Disponibilidade = disponibilidade;
            RecebimentosProgramados = recebimentosProgramados;
            EstoqueSeguranca = estoqueSeguranca;
            NecessidadeLiquida = CalcularLiquida();
        }

        private decimal CalcularLiquida()
        {
            var liquida = NecessidadeBruta + EstoqueSeguranca - Disponibilidade - RecebimentosProgramados;
            return liquida < 0m ? 0m : liquida;
        }
    }
}
