using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    public record FolhaPagamentoVerbaInput(
        string Descricao,
        string Tipo, // Provento, Desconto
        decimal Valor
    );

    public record ProcessarFolhaPagamentoCommand(
        Guid ColaboradorId,
        int MesCompetencia,
        int AnoCompetencia,
        List<FolhaPagamentoVerbaInput> Verbas
    ) : ICommand;

    public class ProcessarFolhaPagamentoCommandValidator : AbstractValidator<ProcessarFolhaPagamentoCommand>
    {
        public ProcessarFolhaPagamentoCommandValidator()
        {
            RuleFor(c => c.ColaboradorId)
                .NotEmpty().WithMessage("O ID do colaborador é obrigatório.");

            RuleFor(c => c.MesCompetencia)
                .InclusiveBetween(1, 12).WithMessage("O mês de competência deve ser de 1 a 12.");

            RuleFor(c => c.AnoCompetencia)
                .InclusiveBetween(2000, 2100).WithMessage("O ano de competência é inválido.");

            RuleForEach(c => c.Verbas).ChildRules(verba =>
            {
                verba.RuleFor(v => v.Descricao)
                    .NotEmpty().WithMessage("A descrição da verba é obrigatória.");

                verba.RuleFor(v => v.Tipo)
                    .Must(t => t == "Provento" || t == "Desconto")
                    .WithMessage("O tipo de verba deve ser 'Provento' ou 'Desconto'.");

                verba.RuleFor(v => v.Valor)
                    .GreaterThan(0).WithMessage("O valor da verba deve ser maior que zero.");
            });
        }
    }
}
