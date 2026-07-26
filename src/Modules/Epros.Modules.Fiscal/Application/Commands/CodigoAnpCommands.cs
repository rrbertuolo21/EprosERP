using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarCodigoAnpCommand(string Codigo, string Descricao, DateTime DataInicioVigencia, DateTime? DataFinalVigencia) : ICommand;
    public record AtualizarCodigoAnpCommand(Guid Id, string Codigo, string Descricao, DateTime DataInicioVigencia, DateTime? DataFinalVigencia) : ICommand;
    public record DeletarCodigoAnpCommand(Guid Id) : ICommand;

    public class CriarCodigoAnpCommandValidator : AbstractValidator<CriarCodigoAnpCommand>
    {
        public CriarCodigoAnpCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código ANP é obrigatório.")
                .MaximumLength(20).WithMessage("O código ANP deve possuir no máximo 20 caracteres.");
            RuleFor(c => c.Descricao).MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");
        }
    }

    public class AtualizarCodigoAnpCommandValidator : AbstractValidator<AtualizarCodigoAnpCommand>
    {
        public AtualizarCodigoAnpCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID do código ANP é obrigatório.");
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código ANP é obrigatório.")
                .MaximumLength(20).WithMessage("O código ANP deve possuir no máximo 20 caracteres.");
            RuleFor(c => c.Descricao).MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");
        }
    }
}
