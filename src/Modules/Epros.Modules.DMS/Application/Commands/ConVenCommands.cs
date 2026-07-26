using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarEstoqueVeiculoCommand(
        Guid VeiculoId,
        string ChassiVin,
        Guid LocalId,
        decimal? Custo,
        decimal? PrecoSugerido,
        DateTime? DataEntrada
    ) : ICommand;

    public class CriarEstoqueVeiculoCommandValidator : AbstractValidator<CriarEstoqueVeiculoCommand>
    {
        public CriarEstoqueVeiculoCommandValidator()
        {
            RuleFor(c => c.VeiculoId).NotEmpty();
            RuleFor(c => c.ChassiVin).NotEmpty().Length(17).WithMessage("O chassi/VIN deve possuir exatamente 17 caracteres.");
            RuleFor(c => c.LocalId).NotEmpty();
        }
    }

    public record CriarReservaVeiculoCommand(
        Guid EstoqueVeiculoId,
        Guid OportunidadeId,
        DateTime Inicio,
        DateTime Fim
    ) : ICommand;

    public class CriarReservaVeiculoCommandValidator : AbstractValidator<CriarReservaVeiculoCommand>
    {
        public CriarReservaVeiculoCommandValidator()
        {
            RuleFor(c => c.EstoqueVeiculoId).NotEmpty();
            RuleFor(c => c.OportunidadeId).NotEmpty();
            RuleFor(c => c.Fim).GreaterThan(c => c.Inicio).WithMessage("A data/hora de término deve ser posterior ao início.");
        }
    }

    public record CriarPropostaVendaCommand(
        Guid OportunidadeId,
        Guid ClienteId,
        Guid? EstoqueVeiculoId,
        DateTime ValidaAte,
        decimal ValorVeiculo,
        decimal Desconto
    ) : ICommand;

    public class CriarPropostaVendaCommandValidator : AbstractValidator<CriarPropostaVendaCommand>
    {
        public CriarPropostaVendaCommandValidator()
        {
            RuleFor(c => c.OportunidadeId).NotEmpty();
            RuleFor(c => c.ClienteId).NotEmpty();
            RuleFor(c => c.ValorVeiculo).GreaterThan(0);
            RuleFor(c => c.Desconto).GreaterThanOrEqualTo(0);
        }
    }
}
