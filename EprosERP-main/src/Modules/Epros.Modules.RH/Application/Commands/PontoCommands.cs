using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    // RH-PNT — Ponto e Jornada.

    public record RegistrarMarcacaoCommand(
        Guid ColaboradorId,
        Guid RelogioId,
        int? Nsr,
        DateTime? DataMarcacao,
        TimeSpan HoraMarcacao,
        string TipoMarcacao,
        string TipoRegistro,
        string ParEntradaSaida,
        string Justificativa,
        string? Origem
    ) : ICommand;

    public class RegistrarMarcacaoCommandValidator : AbstractValidator<RegistrarMarcacaoCommand>
    {
        public RegistrarMarcacaoCommandValidator()
        {
            RuleFor(c => c.ColaboradorId).NotEmpty().WithMessage("O colaborador da marcacao e obrigatorio (PNT-001).");
            RuleFor(c => c.TipoMarcacao).NotEmpty();
            RuleFor(c => c.TipoRegistro).NotEmpty();
        }
    }

    public record AbrirPeriodoApuracaoCommand(
        Guid EmpresaId,
        string Competencia,
        DateTime DataInicio,
        DateTime DataFim
    ) : ICommand;

    public class AbrirPeriodoApuracaoCommandValidator : AbstractValidator<AbrirPeriodoApuracaoCommand>
    {
        public AbrirPeriodoApuracaoCommandValidator()
        {
            RuleFor(c => c.EmpresaId).NotEmpty();
            RuleFor(c => c.Competencia).NotEmpty();
        }
    }

    public record FecharPeriodoApuracaoCommand(Guid PeriodoId) : ICommand;
}
