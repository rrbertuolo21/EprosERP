using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Manutencao.Application.Commands
{
    public record AbrirOrdemManutencaoCommand(
        Guid EquipamentoId,
        string Tipo, // Corretiva, Preventiva, Preditiva
        string DescricaoProblema
    ) : ICommand;

    public class AbrirOrdemManutencaoCommandValidator : AbstractValidator<AbrirOrdemManutencaoCommand>
    {
        public AbrirOrdemManutencaoCommandValidator()
        {
            RuleFor(c => c.EquipamentoId)
                .NotEmpty().WithMessage("O ID do equipamento é obrigatório.");

            RuleFor(c => c.Tipo)
                .Must(t => t == "Corretiva" || t == "Preventiva" || t == "Preditiva")
                .WithMessage("O Tipo de OS deve ser 'Corretiva', 'Preventiva' ou 'Preditiva'.");

            RuleFor(c => c.DescricaoProblema)
                .NotEmpty().WithMessage("A descrição do problema é obrigatória.");
        }
    }
}
