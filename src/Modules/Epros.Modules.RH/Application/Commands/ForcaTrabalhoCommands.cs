using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    // RH-WFM — Gestao da Forca de Trabalho.

    public record AdmitirWfmColaboradorCommand(
        Guid PessoaId,
        string Matricula,
        Guid CargoId,
        Guid DepartamentoId,
        Guid? FilialId,
        Guid? TurnoId,
        DateTime? DataAdmissao,
        decimal? SalarioBase,
        string? TipoRemuneracao
    ) : ICommand;

    public class AdmitirWfmColaboradorCommandValidator : AbstractValidator<AdmitirWfmColaboradorCommand>
    {
        public AdmitirWfmColaboradorCommandValidator()
        {
            RuleFor(c => c.PessoaId).NotEmpty().WithMessage("O colaborador deve referenciar uma pessoa existente (WFM secao 20.1).");
            RuleFor(c => c.Matricula).NotEmpty().WithMessage("A matricula e obrigatoria (WFM secao 20.2).");
            RuleFor(c => c.CargoId).NotEmpty();
            RuleFor(c => c.DepartamentoId).NotEmpty();
        }
    }

    public record DemitirWfmColaboradorCommand(Guid ColaboradorId) : ICommand;

    public record DefinirComissaoColaboradorCommand(
        Guid ColaboradorId,
        string TipoCargo,
        decimal ValorPercentualComissao
    ) : ICommand;

    public class DefinirComissaoColaboradorCommandValidator : AbstractValidator<DefinirComissaoColaboradorCommand>
    {
        public DefinirComissaoColaboradorCommandValidator()
        {
            RuleFor(c => c.ColaboradorId).NotEmpty();
            RuleFor(c => c.ValorPercentualComissao).InclusiveBetween(0m, 100m)
                .WithMessage("O percentual de comissao deve estar entre 0 e 100 (WFM secao 20.7).");
        }
    }
}
