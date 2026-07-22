using System;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Fiscal.Application.Commands
{
    // ======= Sub-DTOs de entrada (espelham NfseDtos do legado, versão enxuta e neutra) =======

    public record NfsePrestadorCmd(
        string Documento,
        int Crt = 3,
        string? InscricaoMunicipal = null,
        string? RazaoSocial = null,
        int CodigoMunicipioIbge = 0,
        string? Uf = null);

    public record NfseTomadorCmd(
        string Documento,
        string? InscricaoMunicipal = null,
        string? RazaoSocial = null);

    public record NfseRpsCmd(
        string Numero,
        string Serie,
        int Tipo = 1,
        DateTime? DataEmissao = null);

    public record NfseServicoCmd(
        string ItemListaServico,
        decimal ValorServicos,
        decimal AliquotaIss = 0,
        decimal ValorIss = 0,
        decimal ValorIssRetido = 0,
        decimal ValorDeducoes = 0,
        decimal DescontoIncondicionado = 0,
        decimal DescontoCondicionado = 0,
        int IssRetido = 1,
        string? CodigoCnae = null,
        string? CodigoTributacaoMunicipio = null,
        string? CodigoNbs = null,
        string? Discriminacao = null,
        int CodigoMunicipioIbge = 0,
        int ExigibilidadeIss = 1,
        int MunicipioIncidencia = 0);

    /// <summary>Emite um lote de RPS (uma NFS-e). Fiel ao legado <c>nfse/emitir-lote</c>.</summary>
    public record EmitirLoteNfseCommand(
        int NumeroLote,
        NfseRpsCmd Rps,
        NfsePrestadorCmd Prestador,
        NfseTomadorCmd Tomador,
        NfseServicoCmd Servico,
        bool Sincrono = true,
        int Ambiente = 2,
        int NaturezaOperacao = 1,
        int RegimeEspecialTributacao = 0,
        bool OptanteSimplesNacional = false,
        bool IncentivoFiscal = false,
        DateTime? Competencia = null
    ) : ICommand;

    public class EmitirLoteNfseCommandValidator : AbstractValidator<EmitirLoteNfseCommand>
    {
        public EmitirLoteNfseCommandValidator()
        {
            RuleFor(c => c.NumeroLote).GreaterThan(0).WithMessage("O número do lote é obrigatório.");
            RuleFor(c => c.Rps).NotNull();
            RuleFor(c => c.Rps.Numero).NotEmpty().WithMessage("O número do RPS é obrigatório.");
            RuleFor(c => c.Rps.Serie).NotEmpty().WithMessage("A série do RPS é obrigatória.");
            RuleFor(c => c.Prestador.Documento).NotEmpty().WithMessage("O documento do prestador é obrigatório.");
            RuleFor(c => c.Tomador.Documento).NotEmpty().WithMessage("O documento do tomador é obrigatório.");
            RuleFor(c => c.Servico.ItemListaServico).NotEmpty().WithMessage("O item da lista de serviço é obrigatório.");
            RuleFor(c => c.Servico.ValorServicos).GreaterThan(0).WithMessage("O valor dos serviços deve ser maior que zero.");
        }
    }

    /// <summary>Consulta a situação de um lote de NFS-e. Fiel ao legado <c>nfse/consultar-lote</c>.</summary>
    public record ConsultarLoteNfseCommand(
        int NumeroLote,
        string Protocolo,
        NfsePrestadorCmd Prestador,
        int Ambiente = 2
    ) : ICommand;

    public class ConsultarLoteNfseCommandValidator : AbstractValidator<ConsultarLoteNfseCommand>
    {
        public ConsultarLoteNfseCommandValidator()
        {
            RuleFor(c => c.Protocolo).NotEmpty().WithMessage("O protocolo é obrigatório.");
            RuleFor(c => c.Prestador.Documento).NotEmpty().WithMessage("O documento do prestador é obrigatório.");
        }
    }

    /// <summary>Consulta a NFS-e gerada a partir de um RPS. Fiel ao legado <c>nfse/consultar-por-rps</c>.</summary>
    public record ConsultarNfsePorRpsCommand(
        string NumeroRps,
        string Serie,
        NfsePrestadorCmd Prestador,
        int Tipo = 1,
        int MesCompetencia = 0,
        int AnoCompetencia = 0,
        int Ambiente = 2
    ) : ICommand;

    public class ConsultarNfsePorRpsCommandValidator : AbstractValidator<ConsultarNfsePorRpsCommand>
    {
        public ConsultarNfsePorRpsCommandValidator()
        {
            RuleFor(c => c.NumeroRps).NotEmpty().WithMessage("O número do RPS é obrigatório.");
            RuleFor(c => c.Serie).NotEmpty().WithMessage("A série do RPS é obrigatória.");
            RuleFor(c => c.Prestador.Documento).NotEmpty().WithMessage("O documento do prestador é obrigatório.");
        }
    }

    /// <summary>Cancela uma NFS-e autorizada. Fiel ao legado <c>nfse/cancelar</c>.</summary>
    public record CancelarNfseCommand(
        string NumeroNfse,
        string CodigoCancelamento,
        NfsePrestadorCmd Prestador,
        string? Motivo = null,
        int Ambiente = 2
    ) : ICommand;

    public class CancelarNfseCommandValidator : AbstractValidator<CancelarNfseCommand>
    {
        public CancelarNfseCommandValidator()
        {
            RuleFor(c => c.NumeroNfse).NotEmpty().WithMessage("O número da NFS-e é obrigatório.");
            RuleFor(c => c.CodigoCancelamento).NotEmpty().WithMessage("O código de cancelamento é obrigatório.");
            RuleFor(c => c.Prestador.Documento).NotEmpty().WithMessage("O documento do prestador é obrigatório.");
        }
    }
}
