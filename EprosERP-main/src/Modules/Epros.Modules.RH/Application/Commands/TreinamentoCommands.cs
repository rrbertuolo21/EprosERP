using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    // RH-LMS — Treinamento e Certificacoes.

    public record CriarTreinamentoCommand(
        string Titulo,
        string? Descricao,
        Guid TipoTreinamentoId,
        Guid TreinadorId,
        Guid FilialId,
        Guid DepartamentoId,
        DateTime DataInicio,
        DateTime DataFim,
        TimeSpan HoraInicio,
        TimeSpan HoraFim,
        string? Local,
        int? CapacidadeMaxima,
        decimal? Custo,
        Guid CriadoPorUsuarioId,
        Guid DonoFuncionalId
    ) : ICommand;

    public class CriarTreinamentoCommandValidator : AbstractValidator<CriarTreinamentoCommand>
    {
        public CriarTreinamentoCommandValidator()
        {
            RuleFor(c => c.Titulo).NotEmpty().WithMessage("O titulo do treinamento e obrigatorio (LMS secao 16).");
            RuleFor(c => c.TipoTreinamentoId).NotEmpty();
            RuleFor(c => c.TreinadorId).NotEmpty();
            RuleFor(c => c.DataFim).GreaterThanOrEqualTo(c => c.DataInicio)
                .WithMessage("A data final deve ser maior ou igual a data inicial (LMS secao 16).");
            RuleFor(c => c.HoraFim).GreaterThan(c => c.HoraInicio)
                .WithMessage("A hora final deve ser maior que a hora inicial (LMS secao 16).");
        }
    }

    public record ConcluirTarefaTreinamentoCommand(Guid TarefaId) : ICommand;

    public record RegistrarFeedbackTarefaCommand(
        Guid TarefaId,
        Guid UsuarioAlvoId,
        int Nota,
        string? Comentarios,
        Guid CriadoPorUsuarioId,
        Guid DonoFuncionalId
    ) : ICommand;

    public class RegistrarFeedbackTarefaCommandValidator : AbstractValidator<RegistrarFeedbackTarefaCommand>
    {
        public RegistrarFeedbackTarefaCommandValidator()
        {
            RuleFor(c => c.TarefaId).NotEmpty();
            RuleFor(c => c.Nota).InclusiveBetween(1, 5).WithMessage("A nota do feedback deve estar entre 1 e 5 (LMS secao 19).");
        }
    }
}
