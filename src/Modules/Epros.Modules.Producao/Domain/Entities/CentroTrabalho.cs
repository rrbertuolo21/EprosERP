using System;
using Epros.Modules.Producao.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Producao.Domain.Entities
{
    /// <summary>
    /// PRD-CTR — Centro de trabalho (prd_ctr_centro_trabalho — PD22 · DP-ESC-001/002).
    /// Base de capacidade fabril (PD4): calendário simplificado por turnos/dias úteis e eficiência.
    /// Hoje o código só tinha um `CentroTrabalhoId` solto em prd_esc_programacao — vira entidade real.
    /// Workflow: Rascunho → EmAnalise → Ativo → Inativo/Encerrado.
    /// </summary>
    public class CentroTrabalho : EntidadeSaaSBase
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string? Descricao { get; private set; }
        public EStatusWorkflowProducao Status { get; private set; } = EStatusWorkflowProducao.Rascunho;
        public decimal MinutosPorTurno { get; private set; }
        public int TurnosPorDia { get; private set; }
        public int DiasUteisPorSemana { get; private set; }
        public decimal EficienciaPercentual { get; private set; } = 100m;
        public string? MotivoRejeicao { get; private set; }

        protected CentroTrabalho() { } // EF Core

        public CentroTrabalho(
            string codigo,
            string nome,
            decimal minutosPorTurno,
            int turnosPorDia,
            int diasUteisPorSemana,
            string tenantId,
            string criadoPor,
            string? descricao = null,
            decimal eficienciaPercentual = 100m)
            : base(tenantId, criadoPor)
        {
            Codigo = codigo;
            Nome = nome;
            Descricao = descricao;
            MinutosPorTurno = minutosPorTurno;
            TurnosPorDia = turnosPorDia;
            DiasUteisPorSemana = diasUteisPorSemana;
            EficienciaPercentual = eficienciaPercentual <= 0m ? 100m : eficienciaPercentual;
            Status = EStatusWorkflowProducao.Rascunho;

            AddNotifications(new Contract<CentroTrabalho>()
                .Requires()
                .IsNotNullOrEmpty(codigo, nameof(Codigo), "O código do centro de trabalho é obrigatório [Origem: CentroTrabalho]. (CTR-REG-001)")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do centro de trabalho é obrigatório [Origem: CentroTrabalho]. (CTR-REG-002)")
                .IsGreaterThan(minutosPorTurno, 0m, nameof(MinutosPorTurno), "Os minutos por turno devem ser maiores que zero [Origem: CentroTrabalho].")
                .IsGreaterThan(turnosPorDia, 0, nameof(TurnosPorDia), "Os turnos por dia devem ser maiores que zero [Origem: CentroTrabalho].")
                .IsGreaterOrEqualsThan(diasUteisPorSemana, 1, nameof(DiasUteisPorSemana), "Os dias úteis por semana devem estar entre 1 e 7 [Origem: CentroTrabalho].")
                .IsLowerOrEqualsThan(diasUteisPorSemana, 7, nameof(DiasUteisPorSemana), "Os dias úteis por semana devem estar entre 1 e 7 [Origem: CentroTrabalho].")
            );
        }

        public void SubmeterParaAnalise(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Rascunho)
            {
                AddNotification(nameof(Status), "Apenas centro em Rascunho pode ser submetido para análise.");
                return;
            }
            Status = EStatusWorkflowProducao.EmAnalise;
            MarcarAlterado(alteradoPor);
        }

        public void Aprovar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.EmAnalise)
            {
                AddNotification(nameof(Status), "Apenas centro em análise pode ser aprovado.");
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
                AddNotification(nameof(Status), "Apenas centro em análise pode ser rejeitado.");
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
                AddNotification(nameof(Status), "Apenas centro ativo pode ser inativado.");
                return;
            }
            Status = EStatusWorkflowProducao.Inativo;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Apenas centro inativo pode ser reativado.");
                return;
            }
            Status = EStatusWorkflowProducao.Ativo;
            MarcarAlterado(alteradoPor);
        }

        public void Encerrar(string alteradoPor)
        {
            if (Status != EStatusWorkflowProducao.Ativo && Status != EStatusWorkflowProducao.Inativo)
            {
                AddNotification(nameof(Status), "Somente centro ativo ou inativo pode ser encerrado.");
                return;
            }
            Status = EStatusWorkflowProducao.Encerrado;
            MarcarAlterado(alteradoPor);
        }
    }
}
