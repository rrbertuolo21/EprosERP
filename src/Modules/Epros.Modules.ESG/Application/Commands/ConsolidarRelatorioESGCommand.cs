using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.ESG.Application.Commands
{
    public record ConsolidarRelatorioESGCommand(
        int AnoFiscal
    ) : ICommand;

    public class ConsolidarRelatorioESGCommandValidator : AbstractValidator<ConsolidarRelatorioESGCommand>
    {
        public ConsolidarRelatorioESGCommandValidator()
        {
            RuleFor(c => c.AnoFiscal).GreaterThan(1999).WithMessage("Ano fiscal inválido.");
        }
    }
}
