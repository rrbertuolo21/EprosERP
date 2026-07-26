using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record AtivarClienteCommand(Guid ClienteId) : ICommand;

    public class AtivarClienteCommandValidator : AbstractValidator<AtivarClienteCommand>
    {
        public AtivarClienteCommandValidator()
        {
            RuleFor(c => c.ClienteId)
                .NotEmpty().WithMessage("O ID do cliente é obrigatório para ativação.");
        }
    }
}
