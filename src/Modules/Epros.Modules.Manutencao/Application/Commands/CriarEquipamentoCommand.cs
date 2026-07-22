using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Manutencao.Application.Commands
{
    public record CriarEquipamentoCommand(
        string Nome,
        string Codigo,
        string Setor,
        DateTime DataAquisicao,
        string Criticidade
    ) : ICommand;

    public class CriarEquipamentoCommandValidator : AbstractValidator<CriarEquipamentoCommand>
    {
        public CriarEquipamentoCommandValidator()
        {
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("O Nome do equipamento é obrigatório.");

            RuleFor(c => c.Codigo)
                .NotEmpty().WithMessage("O Código do equipamento é obrigatório.");

            RuleFor(c => c.Setor)
                .NotEmpty().WithMessage("O Setor é obrigatório.");

            RuleFor(c => c.Criticidade)
                .Must(crit => crit == "Alta" || crit == "Media" || crit == "Baixa")
                .WithMessage("A Criticidade deve ser 'Alta', 'Media' ou 'Baixa'.");
        }
    }
}
