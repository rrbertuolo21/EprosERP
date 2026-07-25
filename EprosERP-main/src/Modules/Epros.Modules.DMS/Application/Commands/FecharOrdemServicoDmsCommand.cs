using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record FecharOrdemServicoDmsCommand(
        Guid Id
    ) : ICommand;

    public class FecharOrdemServicoDmsCommandValidator : AbstractValidator<FecharOrdemServicoDmsCommand>
    {
        public FecharOrdemServicoDmsCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID da ordem de serviço é obrigatório.");
        }
    }
}
