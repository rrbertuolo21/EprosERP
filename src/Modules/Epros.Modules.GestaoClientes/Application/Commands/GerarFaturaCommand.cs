using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record GerarFaturaCommand(
        Guid ClienteId,
        decimal Valor,
        DateTime DataVencimento
    ) : ICommand;

    public class GerarFaturaCommandValidator : AbstractValidator<GerarFaturaCommand>
    {
        public GerarFaturaCommandValidator()
        {
            RuleFor(f => f.ClienteId)
                .NotEmpty().WithMessage("O ID do Cliente é obrigatório.");

            RuleFor(f => f.Valor)
                .GreaterThan(0).WithMessage("O Valor da fatura deve ser maior que zero.");

            RuleFor(f => f.DataVencimento)
                .NotEmpty().WithMessage("A Data de Vencimento é obrigatória.")
                .Must(dt => dt > DateTime.UtcNow.AddMinutes(-5)).WithMessage("A Data de Vencimento não pode ser no passado.");
        }
    }
}
