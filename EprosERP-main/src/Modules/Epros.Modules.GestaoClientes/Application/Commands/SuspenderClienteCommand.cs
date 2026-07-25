using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record SuspenderClienteCommand(Guid ClienteId) : ICommand;

    public class SuspenderClienteCommandValidator : AbstractValidator<SuspenderClienteCommand>
    {
        public SuspenderClienteCommandValidator()
        {
            RuleFor(c => c.ClienteId)
                .NotEmpty().WithMessage("O ID do cliente é obrigatório para suspensão.");
        }
    }
}
