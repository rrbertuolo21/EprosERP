using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record AbrirOrdemProducaoCommand(
        string? Codigo,
        string ProdutoAcabadoSku,
        decimal QuantidadePlanejada
    ) : ICommand;

    public class AbrirOrdemProducaoCommandValidator : AbstractValidator<AbrirOrdemProducaoCommand>
    {
        public AbrirOrdemProducaoCommandValidator()
        {
            RuleFor(c => c.ProdutoAcabadoSku)
                .NotEmpty().WithMessage("O SKU do produto acabado é obrigatório.");

            RuleFor(c => c.QuantidadePlanejada)
                .GreaterThan(0).WithMessage("A quantidade planejada deve ser maior que zero.");
        }
    }
}
