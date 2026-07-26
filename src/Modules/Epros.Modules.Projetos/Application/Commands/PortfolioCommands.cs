using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Projetos.Application.Commands
{
    // ===== PRJ-PRT (Portfolio e Priorizacao) =====

    public record CriarPortfolioCommand(
        string Codigo,
        string Descricao,
        Guid ResponsavelId,
        string? TipoPortfolio,
        string? Justificativa
    ) : ICommand;

    public class CriarPortfolioCommandValidator : AbstractValidator<CriarPortfolioCommand>
    {
        public CriarPortfolioCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30).WithMessage("O codigo e obrigatorio (max 30).");
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500).WithMessage("A descricao e obrigatoria (max 500).");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsavel e obrigatorio.");
        }
    }

    public record AdicionarItemPortfolioCommand(
        Guid PortfolioId,
        int Sequencia,
        string TipoItem,
        Guid? ProjetoId,
        Guid? ProgramaId,
        string? Titulo,
        decimal? ValorEstimado,
        decimal? EsforcoEstimado,
        decimal? CapacidadeRequerida,
        decimal? Npv,
        decimal? Payback,
        decimal? AlinhamentoEstrategico,
        decimal? Risco,
        decimal? Score,
        string? JustificativaPrioridade,
        string? Observacao
    ) : ICommand;

    public record PriorizarPortfolioManualCommand(
        Guid PortfolioId,
        decimal? ScoreTotal,
        string Justificativa,
        Guid UsuarioId
    ) : ICommand;

    public record AnexarDocumentoPortfolioCommand(
        Guid PortfolioId,
        Guid? ItemId,
        Guid ArquivoId,
        string? TipoAnexo
    ) : ICommand;

    public record SubmeterPortfolioCommand(Guid PortfolioId, Guid UsuarioId) : ICommand;

    public record AprovarPortfolioCommand(Guid PortfolioId, Guid UsuarioId) : ICommand;

    public record RejeitarPortfolioCommand(Guid PortfolioId, string Motivo, Guid UsuarioId) : ICommand;

    public record SuspenderPortfolioCommand(Guid PortfolioId, Guid UsuarioId) : ICommand;

    public record RetomarPortfolioCommand(Guid PortfolioId, Guid UsuarioId) : ICommand;

    public record EncerrarPortfolioCommand(Guid PortfolioId, Guid UsuarioId) : ICommand;

    public record InativarPortfolioCommand(Guid PortfolioId, Guid UsuarioId) : ICommand;

    public record ReativarPortfolioCommand(Guid PortfolioId, Guid UsuarioId) : ICommand;
}
