using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Projetos.Application.Commands
{
    public record CriarProjetoCommand(
        string Nome,
        string Descricao,
        Guid ClienteId,
        DateTime DataInicio,
        DateTime? DataTermino,
        decimal OrcamentoTotal
    ) : ICommand;

    public class CriarProjetoCommandValidator : AbstractValidator<CriarProjetoCommand>
    {
        public CriarProjetoCommandValidator()
        {
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("O Nome do projeto é obrigatório.");

            RuleFor(c => c.ClienteId)
                .NotEmpty().WithMessage("O Cliente do projeto é obrigatório.");

            RuleFor(c => c.OrcamentoTotal)
                .GreaterThan(0).WithMessage("O Orçamento total deve ser maior que zero.");
        }
    }
}
