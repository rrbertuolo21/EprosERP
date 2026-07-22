using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    public record CriarCodigoBeneficioFiscalCommand(
        string Codigo,
        EEstado Uf,
        string? Descricao,
        List<ECodigoSituacaoTributariaIcms> Csts,
        List<ECodigoSituacaoOperacaoSimplesNacional> Csosns
    ) : ICommand;

    public class CriarCodigoBeneficioFiscalCommandValidator : AbstractValidator<CriarCodigoBeneficioFiscalCommand>
    {
        public CriarCodigoBeneficioFiscalCommandValidator()
        {
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
