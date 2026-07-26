using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class LogExecucaoMassa : EntidadeSaaSBase
    {
        public Guid ExecuteQueryId { get; private set; }
        public string TargetTenantId { get; private set; } = string.Empty;
        public string Status { get; private set; } = string.Empty; // Passed, Failed
        public string? Mensagem { get; private set; }
        public DateTime ProcessadoEm { get; private set; }

        protected LogExecucaoMassa() { } // EF Core

        public LogExecucaoMassa(
            Guid executeQueryId, 
            string targetTenantId, 
            string status, 
            string? mensagem,
            string tenantId, 
            string criadoPor) : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<LogExecucaoMassa>()
                .Requires()
                .IsNotNullOrEmpty(targetTenantId, nameof(TargetTenantId), "O tenant alvo do processamento é obrigatório")
                .IsNotNullOrEmpty(status, nameof(Status), "O status de processamento é obrigatório")
            );

            ExecuteQueryId = executeQueryId;
            TargetTenantId = targetTenantId;
            Status = status;
            Mensagem = mensagem;
            ProcessadoEm = DateTime.UtcNow;
        }
    }
}
