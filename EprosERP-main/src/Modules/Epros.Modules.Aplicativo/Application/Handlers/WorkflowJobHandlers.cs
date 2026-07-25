using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.Aplicativo.Domain.Entities.Workflow;
using Epros.Modules.Aplicativo.Domain.Enums;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    /// <summary>
    /// Handlers da camada de agendamentos (§7.4) e fila de jobs (§7.5/§8.3) do motor de Workflow.
    /// A política de retry vive aqui, separada da regra de domínio executada (§7.5.10 / WF-CA-012).
    /// </summary>
    public class WorkflowJobHandlers :
        ICommandHandler<CriarWfAgendamentoCommand>,
        ICommandHandler<AlterarWfAgendamentoCommand>,
        ICommandHandler<AtivarWfAgendamentoCommand>,
        ICommandHandler<DesativarWfAgendamentoCommand>,
        ICommandHandler<EnfileirarJobsAgendadosCommand>,
        ICommandHandler<IniciarWfJobCommand>,
        ICommandHandler<ResolverWfJobSucessoCommand>,
        ICommandHandler<ResolverWfJobFalhaCommand>
    {
        /// <summary>Máximo de tentativas antes da falha final (política de retry).</summary>
        public const int MaxTentativas = 3;

        private readonly ContextAplicativo _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;
        private readonly IAgendaIntervalarService _agenda;

        public WorkflowJobHandlers(
            ContextAplicativo context,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser,
            IAgendaIntervalarService agenda)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
            _agenda = agenda;
        }

        private string Tenant() => _tenantProvider.GetTenantId();
        private string User() => _currentUser.GetUserId() ?? "system";

        // ---------- Agendamento ----------

        public async Task<CommandResult> Handle(CriarWfAgendamentoCommand request, CancellationToken ct)
        {
            if (!_agenda.ExpressaoValida(request.ExpressaoIntervalar, out var erro))
                return CommandResult.Falha(erro ?? "Expressão intervalar inválida.");

            var proxima = request.Ativo ? _agenda.ProximaExecucao(request.ExpressaoIntervalar, DateTime.UtcNow) : null;
            var agendamento = new WfAgendamento(request.Nome, request.ExpressaoIntervalar, request.Ativo, proxima, Tenant(), User());
            if (!agendamento.IsValid) return CommandResult.Falha(agendamento.Notifications.Select(n => n.Message));

            _context.WfAgendamentos.Add(agendamento);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Agendamento criado.", new { agendamento.Id, agendamento.ProximaExecucaoEm });
        }

        public async Task<CommandResult> Handle(AlterarWfAgendamentoCommand request, CancellationToken ct)
        {
            if (!_agenda.ExpressaoValida(request.ExpressaoIntervalar, out var erro))
                return CommandResult.Falha(erro ?? "Expressão intervalar inválida.");

            var agendamento = await _context.WfAgendamentos.FirstOrDefaultAsync(a => a.Id == request.AgendamentoId && a.DeletadoEm == null, ct);
            if (agendamento == null) return CommandResult.Falha("Agendamento não encontrado.");

            agendamento.Alterar(request.Nome, request.ExpressaoIntervalar, request.Ativo, User());
            if (!agendamento.IsValid) return CommandResult.Falha(agendamento.Notifications.Select(n => n.Message));

            var proxima = request.Ativo ? _agenda.ProximaExecucao(request.ExpressaoIntervalar, DateTime.UtcNow) : null;
            agendamento.AtualizarProximaExecucao(proxima, User());

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Agendamento atualizado.", new { agendamento.Id, agendamento.ProximaExecucaoEm });
        }

        public async Task<CommandResult> Handle(AtivarWfAgendamentoCommand request, CancellationToken ct)
        {
            var agendamento = await _context.WfAgendamentos.FirstOrDefaultAsync(a => a.Id == request.AgendamentoId && a.DeletadoEm == null, ct);
            if (agendamento == null) return CommandResult.Falha("Agendamento não encontrado.");

            var proxima = _agenda.ProximaExecucao(agendamento.ExpressaoIntervalar, DateTime.UtcNow);
            agendamento.Alterar(agendamento.Nome, agendamento.ExpressaoIntervalar, true, User());
            agendamento.AtualizarProximaExecucao(proxima, User());
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Agendamento ativado.");
        }

        public async Task<CommandResult> Handle(DesativarWfAgendamentoCommand request, CancellationToken ct)
        {
            var agendamento = await _context.WfAgendamentos.FirstOrDefaultAsync(a => a.Id == request.AgendamentoId && a.DeletadoEm == null, ct);
            if (agendamento == null) return CommandResult.Falha("Agendamento não encontrado.");

            agendamento.Alterar(agendamento.Nome, agendamento.ExpressaoIntervalar, false, User());
            agendamento.AtualizarProximaExecucao(null, User());
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Agendamento desativado.");
        }

        public async Task<CommandResult> Handle(EnfileirarJobsAgendadosCommand request, CancellationToken ct)
        {
            var referencia = request.ReferenciaUtc ?? DateTime.UtcNow;
            var user = User();
            var tenant = Tenant();

            // Somente agendas ativas e vencidas (§7.4.5).
            var agendas = await _context.WfAgendamentos
                .Where(a => a.DeletadoEm == null && a.Ativo && a.ProximaExecucaoEm != null && a.ProximaExecucaoEm <= referencia)
                .ToListAsync(ct);

            var enfileirados = 0;
            foreach (var agenda in agendas)
            {
                // Anti-duplicação (WF-CA-011): não enfileira se já existe job aberto para a mesma agenda.
                var jaPendente = await _context.WfJobs.AnyAsync(
                    j => j.AgendamentoId == agenda.Id && j.DeletadoEm == null &&
                         (j.Status == EWfJobStatus.Pendente || j.Status == EWfJobStatus.EmExecucao || j.Status == EWfJobStatus.Adiado), ct);

                if (!jaPendente)
                {
                    var job = new WfJob(agenda.Id, agenda.Nome, agenda.ProximaExecucaoEm ?? referencia, null, tenant, user);
                    if (job.IsValid)
                    {
                        _context.WfJobs.Add(job);
                        enfileirados++;
                    }
                }

                // Reprograma a próxima execução mesmo quando pulou por duplicidade (evita reprocessar o mesmo instante).
                var proxima = _agenda.ProximaExecucao(agenda.ExpressaoIntervalar, referencia);
                agenda.AtualizarProximaExecucao(proxima, user);
            }

            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok($"{enfileirados} job(s) enfileirado(s).", new { Enfileirados = enfileirados, AgendasAvaliadas = agendas.Count });
        }

        // ---------- Fila de jobs ----------

        public async Task<CommandResult> Handle(IniciarWfJobCommand request, CancellationToken ct)
        {
            var job = await _context.WfJobs.FirstOrDefaultAsync(j => j.Id == request.JobId && j.DeletadoEm == null, ct);
            if (job == null) return CommandResult.Falha("Job não encontrado.");
            if (job.Status != EWfJobStatus.Pendente && job.Status != EWfJobStatus.Adiado)
                return CommandResult.Falha("Só é possível iniciar job Pendente ou Adiado.");

            job.Iniciar(User());
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Job iniciado.", new { job.Id, job.TentativaAtual });
        }

        public async Task<CommandResult> Handle(ResolverWfJobSucessoCommand request, CancellationToken ct)
        {
            var job = await _context.WfJobs.FirstOrDefaultAsync(j => j.Id == request.JobId && j.DeletadoEm == null, ct);
            if (job == null) return CommandResult.Falha("Job não encontrado.");

            RegistrarTentativa(job, EWfJobTentativaStatus.Sucesso, request.Log);
            job.ResolverSucesso(request.Log, User());
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Job concluído com sucesso.");
        }

        public async Task<CommandResult> Handle(ResolverWfJobFalhaCommand request, CancellationToken ct)
        {
            var job = await _context.WfJobs.FirstOrDefaultAsync(j => j.Id == request.JobId && j.DeletadoEm == null, ct);
            if (job == null) return CommandResult.Falha("Job não encontrado.");

            var user = User();
            if (job.TentativaAtual < MaxTentativas)
            {
                // Ainda há tentativa: registra retry e adia com backoff exponencial (§7.5.4-5 / WF-CA-012).
                RegistrarTentativa(job, EWfJobTentativaStatus.Retry, request.Log);
                var backoffMinutos = (int)Math.Pow(2, job.TentativaAtual) * 5; // 10, 20, 40...
                job.Adiar(DateTime.UtcNow.AddMinutes(backoffMinutos), user);
                await _context.SaveChangesAsync(ct);
                return CommandResult.Ok("Falha registrada; job reprogramado (retry).", new { job.Id, job.TentativaAtual, ReprogramadoPara = job.PrevistoPara });
            }

            // Esgotou tentativas: falha final (§7.5.6).
            RegistrarTentativa(job, EWfJobTentativaStatus.FalhaFinal, request.Log);
            job.FalhaFinal(request.Log, user);
            await _context.SaveChangesAsync(ct);
            return CommandResult.Ok("Falha final registrada; job encerrado.");
        }

        private void RegistrarTentativa(WfJob job, EWfJobTentativaStatus status, string? mensagem)
        {
            var numero = job.TentativaAtual <= 0 ? 1 : job.TentativaAtual;
            var tentativa = new WfJobTentativa(job.Id, numero, status, mensagem, job.IniciadoEm, DateTime.UtcNow, Tenant(), User());
            if (tentativa.IsValid) _context.WfJobTentativas.Add(tentativa);
        }
    }
}
