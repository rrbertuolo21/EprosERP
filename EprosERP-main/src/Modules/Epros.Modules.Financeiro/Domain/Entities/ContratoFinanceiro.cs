using System;
using Epros.Modules.Financeiro.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Contrato financeiro recorrente vinculado a um plano e a uma pessoa (EF FIN-GCF §13.2 gcf_contrato_financeiro).
    /// Plano intra-módulo (navegação); pessoa cross-module por Guid FK.
    /// </summary>
    public class ContratoFinanceiro : EntidadeSaaSBase
    {
        public Guid PlanoId { get; private set; }
        public Guid PessoaId { get; private set; }
        public DateTime DataInicio { get; private set; }
        public DateTime? DataFim { get; private set; }
        public EStatusContratoFinanceiro Status { get; private set; } = EStatusContratoFinanceiro.Ativo;
        public string? MotivoCancelamento { get; private set; }

        public PlanoContrato Plano { get; private set; } = null!;

        protected ContratoFinanceiro() { } // EF Core

        public ContratoFinanceiro(Guid planoId, Guid pessoaId, DateTime dataInicio, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            PlanoId = planoId;
            PessoaId = pessoaId;
            DataInicio = dataInicio;
            Status = EStatusContratoFinanceiro.Ativo;
            Validar();
        }

        public void Suspender(string usuario)
        {
            if (Status != EStatusContratoFinanceiro.Ativo)
            {
                AddNotification(nameof(Status), "Somente contrato ativo pode ser suspenso.");
                return;
            }
            Status = EStatusContratoFinanceiro.Suspenso;
            MarcarAlterado(usuario);
        }

        public void Reativar(string usuario)
        {
            if (Status != EStatusContratoFinanceiro.Suspenso)
            {
                AddNotification(nameof(Status), "Somente contrato suspenso pode ser reativado.");
                return;
            }
            Status = EStatusContratoFinanceiro.Ativo;
            MarcarAlterado(usuario);
        }

        public void Cancelar(string motivo, DateTime? dataFim, string usuario)
        {
            if (Status == EStatusContratoFinanceiro.Cancelado)
            {
                AddNotification(nameof(Status), "Contrato já cancelado.");
                return;
            }
            Status = EStatusContratoFinanceiro.Cancelado;
            MotivoCancelamento = motivo;
            DataFim = dataFim ?? DateTime.UtcNow;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ContratoFinanceiro>()
                .Requires()
                .IsNotEmpty(PlanoId, nameof(PlanoId), "O plano é obrigatório [Origem: ContratoFinanceiro]")
                .IsNotEmpty(PessoaId, nameof(PessoaId), "A pessoa é obrigatória [Origem: ContratoFinanceiro]")
                .IsGreaterThan(DataInicio, DateTime.MinValue, nameof(DataInicio), "A data de início é obrigatória [Origem: ContratoFinanceiro]")
            );
        }
    }
}
