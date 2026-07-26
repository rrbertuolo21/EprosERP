using System;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Aplicativo.Domain.Entities.Workflow
{
    /// <summary>
    /// wf_agendamento — agenda intervalar de jobs no formato mins::hours::day_of_month::months::day_of_week.
    /// [Origem: EF WORKFLOW 10.10]
    /// </summary>
    public class WfAgendamento : EntidadeSaaSBase
    {
        public string Nome { get; private set; } = string.Empty;
        public string ExpressaoIntervalar { get; private set; } = string.Empty;
        public bool Ativo { get; private set; }
        public DateTime? ProximaExecucaoEm { get; private set; }

        protected WfAgendamento() { }

        public WfAgendamento(string nome, string expressaoIntervalar, bool ativo, DateTime? proximaExecucaoEm, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Nome = nome;
            ExpressaoIntervalar = expressaoIntervalar;
            Ativo = ativo;
            ProximaExecucaoEm = proximaExecucaoEm;
            Validar();
        }

        public void Alterar(string nome, string expressaoIntervalar, bool ativo, string alteradoPor)
        {
            Nome = nome;
            ExpressaoIntervalar = expressaoIntervalar;
            Ativo = ativo;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void AtualizarProximaExecucao(DateTime? proximaExecucaoEm, string alteradoPor)
        {
            ProximaExecucaoEm = proximaExecucaoEm;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<WfAgendamento>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome da agenda é obrigatório [Origem: WfAgendamento]")
                .IsNotNullOrEmpty(ExpressaoIntervalar, nameof(ExpressaoIntervalar), "A expressão intervalar é obrigatória [Origem: WfAgendamento]"));

            // Formato funcional mins::hours::day_of_month::months::day_of_week (5 segmentos separados por '::')
            if (!string.IsNullOrWhiteSpace(ExpressaoIntervalar) && ExpressaoIntervalar.Split("::").Length != 5)
            {
                AddNotification(nameof(ExpressaoIntervalar), "A expressão intervalar deve ter 5 segmentos (mins::hours::day_of_month::months::day_of_week) [Origem: WfAgendamento]");
            }
        }
    }

    /// <summary>
    /// wf_job — execução de job agendado, com controle de tentativas e log. [Origem: EF WORKFLOW 10.11]
    /// </summary>
    public class WfJob : EntidadeSaaSBase
    {
        public Guid AgendamentoId { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public EWfJobStatus Status { get; private set; }
        public int TentativaAtual { get; private set; }
        public Guid? ContextoUsuarioId { get; private set; }
        public DateTime PrevistoPara { get; private set; }
        public DateTime? IniciadoEm { get; private set; }
        public DateTime? FinalizadoEm { get; private set; }
        public string? Log { get; private set; }

        protected WfJob() { }

        public WfJob(Guid agendamentoId, string nome, DateTime previstoPara, Guid? contextoUsuarioId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            AgendamentoId = agendamentoId;
            Nome = nome;
            PrevistoPara = previstoPara;
            ContextoUsuarioId = contextoUsuarioId;
            Status = EWfJobStatus.Pendente;
            TentativaAtual = 0;

            AddNotifications(new Contract<WfJob>()
                .Requires()
                .AreNotEquals(agendamentoId, Guid.Empty, nameof(AgendamentoId), "A agenda do job é obrigatória [Origem: WfJob]")
                .IsNotNullOrEmpty(nome, nameof(Nome), "O nome do job é obrigatório [Origem: WfJob]"));
        }

        public void Iniciar(string alteradoPor)
        {
            Status = EWfJobStatus.EmExecucao;
            IniciadoEm = DateTime.UtcNow;
            TentativaAtual++;
            MarcarAlterado(alteradoPor);
        }

        public void ResolverSucesso(string? log, string alteradoPor)
        {
            Status = EWfJobStatus.Sucesso;
            FinalizadoEm = DateTime.UtcNow;
            Log = log;
            MarcarAlterado(alteradoPor);
        }

        public void ResolverFalha(string? log, string alteradoPor)
        {
            Status = EWfJobStatus.Falha;
            FinalizadoEm = DateTime.UtcNow;
            Log = log;
            MarcarAlterado(alteradoPor);
        }

        public void Adiar(DateTime novaPrevisao, string alteradoPor)
        {
            Status = EWfJobStatus.Adiado;
            PrevistoPara = novaPrevisao;
            MarcarAlterado(alteradoPor);
        }

        public void FalhaFinal(string? log, string alteradoPor)
        {
            Status = EWfJobStatus.FalhaFinal;
            FinalizadoEm = DateTime.UtcNow;
            Log = log;
            MarcarAlterado(alteradoPor);
        }
    }

    /// <summary>
    /// wf_job_tentativa — tentativas, retry e falhas de um job. Preserva o histórico de tentativas. [Origem: EF WORKFLOW 10.12]
    /// </summary>
    public class WfJobTentativa : EntidadeSaaSBase
    {
        public Guid JobId { get; private set; }
        public int NumeroTentativa { get; private set; }
        public EWfJobTentativaStatus Status { get; private set; }
        public string? Mensagem { get; private set; }
        public DateTime? IniciadoEm { get; private set; }
        public DateTime? FinalizadoEm { get; private set; }

        protected WfJobTentativa() { }

        public WfJobTentativa(Guid jobId, int numeroTentativa, EWfJobTentativaStatus status, string? mensagem, DateTime? iniciadoEm, DateTime? finalizadoEm, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            JobId = jobId;
            NumeroTentativa = numeroTentativa;
            Status = status;
            Mensagem = mensagem;
            IniciadoEm = iniciadoEm;
            FinalizadoEm = finalizadoEm;

            AddNotifications(new Contract<WfJobTentativa>()
                .Requires()
                .AreNotEquals(jobId, Guid.Empty, nameof(JobId), "O job da tentativa é obrigatório [Origem: WfJobTentativa]")
                .IsGreaterThan(numeroTentativa, 0, nameof(NumeroTentativa), "O número da tentativa deve ser maior que zero [Origem: WfJobTentativa]"));
        }
    }
}
