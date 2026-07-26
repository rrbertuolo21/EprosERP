using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record AprovarAssinaturaManualCommand(
        Guid ClienteId,
        DateTime DataInicio,
        DateTime? DataFim,
        int DiaVencimento,
        decimal ValorRecorrente,
        Guid? FaturaPendenteIdParaBaixar,
        string Operador,
        string Justificativa
    ) : ICommand;

    public class AprovarAssinaturaManualCommandValidator : AbstractValidator<AprovarAssinaturaManualCommand>
    {
        public AprovarAssinaturaManualCommandValidator()
        {
            RuleFor(c => c.ClienteId)
                .NotEmpty().WithMessage("O ClienteId é obrigatório.");

            RuleFor(c => c.DataInicio)
                .NotEmpty().WithMessage("A data de início é obrigatória.");

            RuleFor(c => c.DiaVencimento)
                .InclusiveBetween(1, 28).WithMessage("O dia de vencimento deve estar entre 1 e 28.");

            RuleFor(c => c.ValorRecorrente)
                .GreaterThan(0).WithMessage("O valor recorrente deve ser maior que zero.");

            RuleFor(c => c.Operador)
                .NotEmpty().WithMessage("O nome do operador é obrigatório.");

            RuleFor(c => c.Justificativa)
                .NotEmpty().WithMessage("A justificativa de aprovação manual é obrigatória.")
                .MinimumLength(10).WithMessage("A justificativa de aprovação manual deve ter no mínimo 10 caracteres.");
        }
    }
}
