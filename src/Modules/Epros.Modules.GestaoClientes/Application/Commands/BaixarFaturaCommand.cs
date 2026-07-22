using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record BaixarFaturaCommand(
        Guid FaturaId
    ) : ICommand;

    public class BaixarFaturaCommandValidator : AbstractValidator<BaixarFaturaCommand>
    {
        public BaixarFaturaCommandValidator()
        {
            RuleFor(f => f.FaturaId)
                .NotEmpty().WithMessage("O ID da Fatura é obrigatório.");
        }
    }
}
