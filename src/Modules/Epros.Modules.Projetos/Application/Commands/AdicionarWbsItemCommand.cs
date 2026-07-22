using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Projetos.Application.Commands
{
    public record AdicionarWbsItemCommand(
        Guid ProjetoId,
        string Nome,
        string Descricao,
        DateTime DataInicio,
        DateTime DataTermino,
        decimal PesoPonderado
    ) : ICommand;

    public class AdicionarWbsItemCommandValidator : AbstractValidator<AdicionarWbsItemCommand>
    {
        public AdicionarWbsItemCommandValidator()
        {
            RuleFor(c => c.ProjetoId)
                .NotEmpty().WithMessage("O ID do projeto é obrigatório.");

            RuleFor(c => c.Nome)
                .NotEmpty().WithMessage("O Nome da tarefa é obrigatório.");

            RuleFor(c => c.PesoPonderado)
                .GreaterThan(0).WithMessage("O Peso ponderado deve ser maior que zero.");
        }
    }
}
