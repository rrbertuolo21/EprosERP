using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    public record RegistrarTimesheetCommand(
        Guid ColaboradorId,
        DateTime Data,
        decimal HorasTrabalhadas,
        string DescricaoAtividade
    ) : ICommand;

    public class RegistrarTimesheetCommandValidator : AbstractValidator<RegistrarTimesheetCommand>
    {
        public RegistrarTimesheetCommandValidator()
        {
            RuleFor(c => c.ColaboradorId)
                .NotEmpty().WithMessage("O ID do colaborador é obrigatório.");

            RuleFor(c => c.Data)
                .NotEmpty().WithMessage("A data é obrigatória.");

            RuleFor(c => c.HorasTrabalhadas)
                .GreaterThan(0).WithMessage("As horas trabalhadas devem ser maiores que zero.")
                .LessThanOrEqualTo(24).WithMessage("As horas trabalhadas não podem passar de 24 horas em um único dia.");

            RuleFor(c => c.DescricaoAtividade)
                .NotEmpty().WithMessage("A descrição da atividade é obrigatória.");
        }
    }
}
