using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarPecaReposicaoCommand(
        Guid ProdutoId,
        string? FamiliaTecnica,
        string? Criticidade
    ) : ICommand;

    public class CriarPecaReposicaoCommandValidator : AbstractValidator<CriarPecaReposicaoCommand>
    {
        public CriarPecaReposicaoCommandValidator()
        {
            RuleFor(c => c.ProdutoId).NotEmpty().WithMessage("O produto é obrigatório.");
        }
    }

    public record CriarDemandaPecaCommand(
        Guid PecaId,
        Guid LocalId,
        string OrigemTipo,
        Guid OrigemId,
        Guid ItemOrigemId,
        decimal Quantidade,
        DateTime Prazo
    ) : ICommand;

    public class CriarDemandaPecaCommandValidator : AbstractValidator<CriarDemandaPecaCommand>
    {
        public CriarDemandaPecaCommandValidator()
        {
            RuleFor(c => c.PecaId).NotEmpty().WithMessage("A peça é obrigatória.");
            RuleFor(c => c.LocalId).NotEmpty().WithMessage("O local é obrigatório.");
            RuleFor(c => c.OrigemTipo).NotEmpty().WithMessage("O tipo de origem é obrigatório.");
            RuleFor(c => c.OrigemId).NotEmpty().WithMessage("A origem é obrigatória.");
            RuleFor(c => c.ItemOrigemId).NotEmpty().WithMessage("O item de origem é obrigatório.");
            RuleFor(c => c.Quantidade).GreaterThan(0).WithMessage("A quantidade deve ser maior que zero.");
        }
    }

    public record CriarReservaPecaCommand(
        Guid DemandaId,
        Guid PecaId,
        Guid LocalId,
        decimal QuantidadeReservada
    ) : ICommand;

    public class CriarReservaPecaCommandValidator : AbstractValidator<CriarReservaPecaCommand>
    {
        public CriarReservaPecaCommandValidator()
        {
            RuleFor(c => c.DemandaId).NotEmpty().WithMessage("A demanda é obrigatória.");
            RuleFor(c => c.PecaId).NotEmpty().WithMessage("A peça é obrigatória.");
            RuleFor(c => c.LocalId).NotEmpty().WithMessage("O local é obrigatório.");
            RuleFor(c => c.QuantidadeReservada).GreaterThan(0).WithMessage("A quantidade reservada deve ser maior que zero.");
        }
    }
}
