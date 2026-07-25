using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.DMS.Application.Commands
{
    public record CriarJornadaFinCommand(
        Guid OportunidadeId,
        Guid? VendaId,
        Guid ClienteId,
        Guid VeiculoId
    ) : ICommand;

    public class CriarJornadaFinCommandValidator : AbstractValidator<CriarJornadaFinCommand>
    {
        public CriarJornadaFinCommandValidator()
        {
            RuleFor(c => c.OportunidadeId).NotEmpty();
            RuleFor(c => c.ClienteId).NotEmpty();
            RuleFor(c => c.VeiculoId).NotEmpty();
        }
    }

    public record CriarSimulacaoFinCommand(
        Guid JornadaId,
        string ChaveIdempotencia,
        decimal PrecoVeiculo,
        decimal Entrada,
        decimal Saldo,
        int PrazoQuantidade,
        string PrazoUnidade,
        string? OrigemVersao,
        string? MemoriaJson
    ) : ICommand;

    public class CriarSimulacaoFinCommandValidator : AbstractValidator<CriarSimulacaoFinCommand>
    {
        public CriarSimulacaoFinCommandValidator()
        {
            RuleFor(c => c.JornadaId).NotEmpty();
            RuleFor(c => c.ChaveIdempotencia).NotEmpty();
            RuleFor(c => c.PrecoVeiculo).GreaterThan(0);
            RuleFor(c => c.Entrada).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Saldo).GreaterThanOrEqualTo(0);
            RuleFor(c => c.PrazoQuantidade).GreaterThan(0);
        }
    }

    public record CriarContratoFinCommand(
        Guid? PropostaId,
        Guid VendaId,
        string NumeroContrato,
        string? CondicaoFinalJson
    ) : ICommand;

    public class CriarContratoFinCommandValidator : AbstractValidator<CriarContratoFinCommand>
    {
        public CriarContratoFinCommandValidator()
        {
            RuleFor(c => c.VendaId).NotEmpty();
            RuleFor(c => c.NumeroContrato).NotEmpty();
        }
    }
}
