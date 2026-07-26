using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Projetos.Application.Commands
{
    public record AtualizarProgressoTarefaCommand(
        Guid ProjetoId,
        Guid WbsItemId,
        decimal PercentualConclusao
    ) : ICommand;

    public class AtualizarProgressoTarefaCommandValidator : AbstractValidator<AtualizarProgressoTarefaCommand>
    {
        public AtualizarProgressoTarefaCommandValidator()
        {
            RuleFor(c => c.ProjetoId)
                .NotEmpty().WithMessage("O ID do projeto é obrigatório.");

            RuleFor(c => c.WbsItemId)
                .NotEmpty().WithMessage("O ID da tarefa WBS é obrigatório.");

            RuleFor(c => c.PercentualConclusao)
                .InclusiveBetween(0, 100).WithMessage("O percentual de conclusão deve ser entre 0 e 100.");
        }
    }
}
