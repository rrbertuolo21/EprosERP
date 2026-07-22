using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarRevendaCommand(
        string Nome,
        decimal PercentualComissao,
        bool Ativo = true
    ) : ICommand;

    public class CriarRevendaCommandValidator : AbstractValidator<CriarRevendaCommand>
    {
        public CriarRevendaCommandValidator()
        {
            RuleFor(r => r.Nome)
                .NotEmpty().WithMessage("O Nome da revenda é obrigatório.");

            RuleFor(r => r.PercentualComissao)
                .GreaterThanOrEqualTo(0).WithMessage("O Percentual de comissão deve ser maior ou igual a zero.");
        }
    }

    public record AtualizarRevendaCommand(
        Guid Id,
        string Nome,
        decimal PercentualComissao,
        bool Ativo
    ) : ICommand;

    public class AtualizarRevendaCommandValidator : AbstractValidator<AtualizarRevendaCommand>
    {
        public AtualizarRevendaCommandValidator()
        {
            RuleFor(r => r.Id)
                .NotEmpty().WithMessage("O ID da revenda é obrigatório.");

            RuleFor(r => r.Nome)
                .NotEmpty().WithMessage("O Nome da revenda é obrigatório.");

            RuleFor(r => r.PercentualComissao)
                .GreaterThanOrEqualTo(0).WithMessage("O Percentual de comissão deve ser maior ou igual a zero.");
        }
    }

    public record ExcluirRevendaCommand(Guid Id) : ICommand;
}
