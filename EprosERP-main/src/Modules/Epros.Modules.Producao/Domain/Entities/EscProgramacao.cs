using System;
using System.Collections.Generic;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-ESC — Registro principal de escalonamento e programação (prd_esc_programacao).
    /// Fiel ao EF ESCALONAMENTO_PROGRAMACAO §17. Workflow: Rascunho → EmAnalise → Ativo → Inativo/Encerrado.
    /// Motor de sequenciamento/capacidade finita é lacuna controlada — NÃO implementado.
    /// </summary>
    public class EscProgramacao : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public EStatusWorkflowProducao Status { get; private set; } = EStatusWorkflowProducao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public Guid? PlanoProducaoId { get; private set; }
        public Guid? OrdemProducaoId { get; private set; }
        public Guid? CentroTrabalhoId { get; private set; }
        public int? Prioridade { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        public List<EscOperacao> Operacoes { get; private set; } = new();

        protected EscProgramacao() { } // EF Core

        public EscProgramacao(
            string codigo,
            Guid responsavelId,
            string tenantId,
            string criadoPor,
            Guid? planoProducaoId = null,
            Guid? ordemProducaoId = null,
            Guid? centroTrabalhoId = null,
            int? prioridade = null)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            ResponsavelId = responsavelId;
            Status = EStatusWorkflowProducao.Rascunho;
            PlanoProducaoId = planoProducaoId;
            OrdemProducaoId = ordemProducaoId;
            CentroTrabalhoId = centroTrabalhoId;
            Prioridade = prioridade;

            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<EscProgramacao>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código da programação é obrigatório [Origem: EscProgramacao]. (ESC-REG-002)")
                .AreNotEquals(ResponsavelId, Guid.Empty, nameof(ResponsavelId), "O responsável é obrigatório [Origem: EscProgramacao]. (ESC-REG-004)")
            );
        }

        /// <summary>ESC-REG-016..020: adiciona operação sequenciada com janelas, durações e recursos.</summary>
        public void AdicionarOperacao(EscOperacao operacao, string alteradoPor)
        {
            if (operacao == null) return;
            if (!operacao.IsValid)
            {
                AddNotifications(operacao.Notifications);
                return;
            }
            Operacoes.Add(operacao);
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ESC-REG-006/007: apenas Rascunho pode ser submetido para análise.</summary>
        public void SubmeterParaAnalise(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas programação em Rascunho pode ser submetida para análise. (ESC-REG-006)");
                return;
            }
            Status = EStatusWorkflowProducao.EmAnalise;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ESC-REG-008/009: programação em análise pode ser aprovada.</summary>
        public void Aprovar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas programação em análise pode ser aprovada. (ESC-REG-008)");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MotivoRejeicao = null;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ESC-REG-010: rejeição retorna para Rascunho com motivo.</summary>
        public void Rejeitar(string motivo, string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas programação em análise pode ser rejeitada. (ESC-REG-008)");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "O motivo da rejeição é obrigatório. (ESC-REG-010)");
                return;
            }
            Status = EStatusWorkflowProducao.Rascunho;
            MotivoRejeicao = motivo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ESC-REG-011: programação ativa pode ser inativada por gestor.</summary>
        public void Inativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo)
            {
                AddNotification(nameof(Status), "Apenas programação ativa pode ser inativada. (ESC-REG-011)");
                return;
            }
            Status = EStatusWorkflowProducao.Inativo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ESC-REG-013: programação inativa pode ser reativada por gestor.</summary>
        public void Reativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Apenas programação inativa pode ser reativada. (ESC-REG-013)");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>ESC-REG-012: programação ativa pode ser encerrada por gestor.</summary>
        public void Encerrar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo && Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Somente programação ativa ou inativa pode ser encerrada. (ESC-REG-012)");
                return;
            }
            Status = EStatusWorkflowProducao.Encerrado;
            MarcarAlterado(alteradoPor);
        }
    }
}
