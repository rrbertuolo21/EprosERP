using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Qualidade.Application.Commands
{
    public record RegistrarResultadoInspecaoCommand(
        Guid InspecaoLoteId,
        string Status,
        string Responsavel,
        string? Observacoes
    ) : ICommand;

    public class RegistrarResultadoInspecaoCommandValidator : AbstractValidator<RegistrarResultadoInspecaoCommand>
    {
        public RegistrarResultadoInspecaoCommandValidator()
        {
            RuleFor(c => c.InspecaoLoteId)
                .NotEmpty().WithMessage("O ID da inspeção do lote é obrigatório.");

            RuleFor(c => c.Status)
                .NotEmpty().WithMessage("O status da inspeção é obrigatório.")
                .Must(s => s == "Aprovado" || s == "Reprovado")
                .WithMessage("O status deve ser 'Aprovado' ou 'Reprovado'.");

            RuleFor(c => c.Responsavel)
                .NotEmpty().WithMessage("O responsável pela inspeção é obrigatório.");
        }
    }
}
