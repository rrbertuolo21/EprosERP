using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    /// <summary>
    /// APP-TEN-010 / REG-015 / REG-019 — governança de upgrade de versão do Super Admin.
    /// Solicitação de upgrade (maker). O executor (checker) precisa ser diferente.
    /// </summary>
    public record SolicitarUpgradeVersaoCommand(string VersaoAtual, string VersaoAlvo, string Motivo, bool RollbackDisponivel) : ICommand;

    public class SolicitarUpgradeVersaoCommandValidator : AbstractValidator<SolicitarUpgradeVersaoCommand>
    {
        public SolicitarUpgradeVersaoCommandValidator()
        {
            RuleFor(x => x.VersaoAtual).NotEmpty().WithMessage("A versão atual é obrigatória.");
            RuleFor(x => x.VersaoAlvo).NotEmpty().WithMessage("A versão alvo é obrigatória.");
            RuleFor(x => x.Motivo).NotEmpty().WithMessage("O motivo do upgrade é obrigatório.");
        }
    }

    public record AprovarUpgradeVersaoCommand(Guid SolicitacaoId, string? Comentario) : ICommand;

    public record RejeitarUpgradeVersaoCommand(Guid SolicitacaoId, string? Comentario) : ICommand;

    /// <summary>Executa o upgrade aprovado (aplica migrations). Bloqueia reexecução acidental.</summary>
    public record ExecutarUpgradeVersaoCommand(Guid SolicitacaoId) : ICommand;

    public record AplicarRollbackUpgradeCommand(Guid SolicitacaoId, string? Log) : ICommand;
}
