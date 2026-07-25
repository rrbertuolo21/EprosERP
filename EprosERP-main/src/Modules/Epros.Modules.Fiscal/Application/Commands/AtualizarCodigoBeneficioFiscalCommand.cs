using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record AtualizarCodigoBeneficioFiscalCommand(
        Guid Id,
        string Codigo,
        EEstado Uf,
        string? Descricao,
        List<ECodigoSituacaoTributariaIcms> Csts,
        List<ECodigoSituacaoOperacaoSimplesNacional> Csosns
    ) : ICommand;

    public class AtualizarCodigoBeneficioFiscalCommandValidator : AbstractValidator<AtualizarCodigoBeneficioFiscalCommand>
    {
        public AtualizarCodigoBeneficioFiscalCommandValidator()
        {
            RuleFor(c => c.Id)
                .NotEmpty().WithMessage("O ID do benefício é obrigatório.");

            RuleFor(c => c.Codigo)
                .NotEmpty().WithMessage("O código do benefício fiscal é obrigatório.")
                .MaximumLength(10).WithMessage("O código deve possuir no máximo 10 caracteres.");

            RuleFor(c => c.Uf)
                .IsInEnum().WithMessage("UF inválida.");

            RuleFor(c => c.Descricao)
                .MaximumLength(1000).WithMessage("A descrição deve possuir no máximo 1000 caracteres.");
        }
    }
}
