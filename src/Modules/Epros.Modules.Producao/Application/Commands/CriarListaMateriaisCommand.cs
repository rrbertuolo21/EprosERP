using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    public record CriarListaMateriaisItem(
        string InsumoSku,
        decimal QuantidadeNecessaria,
        string UnidadeMedida
    );

    public record CriarListaMateriaisCommand(
        string ProdutoAcabadoSku,
        string Descricao,
        string Versao,
        List<CriarListaMateriaisItem> Itens
    ) : ICommand;

    public class CriarListaMateriaisCommandValidator : AbstractValidator<CriarListaMateriaisCommand>
    {
        public CriarListaMateriaisCommandValidator()
        {
            RuleFor(c => c.ProdutoAcabadoSku)
                .NotEmpty().WithMessage("O SKU do produto acabado é obrigatório.");

            RuleFor(c => c.Descricao)
                .NotEmpty().WithMessage("A descrição da lista de materiais é obrigatória.");

            RuleFor(c => c.Versao)
                .NotEmpty().WithMessage("A versão da lista de materiais é obrigatória.");

            RuleFor(c => c.Itens)
                .NotEmpty().WithMessage("A lista de materiais deve conter pelo menos um item/insumo.");

            RuleForEach(c => c.Itens).ChildRules(item =>
            {
                item.RuleFor(i => i.InsumoSku)
                    .NotEmpty().WithMessage("O SKU do insumo é obrigatório.");

                item.RuleFor(i => i.QuantidadeNecessaria)
                    .GreaterThan(0).WithMessage("A quantidade necessária deve ser maior que zero.");

                item.RuleFor(i => i.UnidadeMedida)
                    .NotEmpty().WithMessage("A unidade de medida é obrigatória.");
            });
        }
    }
}
