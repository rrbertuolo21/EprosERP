using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Projetos.Domain.Entities.Orcamento
{
    /// <summary>
    /// Agregado raiz do orcamento/baseline do projeto. Origem: EF PRJ-ORC 11.1 (prj_orcamento_projeto).
    /// RN-ORC-008 (budget >= 0), RN-ORC-011 (aprovado exige nova versao/aprovacao).
    /// </summary>
    public class OrcamentoProjeto : EntidadeSaaSBase
    {
        public Guid ProjetoId { get; private set; }
        public decimal Budget { get; private set; }
        public EBillingType? BillingType { get; private set; }
        public decimal? BillingRate { get; private set; }
        public decimal? EstimatedHours { get; private set; }
        public decimal? CostsEstimate { get; private set; }
        public EProjetoWorkflowStatus Status { get; private set; } = EProjetoWorkflowStatus.Rascunho;
        public int Versao { get; private set; }

        public List<MarcoOrcamentario> Marcos { get; private set; } = new();

        protected OrcamentoProjeto() { } // EF Core

        public OrcamentoProjeto(
            Guid projetoId,
            decimal budget,
            EBillingType? billingType,
            decimal? billingRate,
            decimal? estimatedHours,
            decimal? costsEstimate,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<OrcamentoProjeto>()
                .Requires()
                .AreNotEquals(projetoId, Guid.Empty, nameof(ProjetoId), "O projeto e obrigatorio. [Origem: OrcamentoProjeto]"));

            if (budget < 0)
                AddNotification(nameof(Budget), "O orcamento deve ser maior ou igual a zero. [Origem: OrcamentoProjeto]");

            ProjetoId = projetoId;
            Budget = budget;
            BillingType = billingType;
            BillingRate = billingRate;
            EstimatedHours = estimatedHours;
            CostsEstimate = costsEstimate;
            Status = EProjetoWorkflowStatus.Rascunho;
            Versao = 1;
        }

        public void AdicionarMarco(string titulo, decimal custo, DateTime dataInicio, DateTime dataFim, string? resumo, string usuario)
        {
            var marco = new MarcoOrcamentario(Id, ProjetoId, titulo, custo, dataInicio, dataFim, resumo, TenantId, usuario);
            if (!marco.IsValid)
            {
                AddNotifications(marco.Notifications);
                return;
            }
            Marcos.Add(marco);
            MarcarAlterado(usuario);
        }

        /// <summary>RN-ORC-011: alteracao pos-aprovacao exige nova versao.</summary>
        public void AlterarOrcamento(decimal budget, string usuario)
        {
            if (Status == EProjetoWorkflowStatus.Ativo)
            {
                Versao++;
                Status = EProjetoWorkflowStatus.EmAnalise;
            }

            if (budget < 0)
            {
                AddNotification(nameof(Budget), "O orcamento deve ser maior ou igual a zero. [Origem: OrcamentoProjeto]");
                return;
            }

            Budget = budget;
            MarcarAlterado(usuario);
        }

        public void Submeter(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.Rascunho)
            {
                AddNotification(nameof(Status), "So e possivel submeter orcamento em Rascunho. [Origem: OrcamentoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.EmAnalise;
            MarcarAlterado(usuario);
        }

        public void Aprovar(string usuario)
        {
            if (Status != EProjetoWorkflowStatus.EmAnalise)
            {
                AddNotification(nameof(Status), "A aprovacao so ocorre a partir de EmAnalise. [Origem: OrcamentoProjeto]");
                return;
            }
            Status = EProjetoWorkflowStatus.Ativo;
            MarcarAlterado(usuario);
        }
    }
}
