using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarContratoCommand(
        Guid ClienteId,
        int DiaVencimento,
        DateTime DataInicio,
        DateTime? DataFim,
        List<CriarContratoItemDto> Itens
    ) : ICommand;

    public record CriarContratoItemDto(
        string Descricao,
        int Quantidade,
        decimal ValorUnitario
    );

    public class CriarContratoCommandValidator : AbstractValidator<CriarContratoCommand>
    {
        public CriarContratoCommandValidator()
        {
            RuleFor(c => c.ClienteId)
                .NotEmpty().WithMessage("O ClienteId é obrigatório.");

            RuleFor(c => c.DiaVencimento)
                .InclusiveBetween(1, 28).WithMessage("O dia de vencimento deve estar entre 1 e 28.");

            RuleFor(c => c.DataInicio)
                .NotEmpty().WithMessage("A data de início é obrigatória.");

            RuleFor(c => c.Itens)
                .NotEmpty().WithMessage("O contrato deve possuir pelo menos um item.");

            RuleForEach(c => c.Itens).ChildRules(item =>
            {
                item.RuleFor(i => i.Descricao).NotEmpty().WithMessage("A descrição do item é obrigatória.");
                item.RuleFor(i => i.Quantidade).GreaterThan(0).WithMessage("A quantidade do item deve ser maior que zero.");
                item.RuleFor(i => i.ValorUnitario).GreaterThan(0).WithMessage("O valor unitário do item deve ser maior que zero.");
            });
        }
    }
}
