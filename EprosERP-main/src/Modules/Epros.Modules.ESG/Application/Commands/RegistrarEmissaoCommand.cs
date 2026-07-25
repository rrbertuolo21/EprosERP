using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.ESG.Application.Commands
{
    public record RegistrarEmissaoCommand(
        string FonteEmissao,
        int Escopo, // 1, 2, 3
        string CategoriaGhg,
        decimal QuantidadeConsumo,
        string UnidadeMedida,
        decimal FatorEmissao,
        DateTime DataTransacao
    ) : ICommand;

    public class RegistrarEmissaoCommandValidator : AbstractValidator<RegistrarEmissaoCommand>
    {
        public RegistrarEmissaoCommandValidator()
        {
            RuleFor(c => c.FonteEmissao).NotEmpty().WithMessage("A fonte de emissão é obrigatória.");
            RuleFor(c => c.Escopo).Must(e => e == 1 || e == 2 || e == 3).WithMessage("O Escopo deve ser 1, 2 ou 3.");
            RuleFor(c => c.CategoriaGhg).NotEmpty().WithMessage("A categoria GHG é obrigatória.");
            RuleFor(c => c.QuantidadeConsumo).GreaterThan(0).WithMessage("A quantidade de consumo deve ser maior que zero.");
            RuleFor(c => c.UnidadeMedida).NotEmpty().WithMessage("A unidade de medida é obrigatória.");
            RuleFor(c => c.FatorEmissao).GreaterThanOrEqualTo(0).WithMessage("O fator de emissão não pode ser negativo.");
        }
    }
}
