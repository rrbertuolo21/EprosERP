using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities
{
    public class ExecucaoMassaGlobal : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        public string ActionPayload { get; private set; } = string.Empty;
        public string CommandType { get; private set; } = string.Empty;
        public string Status { get; private set; } = "Draft"; // Draft, Active, Completed, Failed
        public Guid? AprovadoPor { get; private set; }
        public string? ResultadoLog { get; private set; }

        protected ExecucaoMassaGlobal() { } // EF Core

        public ExecucaoMassaGlobal(string descricao, string actionPayload, string status, string tenantId, string criadoPor, string commandType = "")
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ExecucaoMassaGlobal>()
                .Requires()
                .IsNotNullOrEmpty(descricao, nameof(Descricao), "A descrição é obrigatória")
                .IsNotNullOrEmpty(actionPayload, nameof(ActionPayload), "O payload de ação é obrigatório")
                .IsNotNullOrEmpty(status, nameof(Status), "O status inicial é obrigatório")
            );

            Descricao = descricao;
            ActionPayload = actionPayload;
            Status = status;
            CommandType = commandType;
        }

        public void RegistrarFalha(string log, string alteradoPor)
        {
            Status = "Failed";
            ResultadoLog = log;
            MarcarAlterado(alteradoPor);
        }

        public void ConcluirComSucesso(string log, string alteradoPor)
        {
            Status = "Completed";
            ResultadoLog = log;
            MarcarAlterado(alteradoPor);
        }

        public void RegistrarSimulacao(string log, string alteradoPor)
        {
            ResultadoLog = log;
            MarcarAlterado(alteradoPor);
        }

        public void Ativar(Guid aprovadoPor, string aprovadoPorUserId, string alteradoPor)
        {
            if (Status != "Draft")
            {
                AddNotification(nameof(Status), "A execução em massa só pode ser ativa a partir do status Draft");
            }

            if (aprovadoPorUserId == CriadoPor)
            {
                AddNotification(nameof(AprovadoPor), "O aprovador da execução em massa deve ser diferente do criador (Maker-Checker)");
            }

            if (IsValid)
            {
                Status = "Active";
                AprovadoPor = aprovadoPor;
                MarcarAlterado(alteradoPor);
            }
        }

        public void Concluir(string alteradoPor)
        {
            if (Status != "Active")
            {
                AddNotification(nameof(Status), "A execução em massa só pode ser concluída a partir do status Active");
            }

            if (IsValid)
            {
                Status = "Completed";
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
