using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.RH.Application.Commands
{
    // RH-REC — Recrutamento.

    public record CriarVagaCommand(
        string Titulo,
        int Posicoes,
        int Prioridade,
        string Descricao,
        string Habilidades,
        string TipoCandidatura,
        string? UrlCandidatura,
        Guid FilialId,
        Guid TipoVagaId,
        Guid LocalVagaId,
        Guid CriadoPorUsuarioId,
        Guid DonoFuncionalId
    ) : ICommand;

    public class CriarVagaCommandValidator : AbstractValidator<CriarVagaCommand>
    {
        public CriarVagaCommandValidator()
        {
            RuleFor(c => c.Titulo).NotEmpty().WithMessage("O titulo da vaga e obrigatorio (REC-REG-013).");
            RuleFor(c => c.Posicoes).GreaterThanOrEqualTo(1).WithMessage("A vaga deve ter ao menos 1 posicao (REC-REG-013).");
            RuleFor(c => c.Prioridade).InclusiveBetween(0, 2).WithMessage("Prioridade deve ser 0, 1 ou 2.");
            RuleFor(c => c.TipoCandidatura).Must(t => t == "existing" || t == "custom")
                .WithMessage("Tipo de candidatura deve ser existing ou custom.");
            RuleFor(c => c.UrlCandidatura).NotEmpty()
                .When(c => c.TipoCandidatura == "custom")
                .WithMessage("URL de candidatura e obrigatoria quando o tipo for custom (REC-REG-014).");
        }
    }

    public record PublicarVagaCommand(Guid VagaId) : ICommand;

    public record RegistrarCandidaturaCommand(
        string PrimeiroNome,
        string? Sobrenome,
        string Email,
        decimal AnosExperiencia,
        Guid VagaId,
        Guid FonteCandidatoId,
        Guid CriadoPorUsuarioId,
        Guid DonoFuncionalId
    ) : ICommand;

    public class RegistrarCandidaturaCommandValidator : AbstractValidator<RegistrarCandidaturaCommand>
    {
        public RegistrarCandidaturaCommandValidator()
        {
            RuleFor(c => c.PrimeiroNome).NotEmpty().WithMessage("O nome do candidato e obrigatorio (REC-REG-021).");
            RuleFor(c => c.Email).NotEmpty().EmailAddress().WithMessage("E-mail valido e obrigatorio (REC-REG-021).");
            RuleFor(c => c.VagaId).NotEmpty();
            RuleFor(c => c.FonteCandidatoId).NotEmpty();
        }
    }

    public record RegistrarFeedbackEntrevistaCommand(
        Guid EntrevistaId,
        decimal NotaTecnica,
        decimal NotaComunicacao,
        decimal NotaAderenciaCultural,
        string Recomendacao,
        string? PontosFortes,
        string? PontosFracos,
        string? Comentarios,
        string EntrevistadoresJson,
        Guid CriadoPorUsuarioId,
        Guid DonoFuncionalId
    ) : ICommand;

    public class RegistrarFeedbackEntrevistaCommandValidator : AbstractValidator<RegistrarFeedbackEntrevistaCommand>
    {
        public RegistrarFeedbackEntrevistaCommandValidator()
        {
            RuleFor(c => c.EntrevistaId).NotEmpty();
            RuleFor(c => c.Recomendacao).NotEmpty();
        }
    }
}
