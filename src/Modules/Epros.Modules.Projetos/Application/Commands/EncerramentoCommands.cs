using System;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Projetos.Application.Commands
{
    // ===== PRJ-ENC (Encerramento de Projeto) =====

    public record CriarEncerramentoProjetoCommand(
        Guid ProjetoId,
        string Codigo,
        string Descricao,
        Guid ResponsavelId
    ) : ICommand;

    public class CriarEncerramentoProjetoCommandValidator : AbstractValidator<CriarEncerramentoProjetoCommand>
    {
        public CriarEncerramentoProjetoCommandValidator()
        {
            RuleFor(c => c.ProjetoId).NotEmpty().WithMessage("O projeto e obrigatorio.");
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30).WithMessage("O codigo e obrigatorio (max 30).");
            RuleFor(c => c.Descricao).NotEmpty().MaximumLength(500).WithMessage("A descricao e obrigatoria (max 500).");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsavel e obrigatorio.");
        }
    }

    public record AdicionarItemEncerramentoCommand(
        Guid EncerramentoId,
        int Sequencia,
        decimal? Quantidade,
        string? Observacao
    ) : ICommand;

    public record AnexarDocumentoEncerramentoCommand(
        Guid EncerramentoId,
        Guid ArquivoId
    ) : ICommand;

    public record SubmeterEncerramentoCommand(Guid EncerramentoId) : ICommand;

    public record AprovarEncerramentoCommand(Guid EncerramentoId, EStatusFinalProjeto StatusFinalProjeto) : ICommand;

    public record RejeitarEncerramentoCommand(Guid EncerramentoId, string Motivo) : ICommand;

    public record SuspenderEncerramentoCommand(Guid EncerramentoId) : ICommand;

    public record RetomarEncerramentoCommand(Guid EncerramentoId) : ICommand;

    public record EncerrarEncerramentoCommand(Guid EncerramentoId, string Motivo) : ICommand;

    public record ArquivarEncerramentoCommand(Guid EncerramentoId) : ICommand;
}
