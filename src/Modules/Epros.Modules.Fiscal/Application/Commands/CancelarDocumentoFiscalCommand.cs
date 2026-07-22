using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CancelarDocumentoFiscalCommand(
        Guid DocumentoFiscalId,
        string Justificativa
    ) : ICommand;

    public class CancelarDocumentoFiscalCommandValidator : AbstractValidator<CancelarDocumentoFiscalCommand>
    {
        public CancelarDocumentoFiscalCommandValidator()
        {
            RuleFor(c => c.DocumentoFiscalId)
                .NotEmpty().WithMessage("O DocumentoFiscalId é obrigatório.");

            RuleFor(c => c.Justificativa)
                .NotEmpty().WithMessage("A justificativa de cancelamento é obrigatória.")
                .MinimumLength(15).WithMessage("A justificativa de cancelamento deve possuir no mínimo 15 caracteres.")
                .MaximumLength(255).WithMessage("A justificativa de cancelamento deve possuir no máximo 255 caracteres.");
        }
    }
}
