using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record AprovarExecucaoMassaCommand(
        Guid ExecucaoMassaId
    ) : ICommand;

    public class AprovarExecucaoMassaCommandValidator : AbstractValidator<AprovarExecucaoMassaCommand>
    {
        public AprovarExecucaoMassaCommandValidator()
        {
            RuleFor(c => c.ExecucaoMassaId)
                .NotEmpty().WithMessage("O ExecucaoMassaId é obrigatório.");
        }
    }
}
