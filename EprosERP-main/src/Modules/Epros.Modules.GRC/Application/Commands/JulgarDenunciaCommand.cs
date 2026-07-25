using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    public record JulgarDenunciaCommand(
        Guid DenunciaId,
        string StatusFinal, // Procedente, Improcedente
        string ParecerFinal
    ) : ICommand;

    public class JulgarDenunciaCommandValidator : AbstractValidator<JulgarDenunciaCommand>
    {
        public JulgarDenunciaCommandValidator()
        {
            RuleFor(c => c.DenunciaId).NotEmpty().WithMessage("O ID da denúncia é obrigatório.");
            RuleFor(c => c.StatusFinal).Must(s => s == "Procedente" || s == "Improcedente")
                .WithMessage("O status final deve ser 'Procedente' ou 'Improcedente'.");
            RuleFor(c => c.ParecerFinal).NotEmpty().WithMessage("O parecer final é obrigatório.");
        }
    }
}
