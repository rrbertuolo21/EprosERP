using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarPlanoCommand(
        string Nome,
        decimal Preco,
        List<string> Modulos
    ) : ICommand;

    public class CriarPlanoCommandValidator : AbstractValidator<CriarPlanoCommand>
    {
        public CriarPlanoCommandValidator()
        {
            RuleFor(p => p.Nome)
                .NotEmpty().WithMessage("O Nome do plano é obrigatório.");

            RuleFor(p => p.Preco)
                .GreaterThan(0).WithMessage("O Preço do plano deve ser maior que zero.");

            RuleFor(p => p.Modulos)
                .NotNull().WithMessage("A lista de módulos não pode ser nula.");
        }
    }
}
