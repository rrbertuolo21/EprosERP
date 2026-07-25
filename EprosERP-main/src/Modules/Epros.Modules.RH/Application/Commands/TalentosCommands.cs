using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    // RH-TLT — Gestao de Talentos.

    public record CriarMetaColaboradorCommand(
        Guid ColaboradorId,
        Guid? TipoMetaId,
        string Titulo,
        string? Descricao,
        DateTime DataInicio,
        DateTime DataFim,
        decimal? Alvo,
        decimal Progresso,
        Guid OwnerId,
        Guid? CriadoPorId
    ) : ICommand;

    public class CriarMetaColaboradorCommandValidator : AbstractValidator<CriarMetaColaboradorCommand>
    {
        public CriarMetaColaboradorCommandValidator()
        {
            RuleFor(c => c.ColaboradorId).NotEmpty();
            RuleFor(c => c.Titulo).NotEmpty().WithMessage("O titulo da meta e obrigatorio (TLT secao 18).");
            RuleFor(c => c.DataFim).GreaterThan(c => c.DataInicio)
                .WithMessage("A data final da meta deve ser posterior a data inicial (TLT secao 20.3).");
            RuleFor(c => c.Progresso).InclusiveBetween(0m, 100m)
                .WithMessage("O progresso deve estar entre 0 e 100 (TLT secao 18).");
        }
    }

    public record RegistrarSolicitacaoLicencaCommand(
        Guid ColaboradorId,
        Guid? TipoLicencaId,
        DateTime? DataInicio,
        DateTime? DataFim,
        int? TotalDias,
        string? Motivo,
        string? Anexo,
        Guid? OwnerId,
        Guid? CriadoPorId
    ) : ICommand;

    public record AprovarSolicitacaoLicencaCommand(Guid SolicitacaoId, Guid AprovadoPorId, string? Comentario) : ICommand;
    public record RejeitarSolicitacaoLicencaCommand(Guid SolicitacaoId, Guid AprovadoPorId, string? Comentario) : ICommand;
}
