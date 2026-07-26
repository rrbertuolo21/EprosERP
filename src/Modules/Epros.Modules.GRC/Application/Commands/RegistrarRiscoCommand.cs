using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    public record RegistrarRiscoCommand(
        string Titulo,
        string Descricao,
        string Categoria, // Operacional, Financeiro, Legal, Seguranca
        int Probabilidade, // 1-5
        int Impacto // 1-5
    ) : ICommand;

    public class RegistrarRiscoCommandValidator : AbstractValidator<RegistrarRiscoCommand>
    {
        public RegistrarRiscoCommandValidator()
        {
            RuleFor(c => c.Titulo).NotEmpty().WithMessage("O título do risco é obrigatório.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descrição do risco é obrigatória.");
            RuleFor(c => c.Categoria).Must(cat => cat == "Operacional" || cat == "Financeiro" || cat == "Legal" || cat == "Seguranca")
                .WithMessage("A categoria deve ser 'Operacional', 'Financeiro', 'Legal' ou 'Seguranca'.");
            RuleFor(c => c.Probabilidade).InclusiveBetween(1, 5).WithMessage("A probabilidade deve ser de 1 a 5.");
            RuleFor(c => c.Impacto).InclusiveBetween(1, 5).WithMessage("O impacto deve ser de 1 a 5.");
        }
    }
}
