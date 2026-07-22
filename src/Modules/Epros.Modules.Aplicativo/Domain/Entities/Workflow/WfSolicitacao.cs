using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Workflow
{
    /// <summary>
    /// wf_solicitacao — caso funcional de solicitação com aprovação. Campos preservados fielmente do
    /// material (start_date, end_date, total_days, reason, attachment, approver_comment...). [Origem: EF WORKFLOW 10.8]
    /// </summary>
    public class WfSolicitacao : EntidadeSaaSBase
    {
        public DateTime? StartDate { get; private set; }
        public DateTime? EndDate { get; private set; }
        public decimal? TotalDays { get; private set; }
        public string? Reason { get; private set; }
        public string? Attachment { get; private set; }
        public EWfSolicitacaoStatus Status { get; private set; }
        public string? ApproverComment { get; private set; }
        public DateTime? ApprovedAt { get; private set; }
        public string? EmployeeId { get; private set; }
        public string? LeaveTypeId { get; private set; }
        public string? ApprovedBy { get; private set; }
        public string? CreatorId { get; private set; }

        protected WfSolicitacao() { } // EF Core

        public WfSolicitacao(
            DateTime? startDate,
            DateTime? endDate,
            decimal? totalDays,
            string? reason,
            string? attachment,
            string? employeeId,
            string? leaveTypeId,
            string? creatorId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            StartDate = startDate;
            EndDate = endDate;
            TotalDays = totalDays;
            Reason = reason;
            Attachment = attachment;
            EmployeeId = employeeId;
            LeaveTypeId = leaveTypeId;
            CreatorId = creatorId;
            Status = EWfSolicitacaoStatus.Pendente;
            Validar();
        }

        public void Aprovar(string approvedBy, string alteradoPor)
        {
            Clear();
            if (Status != EWfSolicitacaoStatus.Pendente)
            {
                AddNotification(nameof(Status), "Só é possível aprovar solicitação pendente [Origem: WfSolicitacao]");
                return;
            }
            Status = EWfSolicitacaoStatus.Aprovada;
            ApprovedBy = approvedBy;
            ApprovedAt = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Rejeitar(string approvedBy, string? approverComment, string alteradoPor)
        {
            Clear();
            if (Status != EWfSolicitacaoStatus.Pendente)
            {
                AddNotification(nameof(Status), "Só é possível rejeitar solicitação pendente [Origem: WfSolicitacao]");
                return;
            }
            Status = EWfSolicitacaoStatus.Rejeitada;
            ApprovedBy = approvedBy;
            ApproverComment = approverComment;
            ApprovedAt = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
            {
                AddNotification(nameof(EndDate), "A data final não pode ser anterior à data inicial [Origem: WfSolicitacao]");
            }
        }
    }
}
