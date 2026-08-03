using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.GRC.Application.Commands
{
    // GRC-CIA avançado — amostra (D-CIA-02), token de acesso externo (D-CIA-04), evidência e
    // aprovação/encerramento de achado (RN-CIA-013/014).

    /// <summary>D-CIA-02 — define a amostra do teste (método híbrido, justificativa obrigatória).</summary>
    public record DefinirAmostraAuditoriaCommand(
        Guid? TesteControleId,
        Guid? PlanoAuditoriaId,
        string Metodo, // Estatistico, Dirigido, Censo
        int Tamanho,
        string? Criterio,
        string Justificativa
    ) : ICommand;

    public class DefinirAmostraAuditoriaCommandValidator : AbstractValidator<DefinirAmostraAuditoriaCommand>
    {
        public DefinirAmostraAuditoriaCommandValidator()
        {
            RuleFor(c => c.Metodo).Must(m => m == "Estatistico" || m == "Dirigido" || m == "Censo")
                .WithMessage("O metodo deve ser 'Estatistico', 'Dirigido' ou 'Censo'.");
            RuleFor(c => c.Tamanho).GreaterThan(0);
            RuleFor(c => c.Justificativa).NotEmpty().WithMessage("A justificativa do metodo de amostragem e obrigatoria.");
        }
    }

    /// <summary>
    /// D-CIA-04 — emite token de acesso externo com expiração obrigatória. A duração vem do
    /// parâmetro CIA_TOKEN_EXPIRACAO_HORAS (default 72h). Retorna o token EM CLARO uma única vez.
    /// </summary>
    public record EmitirTokenAcessoAuditoriaCommand(
        Guid PlanoAuditoriaId,
        Guid? AuditorId,
        string? Escopo,
        int? ExpiracaoHoras
    ) : ICommand;

    public class EmitirTokenAcessoAuditoriaCommandValidator : AbstractValidator<EmitirTokenAcessoAuditoriaCommand>
    {
        public EmitirTokenAcessoAuditoriaCommandValidator()
        {
            RuleFor(c => c.PlanoAuditoriaId).NotEmpty();
        }
    }

    /// <summary>D-CIA-04 — revoga um token de acesso externo.</summary>
    public record RevogarTokenAcessoAuditoriaCommand(Guid TokenId) : ICommand;

    /// <summary>RN-CIA-014 — registra evidência (vinculada a achado ou teste de controle).</summary>
    public record RegistrarEvidenciaAuditoriaCommand(
        Guid? AchadoId,
        Guid? TesteControleId,
        Guid ArquivoId,
        string? Descricao
    ) : ICommand;

    public class RegistrarEvidenciaAuditoriaCommandValidator : AbstractValidator<RegistrarEvidenciaAuditoriaCommand>
    {
        public RegistrarEvidenciaAuditoriaCommandValidator()
        {
            RuleFor(c => c.ArquivoId).NotEmpty();
            RuleFor(c => c).Must(c => c.AchadoId != null || c.TesteControleId != null)
                .WithMessage("A evidencia deve estar vinculada a um achado ou teste de controle.");
        }
    }

    /// <summary>RN-CIA-013 — aprova achado (obrigatório para crítico antes de encerrar).</summary>
    public record AprovarAchadoCommand(Guid AchadoId, Guid AprovadorId) : ICommand;

    public class AprovarAchadoCommandValidator : AbstractValidator<AprovarAchadoCommand>
    {
        public AprovarAchadoCommandValidator()
        {
            RuleFor(c => c.AchadoId).NotEmpty();
            RuleFor(c => c.AprovadorId).NotEmpty();
        }
    }

    /// <summary>RN-CIA-013/014 — encerra achado exigindo evidência e (se crítico) aprovação.</summary>
    public record EncerrarAchadoCommand(Guid AchadoId, string Motivo) : ICommand;

    public class EncerrarAchadoCommandValidator : AbstractValidator<EncerrarAchadoCommand>
    {
        public EncerrarAchadoCommandValidator()
        {
            RuleFor(c => c.AchadoId).NotEmpty();
            RuleFor(c => c.Motivo).NotEmpty();
        }
    }
}
