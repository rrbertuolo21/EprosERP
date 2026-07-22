using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarObservacaoNfeCommand(string Descricao) : ICommand;
    public record AtualizarObservacaoNfeCommand(Guid Id, string Descricao) : ICommand;
    public record DeletarObservacaoNfeCommand(Guid Id) : ICommand;

    public class CriarObservacaoNfeCommandValidator : AbstractValidator<CriarObservacaoNfeCommand>
    {
        public CriarObservacaoNfeCommandValidator()
        {
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(5000).WithMessage("A descrição deve possuir no máximo 5000 caracteres.");
        }
    }

    public class AtualizarObservacaoNfeCommandValidator : AbstractValidator<AtualizarObservacaoNfeCommand>
    {
        public AtualizarObservacaoNfeCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID é obrigatório.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(5000).WithMessage("A descrição deve possuir no máximo 5000 caracteres.");
        }
    }
}
