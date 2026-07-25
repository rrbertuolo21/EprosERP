using System;
using System.Collections.Generic;
using Epros.Modules.Projetos.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Projetos.Application.Commands
{
    // ===== PRJ-RSK (Gestao de Riscos de Projeto) =====

    public record CriarRiscoProjetoCommand(
        Guid ProjetoId,
        string Titulo,
        EPrioridadeRisco Prioridade,
        string Descricao,
        Guid EstagioId,
        List<Guid> Responsaveis,
        Guid? CriadorId
    ) : ICommand;

    public class CriarRiscoProjetoCommandValidator : AbstractValidator<CriarRiscoProjetoCommand>
    {
        public CriarRiscoProjetoCommandValidator()
        {
            RuleFor(c => c.ProjetoId).NotEmpty().WithMessage("O projeto e obrigatorio.");
            RuleFor(c => c.Titulo).NotEmpty().MaximumLength(255).WithMessage("O titulo e obrigatorio (max 255).");
            RuleFor(c => c.Descricao).NotEmpty().WithMessage("A descricao e obrigatoria.");
            RuleFor(c => c.EstagioId).NotEmpty().WithMessage("O estagio e obrigatorio.");
            RuleFor(c => c.Responsaveis).NotEmpty().WithMessage("Informe ao menos um responsavel.");
        }
    }

    public record CriarEstagioRiscoCommand(
        string Nome,
        string Cor,
        bool Completo,
        int Ordem,
        Guid? CriadorId
    ) : ICommand;

    public class CriarEstagioRiscoCommandValidator : AbstractValidator<CriarEstagioRiscoCommand>
    {
        public CriarEstagioRiscoCommandValidator()
        {
            RuleFor(c => c.Nome).NotEmpty().MaximumLength(255).WithMessage("O nome do estagio e obrigatorio (max 255).");
            RuleFor(c => c.Cor).NotEmpty().MaximumLength(7).WithMessage("A cor do estagio e obrigatoria (max 7).");
        }
    }

    public record MoverRiscoCommand(Guid RiscoId, Guid EstagioDestinoId, Guid UsuarioId) : ICommand;

    public record ComentarRiscoCommand(Guid RiscoId, Guid UsuarioId, string Comentario) : ICommand;

    public record AlterarPrioridadeRiscoCommand(Guid RiscoId, EPrioridadeRisco Prioridade, Guid UsuarioId) : ICommand;

    public record SubmeterRiscoCommand(Guid RiscoId, Guid UsuarioId) : ICommand;

    public record AprovarRiscoCommand(Guid RiscoId, Guid UsuarioId) : ICommand;

    public record RejeitarRiscoCommand(Guid RiscoId, string Motivo, Guid UsuarioId) : ICommand;

    public record EscalonarRiscoCommand(Guid RiscoId, Guid UsuarioId) : ICommand;

    public record EncerrarRiscoCommand(Guid RiscoId, string Motivo, Guid UsuarioId) : ICommand;
}
