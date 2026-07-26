using System;
using System.Collections.Generic;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-BOM — Registro principal da estrutura de produto (prd_bom_estrutura).
    /// Fiel ao EF ESTRUTURA_DE_PRODUTO_BOM §17. Workflow: Rascunho → EmAnalise → Ativo → Inativo/Encerrado.
    /// </summary>
    public class BomEstrutura : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public Guid VariacaoId { get; private set; }
        public string? Codigo { get; private set; }
        public EStatusWorkflowProducao Status { get; private set; } = EStatusWorkflowProducao.Rascunho;
        public string? IngredientesJson { get; private set; }
        public string? Instrucoes { get; private set; }
        public decimal PercentualDesperdicio { get; private set; }
        public decimal CustoIngredientes { get; private set; }
        public decimal CustoExtra { get; private set; }
        public string? TipoCustoProducao { get; private set; }
        public decimal QuantidadeTotal { get; private set; }
        public decimal? PrecoFinal { get; private set; }
        public Guid? SubUnidadeId { get; private set; }
        public string? Versao { get; private set; }
        public DateTime? InicioVigencia { get; private set; }
        public DateTime? FimVigencia { get; private set; }
        public string? MotivoRejeicao { get; private set; }

        public List<BomComponente> Componentes { get; private set; } = new();

        protected BomEstrutura() { } // EF Core

        public BomEstrutura(
            Guid produtoId,
            Guid variacaoId,
            string? codigo,
            decimal percentualDesperdicio,
            decimal custoIngredientes,
            decimal custoExtra,
            decimal quantidadeTotal,
            string tenantId,
            string criadoPor,
            string? ingredientesJson = null,
            string? instrucoes = null,
            string? tipoCustoProducao = null,
            decimal? precoFinal = null,
            Guid? subUnidadeId = null,
            string? versao = null,
            DateTime? inicioVigencia = null,
            DateTime? fimVigencia = null)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            VariacaoId = variacaoId;
            Codigo = codigo;
            Status = EStatusWorkflowProducao.Rascunho;
            IngredientesJson = ingredientesJson;
            Instrucoes = instrucoes;
            PercentualDesperdicio = percentualDesperdicio;
            CustoIngredientes = custoIngredientes;
            CustoExtra = custoExtra;
            TipoCustoProducao = tipoCustoProducao;
            QuantidadeTotal = quantidadeTotal;
            PrecoFinal = precoFinal;
            SubUnidadeId = subUnidadeId;
            Versao = versao;
            InicioVigencia = inicioVigencia;
            FimVigencia = fimVigencia;

            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<BomEstrutura>()
                .Requires()
                .AreNotEquals(ProdutoId, Guid.Empty, nameof(ProdutoId), "O produto é obrigatório [Origem: BomEstrutura]. (BOM-REG-002)")
                .AreNotEquals(VariacaoId, Guid.Empty, nameof(VariacaoId), "A variação do produto é obrigatória [Origem: BomEstrutura]. (BOM-REG-003)")
                .IsGreaterThan(QuantidadeTotal, 0, nameof(QuantidadeTotal), "A quantidade total deve ser maior que zero [Origem: BomEstrutura].")
                .IsGreaterOrEqualsThan(CustoIngredientes, 0m, nameof(CustoIngredientes), "O custo de ingredientes deve ser maior ou igual a zero [Origem: BomEstrutura]. (BOM-REG-013)")
            );
        }

        /// <summary>Inclui componente na estrutura (BOM-EF §7.2). Recalcula custo dinâmico da linha.</summary>
        public void AdicionarComponente(
            Guid variacaoComponenteId,
            decimal quantidade,
            string alteradoPor,
            Guid? subUnidadeId = null,
            decimal? multiplicadorUnidade = null,
            decimal? percentualDesperdicioLinha = null,
            Guid? grupoComponenteId = null,
            int? ordemMontagem = null,
            decimal? custoUnitarioComImpostos = null)
        {
            var componente = new BomComponente(
                Id, variacaoComponenteId, quantidade, TenantId, alteradoPor,
                subUnidadeId, multiplicadorUnidade, percentualDesperdicioLinha,
                grupoComponenteId, ordemMontagem, custoUnitarioComImpostos);

            if (!componente.IsValid)
            {
                AddNotifications(componente.Notifications);
                return;
            }
            Componentes.Add(componente);
        }

        /// <summary>BOM-REG-024: apenas Rascunho pode ser submetida; exige ao menos um componente (BOM-REG-005).</summary>
        public void SubmeterParaAnalise(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas estrutura em Rascunho pode ser submetida para análise. (BOM-REG-024)");
                return;
            }
            if (Componentes.Count == 0)
            {
                AddNotification(nameof(Componentes), "A estrutura deve possuir ao menos um componente para ser submetida. (BOM-REG-005)");
                return;
            }
            Status = EStatusWorkflowProducao.EmAnalise;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>BOM-REG-026: estrutura aprovada passa para Ativo.</summary>
        public void Aprovar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas estrutura em análise pode ser aprovada. (BOM-REG-025)");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MotivoRejeicao = null;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>BOM-REG-027: estrutura rejeitada retorna para Rascunho com motivo.</summary>
        public void Rejeitar(string motivo, string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas estrutura em análise pode ser rejeitada. (BOM-REG-025)");
                return;
            }
            if (string.IsNullOrWhiteSpace(motivo))
            {
                AddNotification(nameof(MotivoRejeicao), "O motivo da rejeição é obrigatório. (BOM-REG-027)");
                return;
            }
            Status = EStatusWorkflowProducao.Rascunho;
            MotivoRejeicao = motivo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>BOM-REG-028: estrutura ativa pode ser inativada por gestor.</summary>
        public void Inativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo)
            {
                AddNotification(nameof(Status), "Apenas estrutura ativa pode ser inativada. (BOM-REG-028)");
                return;
            }
            Status = EStatusWorkflowProducao.Inativo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>BOM-REG-029: estrutura inativa pode ser reativada por gestor.</summary>
        public void Reativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Apenas estrutura inativa pode ser reativada. (BOM-REG-029)");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>BOM-REG-028: estrutura ativa pode ser encerrada por gestor.</summary>
        public void Encerrar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo && Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Somente estrutura ativa ou inativa pode ser encerrada. (BOM-REG-028)");
                return;
            }
            Status = EStatusWorkflowProducao.Encerrado;
            MarcarAlterado(alteradoPor);
        }
    }
}
