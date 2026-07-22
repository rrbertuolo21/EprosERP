using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record RegistrarApontamentoCommand(
        Guid OrdemProducaoId,
        decimal QuantidadeApontada,
        decimal QuantidadeRefugada,
        string Operador
    ) : ICommand;

    public class RegistrarApontamentoCommandValidator : AbstractValidator<RegistrarApontamentoCommand>
    {
        public RegistrarApontamentoCommandValidator()
        {
            RuleFor(c => c.OrdemProducaoId)
                .NotEmpty().WithMessage("O ID da ordem de produção é obrigatório.");

            RuleFor(c => c.QuantidadeApontada)
                .GreaterThanOrEqualTo(0).WithMessage("A quantidade apontada não pode ser negativa.");

            RuleFor(c => c.QuantidadeRefugada)
                .GreaterThanOrEqualTo(0).WithMessage("A quantidade refugada não pode ser negativa.");

            RuleFor(c => c.Operador)
                .NotEmpty().WithMessage("O nome do operador é obrigatório.");
        }
    }
}
