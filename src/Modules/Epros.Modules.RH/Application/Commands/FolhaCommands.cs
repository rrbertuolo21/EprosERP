using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    // RH-FP — Folha de Pagamento e Beneficios.

    public record CriarRubricaCommand(
        Guid EmpresaId,
        string Codigo,
        string Nome,
        string Descricao,
        string Tipo,
        string Unidade,
        string BaseCalculo,
        decimal? Taxa,
        bool Ativo
    ) : ICommand;

    public class CriarRubricaCommandValidator : AbstractValidator<CriarRubricaCommand>
    {
        public CriarRubricaCommandValidator()
        {
            RuleFor(c => c.EmpresaId).NotEmpty();
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O codigo da rubrica e obrigatorio.");
            RuleFor(c => c.Nome).NotEmpty().WithMessage("O nome da rubrica e obrigatorio.");
            RuleFor(c => c.Tipo)
                .Must(t => t == "Provento" || t == "Desconto" || t == "Informativo")
                .WithMessage("A rubrica deve ser Provento, Desconto ou Informativo (RF-FOL-004).");
        }
    }

    public record AbrirCompetenciaCommand(
        Guid EmpresaId,
        string Competencia,
        string Tipo,
        DateTime? PeriodoInicio,
        DateTime? PeriodoFim,
        string? Descricao
    ) : ICommand;

    public class AbrirCompetenciaCommandValidator : AbstractValidator<AbrirCompetenciaCommand>
    {
        public AbrirCompetenciaCommandValidator()
        {
            RuleFor(c => c.EmpresaId).NotEmpty();
            RuleFor(c => c.Competencia).NotEmpty();
            RuleFor(c => c.Tipo).NotEmpty().WithMessage("O tipo de folha e obrigatorio (RF-FOL-001).");
        }
    }

    public record FecharCompetenciaCommand(Guid CompetenciaId) : ICommand;
}
