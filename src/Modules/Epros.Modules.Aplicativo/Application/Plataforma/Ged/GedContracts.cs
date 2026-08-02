using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Aplicativo.Application.Plataforma.Ged
{
    // ===================== PLT · GED — Commands =====================

    /// <summary>
    /// Registra um documento no GED canônico único. Se <see cref="ConteudoBase64"/> for informado, o
    /// hash (SHA-256) e o tamanho são calculados e o conteúdo é persistido via storage abstrato;
    /// caso contrário usa <see cref="HashInformado"/> + <see cref="TamanhoBytes"/> (registro lógico).
    /// </summary>
    public record RegistrarDocumentoGedCommand(
        string Nome,
        string TipoDocumento,
        string? ConteudoBase64,
        string? HashInformado,
        long TamanhoBytes,
        string? MimeType,
        bool RequerAssinatura,
        string? ModuloOrigem,
        string? EntidadeOrigemTipo,
        string? EntidadeOrigemId
    ) : ICommand;

    public class RegistrarDocumentoGedCommandValidator : AbstractValidator<RegistrarDocumentoGedCommand>
    {
        public RegistrarDocumentoGedCommandValidator()
        {
            RuleFor(c => c.Nome).NotEmpty().WithMessage("O nome do documento é obrigatório.");
            RuleFor(c => c.TipoDocumento).NotEmpty().WithMessage("O tipo do documento é obrigatório.");
            RuleFor(c => c)
                .Must(c => !string.IsNullOrWhiteSpace(c.ConteudoBase64) || !string.IsNullOrWhiteSpace(c.HashInformado))
                .WithMessage("Informe o conteúdo (base64) ou o hash + tamanho do documento.");
        }
    }

    public record RegistrarNovaVersaoDocumentoGedCommand(
        Guid DocumentoId,
        string? ConteudoBase64,
        string? HashInformado,
        long TamanhoBytes
    ) : ICommand;

    public record VincularDocumentoGedCommand(
        Guid DocumentoId,
        string ModuloDestino,
        string EntidadeTipo,
        string EntidadeId
    ) : ICommand;

    public record DefinirPoliticaRetencaoGedCommand(
        string TipoDocumento,
        int PrazoRetencaoDias,
        string BaseLegal,
        string AcaoAposPrazo,
        bool HoldJuridico
    ) : ICommand;

    public record BloquearHashGedCommand(string Hash, string Motivo) : ICommand;

    // ===================== PLT · GED — Queries =====================

    public record ObterDocumentosGedQuery(
        string? TipoDocumento = null,
        string? EntidadeOrigemTipo = null,
        string? EntidadeOrigemId = null
    ) : IQuery<IReadOnlyList<DocumentoGedDto>>;

    public record ObterDocumentoGedPorIdQuery(Guid Id) : IQuery<DocumentoGedDto?>;

    public record BuscarDocumentosGedQuery(string Termo) : IQuery<IReadOnlyList<DocumentoGedDto>>;

    /// <summary>Documentos cujo prazo de retenção venceu e que não estão sob hold jurídico.</summary>
    public record ObterDocumentosRetencaoVencidaQuery() : IQuery<IReadOnlyList<DocumentoRetencaoVencidaDto>>;

    // ===================== PLT · GED — DTOs =====================

    public record DocumentoGedDto(
        Guid Id,
        string Nome,
        string TipoDocumento,
        string Hash,
        long TamanhoBytes,
        string? MimeType,
        int Versao,
        string? StorageRef,
        string StatusAssinatura,
        string? ModuloOrigem,
        string? EntidadeOrigemTipo,
        string? EntidadeOrigemId,
        DateTime CriadoEm
    );

    public record DocumentoRetencaoVencidaDto(
        Guid Id,
        string Nome,
        string TipoDocumento,
        DateTime CriadoEm,
        int PrazoRetencaoDias,
        DateTime VenceEm,
        string AcaoAposPrazo,
        string BaseLegal
    );
}
