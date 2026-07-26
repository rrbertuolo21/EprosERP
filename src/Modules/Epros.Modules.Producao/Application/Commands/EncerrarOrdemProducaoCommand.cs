using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record EncerrarOrdemProducaoCommand(
        Guid OrdemProducaoId
    ) : ICommand;

    public class EncerrarOrdemProducaoCommandValidator : AbstractValidator<EncerrarOrdemProducaoCommand>
    {
        public EncerrarOrdemProducaoCommandValidator()
        {
            RuleFor(c => c.OrdemProducaoId)
                .NotEmpty().WithMessage("O ID da ordem de produção é obrigatório.");
        }
    }
}
