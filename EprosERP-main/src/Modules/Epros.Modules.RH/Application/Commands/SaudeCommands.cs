using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    // RH-SSO — Saude e Seguranca Ocupacional.

    public record CriarPppCommand(
        Guid ColaboradorId,
        string Observacao
    ) : ICommand;

    public class CriarPppCommandValidator : AbstractValidator<CriarPppCommand>
    {
        public CriarPppCommandValidator()
        {
            RuleFor(c => c.ColaboradorId).NotEmpty().WithMessage("O colaborador do PPP e obrigatorio (SSO-REG-001).");
            RuleFor(c => c.Observacao).NotEmpty().WithMessage("A observacao do PPP e obrigatoria (SSO-REG-003).");
        }
    }

    public record RegistrarExameMedicoCommand(
        Guid? PppId,
        Guid ColaboradorId,
        DateTime? DataUltimo,
        string Tipo,
        string Natureza,
        string Exame,
        string IndicacaoResultados
    ) : ICommand;

    public class RegistrarExameMedicoCommandValidator : AbstractValidator<RegistrarExameMedicoCommand>
    {
        public RegistrarExameMedicoCommandValidator()
        {
            RuleFor(c => c.ColaboradorId).NotEmpty();
            RuleFor(c => c.Tipo).NotEmpty().WithMessage("O tipo do exame e obrigatorio (SSO-REG-004).");
            RuleFor(c => c.Natureza).NotEmpty().WithMessage("A natureza do exame e obrigatoria (SSO-REG-004).");
            RuleFor(c => c.Exame).NotEmpty().WithMessage("O exame e obrigatorio (SSO-REG-004).");
            RuleFor(c => c.IndicacaoResultados).NotEmpty().WithMessage("A indicacao de resultados e obrigatoria (SSO-REG-004).");
        }
    }
}
