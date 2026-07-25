using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    // ---------- Agendamento (wf_agendamento) — §7.4 ----------

    public record CriarWfAgendamentoCommand(string Nome, string ExpressaoIntervalar, bool Ativo) : ICommand;

    public class CriarWfAgendamentoCommandValidator : AbstractValidator<CriarWfAgendamentoCommand>
    {
        public CriarWfAgendamentoCommandValidator()
        {
            RuleFor(x => x.Nome).NotEmpty().WithMessage("O nome da agenda é obrigatório.");
            RuleFor(x => x.ExpressaoIntervalar).NotEmpty().WithMessage("A expressão intervalar é obrigatória.");
        }
    }

    public record AlterarWfAgendamentoCommand(Guid AgendamentoId, string Nome, string ExpressaoIntervalar, bool Ativo) : ICommand;

    public record AtivarWfAgendamentoCommand(Guid AgendamentoId) : ICommand;

    public record DesativarWfAgendamentoCommand(Guid AgendamentoId) : ICommand;

    /// <summary>
    /// Enfileira jobs para todas as agendas ativas cuja próxima execução já venceu, evitando duplicidade
    /// (WF-CA-011). Ponto de entrada acionável pelo job Quartz ou por rotina administrativa. §7.4.5-6 / §8.3
    /// </summary>
    public record EnfileirarJobsAgendadosCommand(DateTime? ReferenciaUtc = null) : ICommand;

    // ---------- Fila de jobs (wf_job) — §7.5 / §8.3 ----------

    /// <summary>Marca o início de execução de um job pendente/adiado, registrando contexto e tentativa.</summary>
    public record IniciarWfJobCommand(Guid JobId) : ICommand;

    /// <summary>Resolve um job como sucesso, registrando log e tentativa bem-sucedida.</summary>
    public record ResolverWfJobSucessoCommand(Guid JobId, string? Log) : ICommand;

    /// <summary>
    /// Resolve uma falha de job aplicando a política de retry: reprograma (adiamento com backoff) enquanto
    /// houver tentativa disponível; caso contrário encerra em falha final. §7.5.4-6 / WF-CA-012
    /// </summary>
    public record ResolverWfJobFalhaCommand(Guid JobId, string? Log) : ICommand;

    public class IniciarWfJobCommandValidator : AbstractValidator<IniciarWfJobCommand>
    {
        public IniciarWfJobCommandValidator() => RuleFor(x => x.JobId).NotEmpty().WithMessage("O job é obrigatório.");
    }
}
