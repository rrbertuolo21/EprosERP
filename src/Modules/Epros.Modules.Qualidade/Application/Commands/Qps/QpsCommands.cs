using System;
using System.Collections.Generic;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Qualidade.Application.Commands.Qps
{
    /// <summary>Abre um registro de qualificacao/homologacao de fornecedor (parceiro de suprimento).</summary>
    public record CriarQpsRegistroCommand(string Codigo, Guid ParceiroId, Guid ResponsavelId, string? NomeParceiro) : ICommand;

    public class CriarQpsRegistroCommandValidator : AbstractValidator<CriarQpsRegistroCommand>
    {
        public CriarQpsRegistroCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().MaximumLength(30);
            RuleFor(c => c.ParceiroId).NotEmpty();
            RuleFor(c => c.ResponsavelId).NotEmpty();
        }
    }

    /// <summary>Homologa o fornecedor com validade (vencimento dispara re-homologacao).</summary>
    public record HomologarFornecedorCommand(Guid RegistroId, DateTime DataValidade) : ICommand;

    public class HomologarFornecedorCommandValidator : AbstractValidator<HomologarFornecedorCommand>
    {
        public HomologarFornecedorCommandValidator() => RuleFor(c => c.RegistroId).NotEmpty();
    }

    /// <summary>Bloqueia o fornecedor (manual/automatico) — exige motivo + alcada.</summary>
    public record BloquearFornecedorCommand(Guid RegistroId, EQpsTipoBloqueio TipoBloqueio, string Motivo, Guid? AlcadaId) : ICommand;

    public class BloquearFornecedorCommandValidator : AbstractValidator<BloquearFornecedorCommand>
    {
        public BloquearFornecedorCommandValidator()
        {
            RuleFor(c => c.RegistroId).NotEmpty();
            RuleFor(c => c.Motivo).NotEmpty();
        }
    }

    /// <summary>Desbloqueia o fornecedor (encerra o bloqueio ativo e volta a re-homologacao).</summary>
    public record DesbloquearFornecedorCommand(Guid RegistroId) : ICommand;

    public class DesbloquearFornecedorCommandValidator : AbstractValidator<DesbloquearFornecedorCommand>
    {
        public DesbloquearFornecedorCommandValidator() => RuleFor(c => c.RegistroId).NotEmpty();
    }

    public record IndicadorScoreDto(string Codigo, decimal Valor, decimal Peso, string? Fonte);

    /// <summary>
    /// Calcula o scorecard do fornecedor no periodo a partir dos indicadores (rastreaveis a rejeicoes/NCR).
    /// O motor aplica a media ponderada; formula/pesos/limiar sao parametros da Siser (D14, valida).
    /// </summary>
    public record CalcularScoreFornecedorCommand(
        Guid RegistroId,
        string Periodo,
        List<IndicadorScoreDto> Indicadores,
        decimal? LimiteBloqueio
    ) : ICommand;

    public class CalcularScoreFornecedorCommandValidator : AbstractValidator<CalcularScoreFornecedorCommand>
    {
        public CalcularScoreFornecedorCommandValidator()
        {
            RuleFor(c => c.RegistroId).NotEmpty();
            RuleFor(c => c.Periodo).NotEmpty().MaximumLength(50);
        }
    }

    /// <summary>Anexa um documento de homologacao (com validade) ao registro.</summary>
    public record AdicionarDocumentoQpsCommand(
        Guid RegistroId,
        EQpsTipoDocumento TipoDocumento,
        string Titulo,
        string? Numero,
        DateTime? DataValidade,
        Guid? ArquivoId
    ) : ICommand;

    public class AdicionarDocumentoQpsCommandValidator : AbstractValidator<AdicionarDocumentoQpsCommand>
    {
        public AdicionarDocumentoQpsCommandValidator()
        {
            RuleFor(c => c.RegistroId).NotEmpty();
            RuleFor(c => c.Titulo).NotEmpty().MaximumLength(255);
        }
    }
}
