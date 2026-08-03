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

    /// <summary>
    /// NF-01 — Simula o financiamento (motor F&amp;I). Roda o <c>MotorFinanciamento</c> (Price/SAC +
    /// IOF + CET) e persiste o resultado na <c>SimulacaoFin</c>. A TAXA é parâmetro do cliente
    /// (skill: taxa praticada vive no overlay; // valida-contador para IOF/tarifas).
    /// </summary>
    public record SimularFinanciamentoCommand(
        Guid JornadaId,
        string ChaveIdempotencia,
        decimal PrecoVeiculo,
        decimal Entrada,
        int PrazoQuantidade,
        decimal TaxaJurosMensal,
        string Sistema,                    // "Price" | "Sac"
        bool TaxaAnual = false,            // se true, TaxaJurosMensal é a.a. e é convertida (composta)
        decimal TarifasFinanciadas = 0m,
        decimal TarifasDescontadas = 0m,
        decimal SeguroPorParcela = 0m,
        bool IofFinanciado = true,
        bool PessoaJuridica = false,
        string? OrigemVersao = null
    ) : ICommand;

    public class SimularFinanciamentoCommandValidator : AbstractValidator<SimularFinanciamentoCommand>
    {
        public SimularFinanciamentoCommandValidator()
        {
            RuleFor(c => c.JornadaId).NotEmpty();
            RuleFor(c => c.ChaveIdempotencia).NotEmpty();
            RuleFor(c => c.PrecoVeiculo).GreaterThan(0);
            RuleFor(c => c.Entrada).GreaterThanOrEqualTo(0);
            RuleFor(c => c.PrazoQuantidade).GreaterThan(0);
            RuleFor(c => c.TaxaJurosMensal).GreaterThanOrEqualTo(0);
            RuleFor(c => c.Entrada).LessThan(c => c.PrecoVeiculo)
                .WithMessage("A entrada deve ser menor que o preço do veículo (há saldo a financiar).");
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
