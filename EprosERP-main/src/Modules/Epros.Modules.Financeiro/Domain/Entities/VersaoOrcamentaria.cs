using System;
using System.Collections.Generic;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Versão orçamentária (planejamento) com linhas previsto x realizado (EF FIN-PO §12.1 po_versao_orcamentaria).
    /// Submódulo de evolução — sobe desabilitado (ABAC nega por padrão). Isolamento por tenant via ContextBase.
    /// </summary>
    public class VersaoOrcamentaria : EntidadeSaaSBase
    {
        public string? Nome { get; private set; }
        public DateTime? PeriodoInicio { get; private set; }
        public DateTime? PeriodoFim { get; private set; }
        public EStatusOrcamento Status { get; private set; } = EStatusOrcamento.Rascunho;

        private readonly List<LinhaOrcamento> _linhas = new();
        public IReadOnlyCollection<LinhaOrcamento> Linhas => _linhas.AsReadOnly();

        protected VersaoOrcamentaria() { } // EF Core

        public VersaoOrcamentaria(string? nome, DateTime? periodoInicio, DateTime? periodoFim, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            PeriodoInicio = periodoInicio;
            PeriodoFim = periodoFim;
            Status = EStatusOrcamento.Rascunho;
            Validar();
        }

        public void AdicionarLinha(Guid contaId, Guid? centroCustoId, string periodo, decimal valorOrcado, string tenantId, string criadoPor)
        {
            _linhas.Add(new LinhaOrcamento(Id, contaId, centroCustoId, periodo, valorOrcado, tenantId, criadoPor));
        }

        public void Aprovar(string usuario)
        {
            if (Status != EStatusOrcamento.Rascunho) { AddNotification(nameof(Status), "Somente versão em rascunho pode ser aprovada."); return; }
            Status = EStatusOrcamento.Aprovado; MarcarAlterado(usuario);
        }

        public void Ativar(string usuario)
        {
            if (Status != EStatusOrcamento.Aprovado) { AddNotification(nameof(Status), "Somente versão aprovada pode ser ativada."); return; }
            Status = EStatusOrcamento.Ativo; MarcarAlterado(usuario);
        }

        public void Encerrar(string usuario)
        {
            if (Status != EStatusOrcamento.Ativo) { AddNotification(nameof(Status), "Somente versão ativa pode ser encerrada."); return; }
            Status = EStatusOrcamento.Encerrado; MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            if (PeriodoInicio.HasValue && PeriodoFim.HasValue && PeriodoFim.Value < PeriodoInicio.Value)
                AddNotification(nameof(PeriodoFim), "O fim do período deve ser posterior ao início [Origem: VersaoOrcamentaria]");
        }
    }

    /// <summary>Linha de orçamento com previsto x realizado (EF FIN-PO §12.2 po_linha_orcamento). Conta/centro cross-module por Guid FK.</summary>
    public class LinhaOrcamento : EntidadeSaaSBase
    {
        public Guid VersaoId { get; private set; }
        public Guid ContaId { get; private set; }
        public Guid? CentroCustoId { get; private set; }
        public string Periodo { get; private set; } = string.Empty;
        public decimal ValorOrcado { get; private set; }
        public decimal? ValorRealizado { get; private set; }
        public decimal? VariacaoValor { get; private set; }
        public decimal? VariacaoPercentual { get; private set; }

        public VersaoOrcamentaria Versao { get; private set; } = null!;

        protected LinhaOrcamento() { } // EF Core

        public LinhaOrcamento(Guid versaoId, Guid contaId, Guid? centroCustoId, string periodo, decimal valorOrcado, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VersaoId = versaoId;
            ContaId = contaId;
            CentroCustoId = centroCustoId;
            Periodo = periodo;
            ValorOrcado = valorOrcado;
            Validar();
        }

        public void RegistrarRealizado(decimal valorRealizado, string usuario)
        {
            ValorRealizado = valorRealizado;
            VariacaoValor = valorRealizado - ValorOrcado;
            VariacaoPercentual = ValorOrcado != 0 ? (VariacaoValor / ValorOrcado) * 100 : (decimal?)null;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LinhaOrcamento>()
                .Requires()
                .IsNotEmpty(ContaId, nameof(ContaId), "A conta é obrigatória [Origem: LinhaOrcamento]")
                .IsNotNullOrEmpty(Periodo, nameof(Periodo), "O período é obrigatório [Origem: LinhaOrcamento]"));
        }
    }
}
