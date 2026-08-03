using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC-SOD avançado — bloqueio preventivo (D-SOD-03), workflow de exceção (D-SOD-02),
    // expiração que reativa violação (RN-SOD-039) e bypass do admin (D-SOD-04).

    /// <summary>
    /// D-SOD-03 / GRC-CMD-002 — bloqueio preventivo pré-gravação. A Identidade chama ANTES de
    /// gravar a concessão: dado o conjunto de funções efetivas (atuais + novas), retorna se a
    /// concessão gera conflito e se deve ser bloqueada (regra ModoTratamento=Bloqueia) ou permitida
    /// mediante exceção. Não persiste concessão (RBAC vive na Plataforma — D-SOD-05).
    /// </summary>
    public record AvaliarConcessaoSoDCommand(
        Guid? PerfilId,
        Guid? UsuarioId,
        string Alvo, // Perfil, Usuario
        List<Guid> FuncoesAtuais,
        List<Guid> FuncoesNovas
    ) : ICommand;

    public class AvaliarConcessaoSoDCommandValidator : AbstractValidator<AvaliarConcessaoSoDCommand>
    {
        public AvaliarConcessaoSoDCommandValidator()
        {
            RuleFor(c => c.Alvo).Must(a => a == "Perfil" || a == "Usuario")
                .WithMessage("O alvo deve ser 'Perfil' ou 'Usuario'.");
            RuleFor(c => c).Must(c => c.PerfilId != null || c.UsuarioId != null)
                .WithMessage("Informe um perfil ou um usuario.");
            RuleFor(c => c.FuncoesNovas).NotNull().WithMessage("As funcoes novas sao obrigatorias.");
        }
    }

    /// <summary>D-SOD-02 — solicita exceção para uma violação (fica EmAnalise; exige controle compensatório).</summary>
    public record SolicitarExcecaoSoDCommand(
        Guid ViolacaoId,
        Guid SolicitanteId,
        string Justificativa,
        DateTime DataInicio,
        DateTime DataFim,
        Guid? ControleCompensatorioId,
        string? ControleCompensatorio
    ) : ICommand;

    public class SolicitarExcecaoSoDCommandValidator : AbstractValidator<SolicitarExcecaoSoDCommand>
    {
        public SolicitarExcecaoSoDCommandValidator()
        {
            RuleFor(c => c.ViolacaoId).NotEmpty().WithMessage("A violacao e obrigatoria.");
            RuleFor(c => c.SolicitanteId).NotEmpty().WithMessage("O solicitante e obrigatorio.");
            RuleFor(c => c.Justificativa).NotEmpty().WithMessage("A justificativa e obrigatoria.");
        }
    }

    /// <summary>D-SOD-02 — aprova exceção (autoaprovação proibida) e mitiga a violação vinculada.</summary>
    public record AprovarExcecaoSoDCommand(Guid ExcecaoId, Guid AprovadorId) : ICommand;

    public class AprovarExcecaoSoDCommandValidator : AbstractValidator<AprovarExcecaoSoDCommand>
    {
        public AprovarExcecaoSoDCommandValidator()
        {
            RuleFor(c => c.ExcecaoId).NotEmpty().WithMessage("A excecao e obrigatoria.");
            RuleFor(c => c.AprovadorId).NotEmpty().WithMessage("O aprovador e obrigatorio.");
        }
    }

    /// <summary>D-SOD-02 — renova exceção aprovada/vencida (respeita SOD_PRAZO_MAX_EXCECAO se definido).</summary>
    public record RenovarExcecaoSoDCommand(Guid ExcecaoId, DateTime NovaDataFim) : ICommand;

    public class RenovarExcecaoSoDCommandValidator : AbstractValidator<RenovarExcecaoSoDCommand>
    {
        public RenovarExcecaoSoDCommandValidator()
        {
            RuleFor(c => c.ExcecaoId).NotEmpty().WithMessage("A excecao e obrigatoria.");
        }
    }

    /// <summary>
    /// RN-SOD-039 — varredura de exceções vencidas: marca as aprovadas com DataFim &lt; agora como
    /// Vencidas e REATIVA a violação mitigada correspondente. Executada por job de reconciliação.
    /// </summary>
    public record ExpirarExcecoesVencidasSoDCommand() : ICommand;

    /// <summary>
    /// D-SOD-04 — registra um bypass de SoD (inclusive do admin do tenant). Nunca silencioso:
    /// grava o registro, exige controle compensatório, dispara alerta (evento) e trilha central (T8).
    /// </summary>
    public record RegistrarBypassAdminSoDCommand(
        Guid RegraId,
        Guid AtorId,
        bool AtorEhAdmin,
        string Motivo,
        Guid? ControleCompensatorioId,
        string? ControleCompensatorio
    ) : ICommand;

    public class RegistrarBypassAdminSoDCommandValidator : AbstractValidator<RegistrarBypassAdminSoDCommand>
    {
        public RegistrarBypassAdminSoDCommandValidator()
        {
            RuleFor(c => c.RegraId).NotEmpty().WithMessage("A regra e obrigatoria.");
            RuleFor(c => c.AtorId).NotEmpty().WithMessage("O ator e obrigatorio.");
            RuleFor(c => c.Motivo).NotEmpty().WithMessage("O motivo do bypass e obrigatorio.");
        }
    }
}
