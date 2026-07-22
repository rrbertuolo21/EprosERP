using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.ESG.Application.Commands
{
    public record CriarRelatorioESGCommand(
        int AnoFiscal,
        string NomeRelatorio
    ) : ICommand;

    public class CriarRelatorioESGCommandValidator : AbstractValidator<CriarRelatorioESGCommand>
    {
        public CriarRelatorioESGCommandValidator()
        {
            RuleFor(c => c.AnoFiscal).GreaterThan(1999).WithMessage("Ano fiscal inválido.");
            RuleFor(c => c.NomeRelatorio).NotEmpty().WithMessage("O nome do relatório é obrigatório.");
        }
    }
}
