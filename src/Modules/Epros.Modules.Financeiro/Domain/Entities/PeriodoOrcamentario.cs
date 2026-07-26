using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>Período orçamentário com ciclo de estados (EF FIN-PO §12.6 po_periodo_orcamentario).</summary>
    public class PeriodoOrcamentario : EntidadeSaaSBase
    {
        public DateTime DataInicio { get; private set; }
        public DateTime DataFim { get; private set; }
        public EStatusOrcamento Status { get; private set; } = EStatusOrcamento.Rascunho;

        protected PeriodoOrcamentario() { } // EF Core

        public PeriodoOrcamentario(DateTime dataInicio, DateTime dataFim, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DataInicio = dataInicio;
            DataFim = dataFim;
            Status = EStatusOrcamento.Rascunho;
            Validar();
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusOrcamento.Rascunho) { AddNotification(nameof(Status), "Somente período em rascunho pode ser aprovado."); return; }
            Status = EStatusOrcamento.Aprovado; MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            if (Status != EStatusOrcamento.Aprovado) { AddNotification(nameof(Status), "Somente período aprovado pode ser ativado."); return; }
            Status = EStatusOrcamento.Ativo; MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            if (Status != EStatusOrcamento.Ativo) { AddNotification(nameof(Status), "Somente período ativo pode ser encerrado."); return; }
            Status = EStatusOrcamento.Encerrado; MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<PeriodoOrcamentario>()
                .Requires()
                .IsGreaterThan(DataInicio, DateTime.MinValue, nameof(DataInicio), "A data de início é obrigatória [Origem: PeriodoOrcamentario]"));
            if (DataFim <= DataInicio)
                AddNotification(nameof(DataFim), "A data fim deve ser posterior à data início [Origem: PeriodoOrcamentario]");
        }
    }

    /// <summary>Budget de um período orçamentário (EF FIN-PO §12.7 po_budget).</summary>
    public class Budget : EntidadeSaaSBase
    {
        public Guid PeriodoId { get; private set; }
        public string Tipo { get; private set; } = string.Empty;
        public EStatusOrcamento Status { get; private set; } = EStatusOrcamento.Rascunho;
        public decimal? Valor { get; private set; }

        public PeriodoOrcamentario Periodo { get; private set; } = null!;

        protected Budget() { } // EF Core

        public Budget(Guid periodoId, string tipo, decimal? valor, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PeriodoId = periodoId;
            Tipo = tipo;
            Valor = valor;
            Status = EStatusOrcamento.Rascunho;
            Validar();
        }

        public void Alterar(string tipo, decimal? valor, string usuario)
        {
            if (Status != EStatusOrcamento.Rascunho) { AddNotification(nameof(Status), "Budget fora de rascunho não pode ser editado."); return; }
            Tipo = tipo; Valor = valor; MarcarAlterado(usuario); Validar();
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusOrcamento.Rascunho) { AddNotification(nameof(Status), "Somente budget em rascunho pode ser aprovado."); return; }
            Status = EStatusOrcamento.Aprovado; MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Budget>()
                .Requires()
                .IsNotEmpty(PeriodoId, nameof(PeriodoId), "O período é obrigatório [Origem: Budget]")
                .IsNotNullOrEmpty(Tipo, nameof(Tipo), "O tipo do budget é obrigatório [Origem: Budget]"));
        }
    }

    /// <summary>Alocação de budget a uma conta (EF FIN-PO §12.8 po_budget_alocacao). Conta cross-module por Guid FK.</summary>
    public class BudgetAlocacao : EntidadeSaaSBase
    {
        public Guid BudgetId { get; private set; }
        public Guid ContaId { get; private set; }
        public decimal? ValorAlocado { get; private set; }
        public bool AutoAprovar { get; private set; }

        protected BudgetAlocacao() { } // EF Core

        public BudgetAlocacao(Guid budgetId, Guid contaId, decimal? valorAlocado, bool autoAprovar, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            BudgetId = budgetId;
            ContaId = contaId;
            ValorAlocado = valorAlocado;
            AutoAprovar = autoAprovar;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<BudgetAlocacao>()
                .Requires()
                .IsNotEmpty(BudgetId, nameof(BudgetId), "O budget é obrigatório [Origem: BudgetAlocacao]")
                .IsNotEmpty(ContaId, nameof(ContaId), "A conta é obrigatória [Origem: BudgetAlocacao]"));
        }
    }
}
