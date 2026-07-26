using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Projetos.Application.Commands
{
    // ===== PRJ-ORC (Planejamento e Orcamento) =====

    public record CriarOrcamentoProjetoCommand(
        Guid ProjetoId,
        decimal Budget,
        EBillingType? BillingType,
        decimal? BillingRate,
        decimal? EstimatedHours,
        decimal? CostsEstimate
    ) : ICommand;

    public class CriarOrcamentoProjetoCommandValidator : AbstractValidator<CriarOrcamentoProjetoCommand>
    {
        public CriarOrcamentoProjetoCommandValidator()
        {
            RuleFor(c => c.ProjetoId).NotEmpty().WithMessage("O projeto e obrigatorio.");
            RuleFor(c => c.Budget).GreaterThanOrEqualTo(0).WithMessage("O orcamento deve ser maior ou igual a zero.");
        }
    }

    public record AdicionarMarcoOrcamentarioCommand(
        Guid OrcamentoProjetoId,
        string Titulo,
        decimal Custo,
        DateTime DataInicio,
        DateTime DataFim,
        string? Resumo
    ) : ICommand;

    public record AtualizarProgressoMarcoCommand(
        Guid OrcamentoProjetoId,
        Guid MarcoId,
        int Progresso,
        EMarcoStatus Status
    ) : ICommand;

    public record AprovarOrcamentoProjetoCommand(Guid OrcamentoProjetoId) : ICommand;

    public record SubmeterOrcamentoProjetoCommand(Guid OrcamentoProjetoId) : ICommand;
}
