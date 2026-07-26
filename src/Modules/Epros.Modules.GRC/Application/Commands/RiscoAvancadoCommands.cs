using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC-RIS — Gestao de Riscos Corporativos (aditivo ao RegistrarRiscoCommand ja existente)

    public record AvaliarRiscoCommand(
        Guid RiscoId,
        int Probabilidade, // 1-5
        int Impacto,       // 1-5
        string? Justificativa
    ) : ICommand;

    public class AvaliarRiscoCommandValidator : AbstractValidator<AvaliarRiscoCommand>
    {
        public AvaliarRiscoCommandValidator()
        {
            RuleFor(c => c.RiscoId).NotEmpty().WithMessage("O risco e obrigatorio.");
            RuleFor(c => c.Probabilidade).InclusiveBetween(1, 5).WithMessage("A probabilidade deve ser de 1 a 5.");
            RuleFor(c => c.Impacto).InclusiveBetween(1, 5).WithMessage("O impacto deve ser de 1 a 5.");
        }
    }

    public record CriarPlanoAcaoRiscoCommand(
        Guid RiscoId,
        string Descricao,
        Guid ResponsavelId,
        DateTime Prazo
    ) : ICommand;

    public class CriarPlanoAcaoRiscoCommandValidator : AbstractValidator<CriarPlanoAcaoRiscoCommand>
    {
        public CriarPlanoAcaoRiscoCommandValidator()
        {
            RuleFor(c => c.RiscoId).NotEmpty().WithMessage("O risco e obrigatorio.");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descricao da acao e obrigatoria.");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsavel e obrigatorio.");
        }
    }

    public record VincularControleMitigadorCommand(
        Guid RiscoId,
        Guid ControleId,
        string? Efetividade,
        string? Observacao
    ) : ICommand;

    public class VincularControleMitigadorCommandValidator : AbstractValidator<VincularControleMitigadorCommand>
    {
        public VincularControleMitigadorCommandValidator()
        {
            RuleFor(c => c.RiscoId).NotEmpty().WithMessage("O risco e obrigatorio.");
            RuleFor(c => c.ControleId).NotEmpty().WithMessage("O controle e obrigatorio.");
        }
    }
}
