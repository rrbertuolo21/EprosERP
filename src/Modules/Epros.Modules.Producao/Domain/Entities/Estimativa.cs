using System;
using System.Collections.Generic;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-EST — Registro principal de estimativa industrial (prd_est_estimativa).
    /// Fiel ao EF ESTIMATIVA §17. Workflow + conversão para planejamento/ordem (EST §7.4).
    /// </summary>
    public class Estimativa : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public EStatusWorkflowProducao Status { get; private set; } = EStatusWorkflowProducao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public Guid? PropostaReferenciaId { get; private set; }
        public Guid? EstruturaRascunhoId { get; private set; }
        public decimal? CustoPrevistoTotal { get; private set; }
        public Guid? PlanejamentoOrigemId { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        public List<EstimativaComponente> Componentes { get; private set; } = new();

        protected Estimativa() { } // EF Core

        public Estimativa(
            string codigo,
            Guid responsavelId,
            string tenantId,
            string criadoPor,
            Guid? propostaReferenciaId = null,
            Guid? estruturaRascunhoId = null,
            decimal? custoPrevistoTotal = null)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            ResponsavelId = responsavelId;
            Status = EStatusWorkflowProducao.Rascunho;
            PropostaReferenciaId = propostaReferenciaId;
            EstruturaRascunhoId = estruturaRascunhoId;
            CustoPrevistoTotal = custoPrevistoTotal;

            AddNotifications(new Contract<Estimativa>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código da estimativa é obrigatório [Origem: Estimativa].")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsável é obrigatório [Origem: Estimativa].")
            );
        }

        public void AdicionarComponente(
            string tipoComponente,
            string alteradoPor,
            Guid? referenciaId = null,
            decimal? quantidade = null,
            decimal? tempoEstimado = null,
            decimal? taxaEstimada = null,
            decimal? custoPrevisto = null,
            string? observacao = null)
        {
            var componente = new EstimativaComponente(
                Id, tipoComponente, TenantId, alteradoPor,
                referenciaId, quantidade, tempoEstimado, taxaEstimada, custoPrevisto, observacao);

            if (!componente.IsValid)
            {
                AddNotifications(componente.Notifications);
                return;
            }
            Componentes.Add(componente);
        }

        public void SubmeterParaAnalise(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas estimativa em Rascunho pode ser submetida.");
                return;
            }
            Status = EStatusWorkflowProducao.EmAnalise;
            MarcarAlterado(alteradoPor);
        }

        public void Aprovar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas estimativa em análise pode ser aprovada.");
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
                AddNotification(nameof(Status), "Apenas estimativa em análise pode ser rejeitada.");
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

        /// <summary>EST §7.4: converte estimativa ativa em planejamento/ordem, registrando a origem gerada.</summary>
        public void ConverterParaPlanejamento(Guid planejamentoOrigemId, string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo)
            {
                AddNotification(nameof(Status), "Somente estimativa ativa pode ser convertida para planejamento/ordem.");
                return;
            }
            if (planejamentoOrigemId == Guid.Empty)
            {
                AddNotification(nameof(PlanejamentoOrigemId), "A referência gerada na conversão é obrigatória.");
                return;
            }
            PlanejamentoOrigemId = planejamentoOrigemId;
            MarcarAlterado(alteradoPor);
        }

        public void Inativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo)
            {
                AddNotification(nameof(Status), "Apenas estimativa ativa pode ser inativada.");
                return;
            }
            Status = EStatusWorkflowProducao.Inativo;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Apenas estimativa inativa pode ser reativada.");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MarcarAlterado(alteradoPor);
        }

        public void Encerrar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo && Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Somente estimativa ativa ou inativa pode ser encerrada.");
                return;
            }
            Status = EStatusWorkflowProducao.Encerrado;
            MarcarAlterado(alteradoPor);
        }
    }
}
