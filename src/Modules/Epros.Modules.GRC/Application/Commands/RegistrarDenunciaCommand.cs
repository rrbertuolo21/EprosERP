using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    public record RegistrarDenunciaCommand(
        string Relato
    ) : ICommand;

    public class RegistrarDenunciaCommandValidator : AbstractValidator<RegistrarDenunciaCommand>
    {
        public RegistrarDenunciaCommandValidator()
        {
            RuleFor(c => c.Relato).NotEmpty().WithMessage("O relato detalhado da denúncia é obrigatório.");
        }
    }
}
