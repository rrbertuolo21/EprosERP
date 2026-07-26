using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    /// <summary>
    /// Envia uma Carta de Correção Eletrônica (CC-e) para uma NF-e/NFC-e autorizada.
    /// A sequência do evento é resolvida automaticamente pelo handler (nº de CC-e anteriores + 1).
    /// </summary>
    public record CartaCorrecaoDocumentoFiscalCommand(
        Guid DocumentoFiscalId,
        string TextoCorrecao
    ) : ICommand;

    public class CartaCorrecaoDocumentoFiscalCommandValidator : AbstractValidator<CartaCorrecaoDocumentoFiscalCommand>
    {
        public CartaCorrecaoDocumentoFiscalCommandValidator()
        {
            RuleFor(c => c.DocumentoFiscalId)
                .NotEmpty().WithMessage("O DocumentoFiscalId é obrigatório.");

            RuleFor(c => c.TextoCorrecao)
                .NotEmpty().WithMessage("O texto da correção é obrigatório.")
                .MinimumLength(15).WithMessage("O texto da correção deve possuir no mínimo 15 caracteres.")
                .MaximumLength(1000).WithMessage("O texto da correção deve possuir no máximo 1000 caracteres.");
        }
    }
}
