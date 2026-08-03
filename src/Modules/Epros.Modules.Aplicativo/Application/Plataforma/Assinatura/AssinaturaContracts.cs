using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Assinatura
{
    // ===================== Commands =====================

    public record SignatarioInput(string Nome, string Identificacao, bool Externo, bool Obrigatorio, int Ordem);

    /// <summary>
    /// Abre a solicitação de assinatura de um documento do GED. Signatários externos recebem link
    /// público (token). O mínimo de assinaturas vem da política do tipo documental, quando existir,
    /// senão do número de signatários obrigatórios.
    /// </summary>
    public record SolicitarAssinaturasCommand(
        Guid DocumentoId,
        string? TipoDocumento,
        string? TipoAssinatura,
        bool ExigeOrdem,
        List<SignatarioInput> Signatarios
    ) : ICommand;

    public class SolicitarAssinaturasCommandValidator : AbstractValidator<SolicitarAssinaturasCommand>
    {
        public SolicitarAssinaturasCommandValidator()
        {
            RuleFor(c => c.DocumentoId).NotEmpty().WithMessage("O documento é obrigatório.");
            RuleFor(c => c.Signatarios).NotEmpty().WithMessage("Informe ao menos um signatário.");
        }
    }

    /// <summary>Registra a assinatura de um signatário interno (passa pela abstração ICP).</summary>
    public record RegistrarAssinaturaCommand(Guid SolicitacaoId, Guid SignatarioId) : ICommand;

    /// <summary>Assinatura pública por link (signatário externo). Token único não revogado.</summary>
    public record AssinarPorLinkPublicoCommand(string LinkToken) : ICommand;

    public record RecusarAssinaturaCommand(Guid SolicitacaoId, Guid SignatarioId, string Motivo) : ICommand;

    public record RevogarLinkPublicoCommand(Guid SignatarioId) : ICommand;

    public record DefinirPoliticaAssinaturaCommand(
        string TipoDocumento, int MinAssinaturas, string TipoAssinatura, bool ExigeOrdem) : ICommand;

    // ===================== Queries =====================

    public record ObterSolicitacoesAssinaturaQuery(string? Estado = null, Guid? DocumentoId = null)
        : IQuery<IReadOnlyList<SolicitacaoAssinaturaDto>>;

    public record ObterSolicitacaoAssinaturaPorIdQuery(Guid Id) : IQuery<SolicitacaoAssinaturaDetalheDto?>;

    // ===================== DTOs =====================

    public record SolicitacaoAssinaturaDto(
        Guid Id, Guid DocumentoId, int VersaoDocumento, string Estado, bool LinkPublico,
        int MinAssinaturas, bool ExigeOrdem, string TipoAssinatura, DateTime CriadoEm);

    public record SignatarioDto(Guid Id, int Ordem, bool Obrigatorio, bool Externo, string Nome,
        string Identificacao, string Status, bool LinkAtivo);

    public record SolicitacaoAssinaturaDetalheDto(
        SolicitacaoAssinaturaDto Solicitacao, IReadOnlyList<SignatarioDto> Signatarios, int Assinadas);
}
