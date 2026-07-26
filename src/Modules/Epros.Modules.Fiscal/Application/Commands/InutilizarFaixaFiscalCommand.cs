using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    /// <summary>
    /// Inutiliza uma faixa de numeração (números pulados/não usados) na SEFAZ.
    /// Não referencia DocumentoFiscal — inutilização é sobre a faixa, não sobre nota emitida.
    /// Fiel ao legado <c>InutilizacaoDfeController.Inutilizar</c> (modelo/série/faixa/justificativa).
    /// </summary>
    public record InutilizarFaixaFiscalCommand(
        int ModeloDocumento,
        int Serie,
        long NrNfInicial,
        long NrNfFinal,
        string Justificativa,
        int Ambiente = 2,
        int Ano = 0
    ) : ICommand;

    public class InutilizarFaixaFiscalCommandValidator : AbstractValidator<InutilizarFaixaFiscalCommand>
    {
        public InutilizarFaixaFiscalCommandValidator()
        {
            RuleFor(c => c.ModeloDocumento)
                .Must(m => m == 55 || m == 65)
                .WithMessage("O modelo do documento deve ser 55 (NF-e) ou 65 (NFC-e).");

            RuleFor(c => c.Serie)
                .GreaterThanOrEqualTo(0).WithMessage("A série é obrigatória.");

            RuleFor(c => c.NrNfInicial)
                .GreaterThan(0).WithMessage("O número inicial da faixa deve ser maior que zero.");

            RuleFor(c => c.NrNfFinal)
                .GreaterThanOrEqualTo(c => c.NrNfInicial)
                .WithMessage("O número final da faixa deve ser maior ou igual ao número inicial.");

            RuleFor(c => c.Justificativa)
                .NotEmpty().WithMessage("A justificativa de inutilização é obrigatória.")
                .MinimumLength(15).WithMessage("A justificativa de inutilização deve possuir no mínimo 15 caracteres.")
                .MaximumLength(255).WithMessage("A justificativa de inutilização deve possuir no máximo 255 caracteres.");
        }
    }
}
