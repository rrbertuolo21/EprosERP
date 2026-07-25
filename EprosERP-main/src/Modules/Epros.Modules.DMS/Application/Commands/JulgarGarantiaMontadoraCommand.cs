using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record JulgarGarantiaMontadoraCommand(
        Guid Id,
        string NovoStatus, // Aprovada, Rejeitada
        string Parecer
    ) : ICommand;

    public class JulgarGarantiaMontadoraCommandValidator : AbstractValidator<JulgarGarantiaMontadoraCommand>
    {
        public JulgarGarantiaMontadoraCommandValidator()
        {
            RuleFor(c => c.Id).NotEmpty().WithMessage("O ID da garantia é obrigatório.");
            RuleFor(c => c.NovoStatus).Must(s => s == "Aprovada" || s == "Rejeitada").WithMessage("O status deve ser 'Aprovada' ou 'Rejeitada'.");
            RuleFor(c => c.Parecer).NotEmpty().WithMessage("O parecer da montadora é obrigatório.");
        }
    }
}
