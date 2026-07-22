using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record SimularExecucaoMassaGlobalCommand(
        Guid ExecucaoMassaGlobalId
    ) : ICommand;

    public class SimularExecucaoMassaGlobalCommandValidator : AbstractValidator<SimularExecucaoMassaGlobalCommand>
    {
        public SimularExecucaoMassaGlobalCommandValidator()
        {
            RuleFor(x => x.ExecucaoMassaGlobalId)
                .NotEmpty().WithMessage("O ID da execução em massa é obrigatório.");
        }
    }
}
