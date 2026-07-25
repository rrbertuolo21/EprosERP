using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    public record CriarControleCommand(
        string Codigo,
        string Nome,
        string Descricao,
        string Frequencia
    ) : ICommand;

    public class CriarControleCommandValidator : AbstractValidator<CriarControleCommand>
    {
        public CriarControleCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código do controle é obrigatório.");
            RuleFor(c => c.Nome).NotEmpty().WithMessage("O nome do controle é obrigatório.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descrição do controle é obrigatória.");
            RuleFor(c => c.Frequencia).Must(f => f == "Diaria" || f == "Semanal" || f == "Mensal" || f == "Anual")
                .WithMessage("A frequência deve ser 'Diaria', 'Semanal', 'Mensal' ou 'Anual'.");
        }
    }
}
