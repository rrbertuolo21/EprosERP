using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarCestCommand(string Codigo, string Descricao) : ICommand;
    public record AtualizarCestCommand(Guid Id, string Codigo, string Descricao) : ICommand;
    public record DeletarCestCommand(Guid Id) : ICommand;

    public class CriarCestCommandValidator : AbstractValidator<CriarCestCommand>
    {
        public CriarCestCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código CEST é obrigatório.")
                .MaximumLength(7).WithMessage("O código CEST deve possuir no máximo 7 caracteres.");
            RuleFor(c => c.Descricao).MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");
        }
    }

    public class AtualizarCestCommandValidator : AbstractValidator<AtualizarCestCommand>
    {
        public AtualizarCestCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID do CEST é obrigatório.");
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código CEST é obrigatório.")
                .MaximumLength(7).WithMessage("O código CEST deve possuir no máximo 7 caracteres.");
            RuleFor(c => c.Descricao).MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");
        }
    }
}
