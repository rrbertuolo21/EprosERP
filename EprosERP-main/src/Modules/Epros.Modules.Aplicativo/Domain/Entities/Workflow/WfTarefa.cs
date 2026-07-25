using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Workflow
{
    /// <summary>
    /// wf_tarefa — pendência humana atribuída a usuário ou papel para completar uma etapa. [Origem: EF WORKFLOW 10.7]
    /// </summary>
    public class WfTarefa : EntidadeSaaSBase
    {
        public Guid InstanciaId { get; private set; }
        public string Titulo { get; private set; } = string.Empty;
        public Guid? ResponsavelUsuarioId { get; private set; }
        public EWfPermissao? ResponsavelPapel { get; private set; }
        public EWfTarefaStatus Status { get; private set; }
        public DateTime? PrazoEm { get; private set; }
        public DateTime? ConcluidaEm { get; private set; }

        protected WfTarefa() { } // EF Core

        public WfTarefa(
            Guid instanciaId,
            string titulo,
            Guid? responsavelUsuarioId,
            EWfPermissao? responsavelPapel,
            DateTime? prazoEm,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            InstanciaId = instanciaId;
            Titulo = titulo;
            ResponsavelUsuarioId = responsavelUsuarioId;
            ResponsavelPapel = responsavelPapel;
            PrazoEm = prazoEm;
            Status = EWfTarefaStatus.Aberta;
            Validar();
        }

        public void Iniciar(string alteradoPor)
        {
            Clear();
            if (Status != EWfTarefaStatus.Aberta)
            {
                AddNotification(nameof(Status), "A tarefa só pode ser iniciada a partir do estado Aberta [Origem: WfTarefa]");
                return;
            }
            Status = EWfTarefaStatus.EmExecucao;
            MarcarAlterado(alteradoPor);
        }

        public void Concluir(string alteradoPor)
        {
            Clear();
            if (Status == EWfTarefaStatus.Concluida || Status == EWfTarefaStatus.Cancelada)
            {
                AddNotification(nameof(Status), "A tarefa já foi finalizada [Origem: WfTarefa]");
                return;
            }
            Status = EWfTarefaStatus.Concluida;
            ConcluidaEm = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void Cancelar(string alteradoPor)
        {
            Clear();
            if (Status == EWfTarefaStatus.Concluida || Status == EWfTarefaStatus.Cancelada)
            {
                AddNotification(nameof(Status), "A tarefa já foi finalizada [Origem: WfTarefa]");
                return;
            }
            Status = EWfTarefaStatus.Cancelada;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<WfTarefa>()
                .Requires()
                .AreNotEquals(InstanciaId, Guid.Empty, nameof(InstanciaId), "A instância da tarefa é obrigatória [Origem: WfTarefa]")
                .IsNotNullOrEmpty(Titulo, nameof(Titulo), "O título da tarefa humana é obrigatório [Origem: WfTarefa]"));
        }
    }
}
