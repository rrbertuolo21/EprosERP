using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Projetos.Application.Commands
{
    public record AlocarRecursoCommand(
        Guid ProjetoId,
        Guid ColaboradorId,
        string Funcao,
        decimal CustoHora,
        decimal HorasPlanejadas
    ) : ICommand;

    public class AlocarRecursoCommandValidator : AbstractValidator<AlocarRecursoCommand>
    {
        public AlocarRecursoCommandValidator()
        {
            RuleFor(c => c.ProjetoId)
                .NotEmpty().WithMessage("O ID do projeto é obrigatório.");

            RuleFor(c => c.ColaboradorId)
                .NotEmpty().WithMessage("O ID do colaborador é obrigatório.");

            RuleFor(c => c.Funcao)
                .NotEmpty().WithMessage("A função é obrigatória.");

            RuleFor(c => c.CustoHora)
                .GreaterThan(0).WithMessage("O custo por hora deve ser maior que zero.");

            RuleFor(c => c.HorasPlanejadas)
                .GreaterThan(0).WithMessage("As horas planejadas devem ser maiores que zero.");
        }
    }
}
