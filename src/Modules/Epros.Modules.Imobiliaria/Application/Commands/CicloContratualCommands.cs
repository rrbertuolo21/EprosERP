using System;
using System.Collections.Generic;
using Epros.Modules.Imobiliaria.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Imobiliaria.Application.Commands
{
    // ==================== Reajuste (ID7/NF-02) ====================

    /// <summary>
    /// Aplica um reajuste ratificado a locacao vigente (ID7/NF-02). O reajuste vem DESLIGADO por
    /// padrao e o indice (IGP-M/IPCA/INPC) e parametrico. O NovoValor e o valor RATIFICADO pelo
    /// contador (valida-contador) — a IMOBILIARIA nao calcula o indice/arredondamento.
    /// </summary>
    public record AplicarReajusteCommand(
        Guid LocacaoId,
        decimal NovoValor,
        string? Indice = null,
        DateTime? DataBase = null,
        decimal? PercentualAplicado = null
    ) : ICommand;

    public class AplicarReajusteCommandValidator : AbstractValidator<AplicarReajusteCommand>
    {
        public AplicarReajusteCommandValidator()
        {
            RuleFor(c => c.LocacaoId).NotEmpty();
            RuleFor(c => c.NovoValor).GreaterThan(0);
        }
    }

    // ==================== Rescisao (ID7) ====================

    /// <summary>
    /// Rescinde/encerra a locacao vigente (ID7): registra motivo, data, aviso previo, multa
    /// proporcional e vistoria de saida; encerra a locacao e libera o imovel. O CALCULO da multa
    /// e valida-contador (NF-02) — persiste-se o valor informado (default 0).
    /// </summary>
    public record RescindirLocacaoCommand(
        Guid LocacaoId,
        string Motivo,
        DateTime DataRescisao,
        int? AvisoPrevioDias = null,
        decimal MultaProporcional = 0m,
        Guid? VistoriaSaidaId = null
    ) : ICommand;

    public class RescindirLocacaoCommandValidator : AbstractValidator<RescindirLocacaoCommand>
    {
        public RescindirLocacaoCommandValidator()
        {
            RuleFor(c => c.LocacaoId).NotEmpty();
            RuleFor(c => c.Motivo).NotEmpty().WithMessage("O motivo da rescisao e obrigatorio.");
            RuleFor(c => c.MultaProporcional).GreaterThanOrEqualTo(0);
        }
    }

    // ==================== Garantias (ID6/NF-04) ====================

    /// <summary>Registra uma garantia da locacao (ID6). Tratamento financeiro da caucao = NF-04.</summary>
    public record AdicionarGarantiaCommand(
        Guid LocacaoId,
        ETipoGarantia Tipo,
        decimal ValorLimite,
        DateTime? VigenciaInicio = null,
        DateTime? VigenciaFim = null,
        string? Descricao = null,
        Guid? FiadorPessoaId = null
    ) : ICommand;

    public class AdicionarGarantiaCommandValidator : AbstractValidator<AdicionarGarantiaCommand>
    {
        public AdicionarGarantiaCommandValidator()
        {
            RuleFor(c => c.LocacaoId).NotEmpty();
            RuleFor(c => c.ValorLimite).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>Substitui uma garantia ativa por outra (ID6).</summary>
    public record SubstituirGarantiaCommand(
        Guid GarantiaAnteriorId,
        ETipoGarantia Tipo,
        decimal ValorLimite,
        DateTime? VigenciaInicio = null,
        DateTime? VigenciaFim = null,
        string? Descricao = null,
        Guid? FiadorPessoaId = null
    ) : ICommand;

    public class SubstituirGarantiaCommandValidator : AbstractValidator<SubstituirGarantiaCommand>
    {
        public SubstituirGarantiaCommandValidator()
        {
            RuleFor(c => c.GarantiaAnteriorId).NotEmpty();
            RuleFor(c => c.ValorLimite).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>Libera uma garantia (ID6). Tratamento da caucao permanece off ate contador (NF-04).</summary>
    public record LiberarGarantiaCommand(Guid GarantiaId) : ICommand;

    // ==================== Proposta (ID2) ====================

    public record PropostaParteInput(Guid PessoaId, EPapelParteProposta Papel);

    /// <summary>Cria uma proposta imobiliaria (ID2). Dono = IMOBILIARIA.</summary>
    public record CriarPropostaCommand(
        ETipoProposta Tipo,
        Guid ImovelId,
        DateTime Validade,
        decimal ValorProposto,
        string? Observacao,
        Guid? ContrapropostaDeId,
        List<PropostaParteInput>? Partes
    ) : ICommand;

    public class CriarPropostaCommandValidator : AbstractValidator<CriarPropostaCommand>
    {
        public CriarPropostaCommandValidator()
        {
            RuleFor(c => c.ImovelId).NotEmpty();
            RuleFor(c => c.ValorProposto).GreaterThan(0);
        }
    }

    public record AprovarPropostaCommand(Guid PropostaId) : ICommand;
    public record RejeitarPropostaCommand(Guid PropostaId) : ICommand;

    /// <summary>
    /// Converte a proposta aprovada (ID2). Para Tipo=Locacao gera a locacao (EmElaboracao) com o
    /// valor/proponentes da proposta e vinculo ao imovel. Aquisicao apenas marca convertida
    /// (contrato de venda pertence a VENDAS — fronteira).
    /// </summary>
    public record ConverterPropostaCommand(
        Guid PropostaId,
        DateTime? PeriodoInicial = null,
        DateTime? PeriodoFinal = null,
        int Vencimento = 10
    ) : ICommand;

    public class ConverterPropostaCommandValidator : AbstractValidator<ConverterPropostaCommand>
    {
        public ConverterPropostaCommandValidator()
        {
            RuleFor(c => c.PropostaId).NotEmpty();
            RuleFor(c => c.Vencimento).InclusiveBetween(1, 31);
        }
    }
}
