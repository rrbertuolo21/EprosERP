using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>
    /// Comando para solicitar o cancelamento de uma nota fiscal de compra já lançada.
    /// Exige justificativa mínima de 10 caracteres.
    /// </summary>
    public record CancelarCompraCommand(
        Guid CompraId,
        string Motivo
    ) : ICommand;

    public class CancelarCompraCommandValidator : AbstractValidator<CancelarCompraCommand>
    {
        public CancelarCompraCommandValidator()
        {
            RuleFor(c => c.CompraId)
                .NotEmpty().WithMessage("O ID da compra é obrigatório.");

            RuleFor(c => c.Motivo)
                .NotEmpty().WithMessage("O motivo do cancelamento é obrigatório.")
                .MinimumLength(10).WithMessage("O motivo do cancelamento deve possuir pelo menos 10 caracteres.");
        }
    }
}
