using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record IniciarProducaoCommand(
        Guid OrdemProducaoId
    ) : ICommand;

    public class IniciarProducaoCommandValidator : AbstractValidator<IniciarProducaoCommand>
    {
        public IniciarProducaoCommandValidator()
        {
            RuleFor(c => c.OrdemProducaoId)
                .NotEmpty().WithMessage("O ID da ordem de produção é obrigatório.");
        }
    }
}
