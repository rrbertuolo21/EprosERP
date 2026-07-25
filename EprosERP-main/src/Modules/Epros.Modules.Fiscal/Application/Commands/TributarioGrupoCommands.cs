using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarTributarioGrupoCommand(string Descricao) : ICommand;
    public record AtualizarTributarioGrupoCommand(Guid Id, string Descricao) : ICommand;
    public record DeletarTributarioGrupoCommand(Guid Id) : ICommand;

    public class CriarTributarioGrupoCommandValidator : AbstractValidator<CriarTributarioGrupoCommand>
    {
        public CriarTributarioGrupoCommandValidator()
        {
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(100).WithMessage("A descrição deve possuir no máximo 100 caracteres.");
        }
    }

    public class AtualizarTributarioGrupoCommandValidator : AbstractValidator<AtualizarTributarioGrupoCommand>
    {
        public AtualizarTributarioGrupoCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID é obrigatório.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descrição é obrigatória.")
                .MaximumLength(100).WithMessage("A descrição deve possuir no máximo 100 caracteres.");
        }
    }
}
