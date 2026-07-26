using System;
using System.Collections.Generic;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-PLN — Agregado raiz do planejamento de produção (prd_pln_planejamento).
    /// Fiel ao EF PLANEJAMENTO_DE_PRODUCAO §11/§12. Workflow: Rascunho → EmAnalise → Ativo → Inativo/Encerrado.
    /// </summary>
    public class PlanejamentoProducao : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public EStatusWorkflowProducao Status { get; private set; } = EStatusWorkflowProducao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        public List<PlanejamentoSnapshotOp> Snapshots { get; private set; } = new();

        protected PlanejamentoProducao() { } // EF Core

        public PlanejamentoProducao(string codigo, Guid responsavelId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            ResponsavelId = responsavelId;
            Status = EStatusWorkflowProducao.Rascunho;

            AddNotifications(new Contract<PlanejamentoProducao>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do planejamento é obrigatório [Origem: PlanejamentoProducao].")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsável é obrigatório [Origem: PlanejamentoProducao].")
            );
        }

        public void AdicionarSnapshot(
            string alteradoPor,
            Guid? ordemProducaoId = null,
            DateTime? inicio = null,
            DateTime? previsaoEntrega = null,
            DateTime? termino = null,
            decimal? porcentoVenda = null,
            decimal? porcentoEstoque = null,
            decimal? custoTotalPrevisto = null)
        {
            var snapshot = new PlanejamentoSnapshotOp(
                Id, TenantId, alteradoPor, ordemProducaoId, inicio, previsaoEntrega,
                termino, porcentoVenda, porcentoEstoque, custoTotalPrevisto);

            if (!snapshot.IsValid)
            {
                AddNotifications(snapshot.Notifications);
                return;
            }
            Snapshots.Add(snapshot);
        }

        public void SubmeterParaAnalise(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas planejamento em Rascunho pode ser submetido.");
                return;
            }
            Status = EStatusWorkflowProducao.EmAnalise;
            MarcarAlterado(alteradoPor);
        }

        public void Aprovar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas planejamento em análise pode ser aprovado.");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MotivoRejeicao = null;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(string motivo, string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas planejamento em análise pode ser rejeitado.");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "O motivo da rejeição é obrigatório.");
                return;
            }
            Status = EStatusWorkflowProducao.Rascunho;
            MotivoRejeicao = motivo;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo)
            {
                AddNotification(nameof(Status), "Apenas planejamento ativo pode ser inativado.");
                return;
            }
            Status = EStatusWorkflowProducao.Inativo;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Apenas planejamento inativo pode ser reativado.");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MarcarAlterado(alteradoPor);
        }

        public void Encerrar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo && Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Somente planejamento ativo ou inativo pode ser encerrado.");
                return;
            }
            Status = EStatusWorkflowProducao.Encerrado;
            MarcarAlterado(alteradoPor);
        }
    }
}
