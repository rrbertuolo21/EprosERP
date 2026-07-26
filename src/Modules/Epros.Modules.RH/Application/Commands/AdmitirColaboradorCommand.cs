using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    public record AdmitirColaboradorCommand(
        string Nome,
        string Cpf,
        string Email,
        string Cargo,
        string Departamento,
        decimal SalarioBase,
        DateTime DataAdmissao
    ) : ICommand;

    public class AdmitirColaboradorCommandValidator : AbstractValidator<AdmitirColaboradorCommand>
    {
        public AdmitirColaboradorCommandValidator()
        {
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("O Nome do colaborador é obrigatório.");

            RuleFor(c => c.Cpf)
                .NotEmpty().WithMessage("O CPF do colaborador é obrigatório.");

            RuleFor(c => c.Email)
                .NotEmpty().WithMessage("O E-mail é obrigatório.")
                .EmailAddress().WithMessage("E-mail em formato inválido.");

            RuleFor(c => c.Cargo)
                .NotEmpty().WithMessage("O Cargo é obrigatório.");

            RuleFor(c => c.Departamento)
                .NotEmpty().WithMessage("O Departamento é obrigatório.");

            RuleFor(c => c.SalarioBase)
                .GreaterThan(0).WithMessage("O Salário Base deve ser maior que zero.");
        }
    }
}
