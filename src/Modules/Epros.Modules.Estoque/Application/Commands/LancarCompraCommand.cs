using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Estoque.Application.Commands
{
    public record LancarCompraCommand(
        string FornecedorCnpj,
        string FornecedorNome,
        string NumeroNota,
        string ChaveAcesso,
        decimal ValorTotal,
        DateTime DataEmissao,
        List<ItemCompraInput> Itens,
        // CD3/SRC-008: id da origem sob alçada (pedido de compra/compra). Quando informado, o lançamento
        // só efetiva se o pedido de aprovação estiver APROVADO. Opcional (default null) — ver AlcadaCompraGate.
        Guid? AprovacaoOrigemId = null
    ) : ICommand;

    public record ItemCompraInput(
        string Sku,
        string NomeProduto,
        decimal Quantidade,
        decimal PrecoUnitario,
        decimal ValorIms, // ICMS
        decimal ValorIpi
    );

    public class LancarCompraCommandValidator : AbstractValidator<LancarCompraCommand>
    {
        public LancarCompraCommandValidator()
        {
            RuleFor(c => c.FornecedorCnpj)
                .NotEmpty().WithMessage("O CNPJ do fornecedor é obrigatório.");

            RuleFor(c => c.FornecedorNome)
                .NotEmpty().WithMessage("O Nome do fornecedor é obrigatório.");

            RuleFor(c => c.NumeroNota)
                .NotEmpty().WithMessage("O Número da nota é obrigatório.");

            RuleFor(c => c.ChaveAcesso)
                .NotEmpty().WithMessage("A Chave de Acesso é obrigatória.")
                .Length(44).WithMessage("A Chave de Acesso da NF-e deve possuir exatamente 44 dígitos.");

            RuleFor(c => c.ValorTotal)
                .GreaterThanOrEqualTo(0).WithMessage("O Valor Total da nota não pode ser negativo.");

            RuleFor(c => c.Itens)
                .NotEmpty().WithMessage("A compra deve possuir pelo menos um item.");

            RuleForEach(c => c.Itens).ChildRules(item =>
            {
                item.RuleFor(i => i.Sku)
                    .NotEmpty().WithMessage("O SKU do produto é obrigatório.");

                item.RuleFor(i => i.NomeProduto)
                    .NotEmpty().WithMessage("O Nome do produto é obrigatório.");

                item.RuleFor(i => i.Quantidade)
                    .GreaterThan(0).WithMessage("A quantidade do item deve ser maior que zero.");

                item.RuleFor(i => i.PrecoUnitario)
                    .GreaterThan(0).WithMessage("O preço unitário do item deve ser maior que zero.");
            });
        }
    }
}
