using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-CST — Registro principal de custo de produção (prd_cst_custo_producao).
    /// Fiel ao EF CUSTOS_DE_PRODUCAO §17. Workflow + desvio = realizado − previsto.
    /// </summary>
    public class CustoProducao : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public EStatusWorkflowProducao Status { get; private set; } = EStatusWorkflowProducao.Rascunho;
        public Guid ResponsavelId { get; private set; }
        public string? ReferenciaOrigem { get; private set; }
        public Guid? ReferenciaId { get; private set; }
        public decimal? CustoTotalPrevisto { get; private set; }
        public decimal? CustoTotalRealizado { get; private set; }
        public decimal? DesvioTotal { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        public List<CustoReferencia> Referencias { get; private set; } = new();

        protected CustoProducao() { } // EF Core

        public CustoProducao(
            string codigo,
            Guid responsavelId,
            string tenantId,
            string criadoPor,
            string? referenciaOrigem = null,
            Guid? referenciaId = null,
            decimal? custoTotalPrevisto = null,
            decimal? custoTotalRealizado = null)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            ResponsavelId = responsavelId;
            Status = EStatusWorkflowProducao.Rascunho;
            ReferenciaOrigem = referenciaOrigem;
            ReferenciaId = referenciaId;
            CustoTotalPrevisto = custoTotalPrevisto;
            CustoTotalRealizado = custoTotalRealizado;
            RecalcularDesvio();

            AddNotifications(new Contract<CustoProducao>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do custo é obrigatório [Origem: CustoProducao].")
                .AreNotEquals(responsavelId, Guid.Empty, nameof(ResponsavelId), "O responsável é obrigatório [Origem: CustoProducao].")
            );
        }

        public void RecalcularDesvio()
        {
            if (CustoTotalPrevisto.HasValue || CustoTotalRealizado.HasValue)
            {
                DesvioTotal = CustoTotalRealizado.GetValueOrDefault(0m) - CustoTotalPrevisto.GetValueOrDefault(0m);
            }
        }

        public void AdicionarReferencia(
            string tipoReferencia,
            string alteradoPor,
            Guid? referenciaId = null,
            decimal? custoPrevisto = null,
            decimal? custoRealizado = null,
            decimal? custoExtra = null,
            string? tipoCustoProducao = null,
            decimal? percentualCustoProducao = null)
        {
            var referencia = new CustoReferencia(
                Id, tipoReferencia, TenantId, alteradoPor,
                referenciaId, custoPrevisto, custoRealizado, custoExtra,
                tipoCustoProducao, percentualCustoProducao);

            if (!referencia.IsValid)
            {
                AddNotifications(referencia.Notifications);
                return;
            }
            Referencias.Add(referencia);
        }

        public void SubmeterParaAnalise(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas custo em Rascunho pode ser submetido para análise.");
                return;
            }
            Status = EStatusWorkflowProducao.EmAnalise;
            MarcarAlterado(alteradoPor);
        }

        public void Aprovar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas custo em análise pode ser aprovado.");
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
                AddNotification(nameof(Status), "Apenas custo em análise pode ser rejeitado.");
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
                AddNotification(nameof(Status), "Apenas custo ativo pode ser inativado.");
                return;
            }
            Status = EStatusWorkflowProducao.Inativo;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Apenas custo inativo pode ser reativado.");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MarcarAlterado(alteradoPor);
        }

        public void Encerrar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo && Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Somente custo ativo ou inativo pode ser encerrado.");
                return;
            }
            Status = EStatusWorkflowProducao.Encerrado;
            MarcarAlterado(alteradoPor);
        }
    }
}
