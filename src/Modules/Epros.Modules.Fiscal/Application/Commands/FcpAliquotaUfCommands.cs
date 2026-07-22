using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarFcpAliquotaUfCommand(EEstado Uf, decimal ValorAliquota, string? Observacao) : ICommand;
    public record AtualizarFcpAliquotaUfCommand(Guid Id, EEstado Uf, decimal ValorAliquota, string? Observacao) : ICommand;
    public record DeletarFcpAliquotaUfCommand(Guid Id) : ICommand;

    public class CriarFcpAliquotaUfCommandValidator : AbstractValidator<CriarFcpAliquotaUfCommand>
    {
        public CriarFcpAliquotaUfCommandValidator()
        {
            RuleFor(c => c.Uf).IsInEnum().WithMessage("UF inválida.");
            RuleFor(c => c.ValorAliquota).GreaterThanOrEqualTo(0).WithMessage("O valor da alíquota deve ser maior ou igual a zero.");
        }
    }

    public class AtualizarFcpAliquotaUfCommandValidator : AbstractValidator<AtualizarFcpAliquotaUfCommand>
    {
        public AtualizarFcpAliquotaUfCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID é obrigatório.");
            RuleFor(c => c.Uf).IsInEnum().WithMessage("UF inválida.");
            RuleFor(c => c.ValorAliquota).GreaterThanOrEqualTo(0).WithMessage("O valor da alíquota deve ser maior ou igual a zero.");
        }
    }
}
