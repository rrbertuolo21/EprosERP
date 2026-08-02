using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Projetos.Domain.Entities.Orcamento
{
    /// <summary>
    /// DP-ORC-002 — Baseline versionada = SNAPSHOT IMUTÁVEL do orçamento (budget + marcos) no momento do
    /// congelamento. Diferente do <see cref="OrcamentoProjeto.Versao"/> (que é apenas lock otimista): esta
    /// entidade congela os valores para comparação PV/EVM e não possui mutadores após a criação.
    /// Origem: RN-ORC-002 (baseline formal) + escopo máximo (DECISOES_IMPLANTACAO_V1 · seção 3).
    /// </summary>
    public class BaselineOrcamento : EntidadeSaaSBase
    {
        public Guid OrcamentoProjetoId { get; private set; }
        public Guid ProjetoId { get; private set; }
        /// <summary>Número sequencial da baseline dentro do orçamento (1, 2, 3...). Snapshot congelado.</summary>
        public int NumeroBaseline { get; private set; }
        /// <summary>Budget total congelado (BAC — Budget At Completion para EVM).</summary>
        public decimal BudgetSnapshot { get; private set; }
        /// <summary>Soma dos custos dos marcos no momento do congelamento.</summary>
        public decimal CustoMarcosTotal { get; private set; }
        /// <summary>Quantidade de marcos congelados.</summary>
        public int MarcosCount { get; private set; }
        /// <summary>Snapshot dos marcos (título/custo/datas/progresso) serializado em JSON — imutável.</summary>
        public string MarcosSnapshotJson { get; private set; } = "[]";
        public DateTime DataCongelamento { get; private set; }
        public string? Motivo { get; private set; }

        protected BaselineOrcamento() { } // EF Core

        public BaselineOrcamento(
            Guid orcamentoProjetoId,
            Guid projetoId,
            int numeroBaseline,
            decimal budgetSnapshot,
            decimal custoMarcosTotal,
            int marcosCount,
            string marcosSnapshotJson,
            string? motivo,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            OrcamentoProjetoId = orcamentoProjetoId;
            ProjetoId = projetoId;
            NumeroBaseline = numeroBaseline;
            BudgetSnapshot = budgetSnapshot;
            CustoMarcosTotal = custoMarcosTotal;
            MarcosCount = marcosCount;
            MarcosSnapshotJson = marcosSnapshotJson ?? "[]";
            Motivo = motivo;
            DataCongelamento = DateTime.UtcNow;
        }
    }
}
