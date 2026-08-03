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
            // A3/A4: só o Nome é obrigatório. Cliente (projeto interno) e orçamento são opcionais na criação
            // (projeto nasce em Rascunho; orçamento é submódulo próprio com baseline).
            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("O Nome do projeto é obrigatório.");

            RuleFor(c => c.OrcamentoTotal)
                .GreaterThanOrEqualTo(0).WithMessage("O Orçamento total não pode ser negativo.");
        }
    }
}
