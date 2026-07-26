using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarIcmsAliquotaInterestadualCommand(EEstado UfOrigem, EEstado UfDestino, decimal ValorAliquota) : ICommand;
    public record AtualizarIcmsAliquotaInterestadualCommand(Guid Id, EEstado UfOrigem, EEstado UfDestino, decimal ValorAliquota) : ICommand;
    public record DeletarIcmsAliquotaInterestadualCommand(Guid Id) : ICommand;

    public class CriarIcmsAliquotaInterestadualCommandValidator : AbstractValidator<CriarIcmsAliquotaInterestadualCommand>
    {
        public CriarIcmsAliquotaInterestadualCommandValidator()
        {
            RuleFor(c => c.UfOrigem).IsInEnum().WithMessage("UF de origem inválida.");
            RuleFor(c => c.UfDestino).IsInEnum().WithMessage("UF de destino inválida.");
            RuleFor(c => c.ValorAliquota).GreaterThanOrEqualTo(0).WithMessage("O valor da alíquota deve ser maior ou igual a zero.");
        }
    }

    public class AtualizarIcmsAliquotaInterestadualCommandValidator : AbstractValidator<AtualizarIcmsAliquotaInterestadualCommand>
    {
        public AtualizarIcmsAliquotaInterestadualCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID é obrigatório.");
            RuleFor(c => c.UfOrigem).IsInEnum().WithMessage("UF de origem inválida.");
            RuleFor(c => c.UfDestino).IsInEnum().WithMessage("UF de destino inválida.");
            RuleFor(c => c.ValorAliquota).GreaterThanOrEqualTo(0).WithMessage("O valor da alíquota deve ser maior ou igual a zero.");
        }
    }
}
