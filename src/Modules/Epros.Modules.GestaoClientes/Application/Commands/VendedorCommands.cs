using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarVendedorCommand(
        string Nome,
        string Email,
        string? Telefone,
        decimal PercentualComissao,
        Guid? RevendaId = null,
        bool Ativo = true
    ) : ICommand;

    public class CriarVendedorCommandValidator : AbstractValidator<CriarVendedorCommand>
    {
        public CriarVendedorCommandValidator()
        {
            RuleFor(v => v.Nome)
                .NotEmpty().WithMessage("O Nome do vendedor é obrigatório.");

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("O E-mail do vendedor é obrigatório.")
                .EmailAddress().WithMessage("O E-mail fornecido não é válido.");

            RuleFor(v => v.PercentualComissao)
                .GreaterThanOrEqualTo(0).WithMessage("O Percentual de comissão deve ser maior ou igual a zero.");
        }
    }

    public record AtualizarVendedorCommand(
        Guid Id,
        string Nome,
        string Email,
        string? Telefone,
        decimal PercentualComissao,
        Guid? RevendaId,
        bool Ativo
    ) : ICommand;

    public class AtualizarVendedorCommandValidator : AbstractValidator<AtualizarVendedorCommand>
    {
        public AtualizarVendedorCommandValidator()
        {
            RuleFor(v => v.Id)
                .NotEmpty().WithMessage("O ID do vendedor é obrigatório.");

            RuleFor(v => v.Nome)
                .NotEmpty().WithMessage("O Nome do vendedor é obrigatório.");

            RuleFor(v => v.Email)
                .NotEmpty().WithMessage("O E-mail do vendedor é obrigatório.")
                .EmailAddress().WithMessage("O E-mail fornecido não é válido.");

            RuleFor(v => v.PercentualComissao)
                .GreaterThanOrEqualTo(0).WithMessage("O Percentual de comissão deve ser maior ou igual a zero.");
        }
    }

    public record ExcluirVendedorCommand(Guid Id) : ICommand;
}
