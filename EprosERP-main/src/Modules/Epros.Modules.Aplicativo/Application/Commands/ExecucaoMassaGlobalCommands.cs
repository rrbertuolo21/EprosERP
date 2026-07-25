using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record CriarExecucaoMassaGlobalCommand(
        string Descricao,
        string ActionPayload
    ) : ICommand;

    public class CriarExecucaoMassaGlobalCommandValidator : AbstractValidator<CriarExecucaoMassaGlobalCommand>
    {
        public CriarExecucaoMassaGlobalCommandValidator()
        {
            RuleFor(x => x.Descricao)
                .NotEmpty().WithMessage("A descrição é obrigatória.");

            RuleFor(x => x.ActionPayload)
                .NotEmpty().WithMessage("O payload da ação é obrigatório.");
        }
    }

    public record AtivarExecucaoMassaGlobalCommand(
        Guid ExecucaoMassaGlobalId
    ) : ICommand;

    public class AtivarExecucaoMassaGlobalCommandValidator : AbstractValidator<AtivarExecucaoMassaGlobalCommand>
    {
        public AtivarExecucaoMassaGlobalCommandValidator()
        {
            RuleFor(x => x.ExecucaoMassaGlobalId)
                .NotEmpty().WithMessage("O ID da execução em massa é obrigatório.");
        }
    }

    public record ConcluirExecucaoMassaGlobalCommand(
        Guid ExecucaoMassaGlobalId
    ) : ICommand;

    public class ConcluirExecucaoMassaGlobalCommandValidator : AbstractValidator<ConcluirExecucaoMassaGlobalCommand>
    {
        public ConcluirExecucaoMassaGlobalCommandValidator()
        {
            RuleFor(x => x.ExecucaoMassaGlobalId)
                .NotEmpty().WithMessage("O ID da execução em massa é obrigatório.");
        }
    }
}
