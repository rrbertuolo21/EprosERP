using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-MRP — Registro raiz do ciclo MRP/IBP (prd_mrp_planejamento).
    /// Fiel ao EF MRP_PLANEJAMENTO_INTEGRADO_IBP §11.1. Workflow: Rascunho → EmAnalise → Ativo → Inativo/Encerrado.
    /// Motor MRP (explosão BOM, netting, sugestões) é lacuna controlada — NÃO implementado (DP-MRP-002..006).
    /// </summary>
    public class MrpPlanejamento : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public EStatusWorkflowProducao Status { get; private set; } = EStatusWorkflowProducao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        protected MrpPlanejamento() { } // EF Core

        public MrpPlanejamento(string codigo, Guid responsavelId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            ResponsavelId = responsavelId;
            Status = EStatusWorkflowProducao.Rascunho;

            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<MrpPlanejamento>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código do planejamento é obrigatório [Origem: MrpPlanejamento]. (RN-MRP-002)")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsável é obrigatório [Origem: MrpPlanejamento]. (RN-MRP-004)")
            );
        }

        /// <summary>RN-MRP-007: apenas Rascunho pode ser submetido para análise.</summary>
        public void SubmeterParaAnalise(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas planejamento em Rascunho pode ser submetido para análise. (RN-MRP-007)");
                return;
            }
            Status = EStatusWorkflowProducao.EmAnalise;
            MarcarAlterado(alteradoPor);
        }

        public void Aprovar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas planejamento em análise pode ser aprovado. (RN-MRP-007)");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MotivoRejeicao = null;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>RN-MRP-007: rejeição retorna para Rascunho com motivo.</summary>
        public void Rejeitar(string motivo, string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas planejamento em análise pode ser rejeitado. (RN-MRP-007)");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "O motivo da rejeição é obrigatório. (RN-MRP-007)");
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
