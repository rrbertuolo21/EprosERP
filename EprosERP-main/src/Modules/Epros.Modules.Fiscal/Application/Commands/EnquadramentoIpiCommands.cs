using System;
using Epros.Modules.Fiscal.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarEnquadramentoIpiCommand(string Codigo, string Descricao, ETipoOperacaoEnquadramentoIpi TipoOperacao) : ICommand;
    public record AtualizarEnquadramentoIpiCommand(Guid Id, string Codigo, string Descricao, ETipoOperacaoEnquadramentoIpi TipoOperacao) : ICommand;
    public record DeletarEnquadramentoIpiCommand(Guid Id) : ICommand;

    public class CriarEnquadramentoIpiCommandValidator : AbstractValidator<CriarEnquadramentoIpiCommand>
    {
        public CriarEnquadramentoIpiCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código é obrigatório.")
                .MaximumLength(3).WithMessage("O código deve possuir no máximo 3 caracteres.");
            RuleFor(c => c.Descricao).MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");
            RuleFor(c => c.TipoOperacao).IsInEnum().WithMessage("Tipo de operação inválido.");
        }
    }

    public class AtualizarEnquadramentoIpiCommandValidator : AbstractValidator<AtualizarEnquadramentoIpiCommand>
    {
        public AtualizarEnquadramentoIpiCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID é obrigatório.");
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código é obrigatório.")
                .MaximumLength(3).WithMessage("O código deve possuir no máximo 3 caracteres.");
            RuleFor(c => c.Descricao).MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");
            RuleFor(c => c.TipoOperacao).IsInEnum().WithMessage("Tipo de operação inválido.");
        }
    }
}
