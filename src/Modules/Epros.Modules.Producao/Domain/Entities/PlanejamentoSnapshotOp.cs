using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-PLN — Snapshot planejado da OP (prd_pln_snapshot_op). Dados planejados do cabeçalho único da OP.
    /// </summary>
    public class PlanejamentoSnapshotOp : EntidadeSaaSBase
    {
        public Guid PlanejamentoId { get; private set; }
        public Guid? OrdemProducaoId { get; private set; }
        public DateTime? Inicio { get; private set; }
        public DateTime? PrevisaoEntrega { get; private set; }
        public DateTime? Termino { get; private set; }
        public decimal? PorcentoVenda { get; private set; }
        public decimal? PorcentoEstoque { get; private set; }
        public decimal? CustoTotalPrevisto { get; private set; }

        protected PlanejamentoSnapshotOp() { } // EF Core

        public PlanejamentoSnapshotOp(
            Guid planejamentoId,
            string tenantId,
            string criadoPor,
            Guid? ordemProducaoId = null,
            DateTime? inicio = null,
            DateTime? previsaoEntrega = null,
            DateTime? termino = null,
            decimal? porcentoVenda = null,
            decimal? porcentoEstoque = null,
            decimal? custoTotalPrevisto = null)
            : base(tenantId, criadoPor)
        {
            PlanejamentoId = planejamentoId;
            OrdemProducaoId = ordemProducaoId;
            Inicio = inicio;
            PrevisaoEntrega = previsaoEntrega;
            Termino = termino;
            PorcentoVenda = porcentoVenda;
            PorcentoEstoque = porcentoEstoque;
            CustoTotalPrevisto = custoTotalPrevisto;

            AddNotifications(new Contract<PlanejamentoSnapshotOp>()
                .Requires()
                .AreNotEquals(planejamentoId, Guid.Empty, nameof(PlanejamentoId), "O planejamento é obrigatório [Origem: PlanejamentoSnapshotOp].")
            );
        }
    }
}
