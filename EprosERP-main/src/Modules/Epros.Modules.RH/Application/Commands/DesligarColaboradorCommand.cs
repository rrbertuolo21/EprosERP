using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    public record DesligarColaboradorCommand(
        Guid ColaboradorId,
        DateTime DataDemissao
    ) : ICommand;

    public class DesligarColaboradorCommandValidator : AbstractValidator<DesligarColaboradorCommand>
    {
        public DesligarColaboradorCommandValidator()
        {
            RuleFor(c => c.ColaboradorId)
                .NotEmpty().WithMessage("O ID do colaborador é obrigatório.");

            RuleFor(c => c.DataDemissao)
                .NotEmpty().WithMessage("A data de demissão é obrigatória.");
        }
    }
}
