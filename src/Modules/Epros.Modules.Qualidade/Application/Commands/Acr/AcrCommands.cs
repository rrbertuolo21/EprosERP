using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Modules.Qualidade.Domain.Services.Aql;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Qualidade.Application.Commands.Acr
{
    /// <summary>Adiciona um item a uma analise de aceite/rejeicao (secao 12.2).</summary>
    public record AdicionarItemAcrCommand(
        Guid AnaliseId,
        int Sequencia,
        decimal QuantidadeAnalisada,
        string UnidadeMedida,
        bool ItemParcial,
        Guid? ProdutoId,
        string? CodigoItem,
        string? NomeItem,
        string? Lote,
        decimal? QuantidadeIntegral,
        string? Ncm,
        string? Cfop,
        decimal? ValorUnitario,
        string? Observacao
    ) : ICommand;

    public class AdicionarItemAcrCommandValidator : AbstractValidator<AdicionarItemAcrCommand>
    {
        public AdicionarItemAcrCommandValidator()
        {
            RuleFor(c => c.AnaliseId).NotEmpty();
            RuleFor(c => c.QuantidadeAnalisada).GreaterThan(0);
            RuleFor(c => c.UnidadeMedida).NotEmpty().MaximumLength(20);
        }
    }

    /// <summary>Submete a analise (RN: exige >=1 item).</summary>
    public record SubmeterAcrCommand(Guid AnaliseId) : ICommand;

    public class SubmeterAcrCommandValidator : AbstractValidator<SubmeterAcrCommand>
    {
        public SubmeterAcrCommandValidator() => RuleFor(c => c.AnaliseId).NotEmpty();
    }

    /// <summary>
    /// Decide a disposicao da analise (Aceito/Rejeitado/Quarentena/AceitoComDesvio/Reinspecao).
    /// Emite intencao ao Estoque (bloquear/liberar/quarentena) e, por gatilho, sugestao de NCR — via Outbox.
    /// Qualidade decide; NAO movimenta saldo nem emite documento fiscal (D3/D21).
    /// </summary>
    public record DecidirAnaliseAcrCommand(
        Guid AnaliseId,
        EResultadoAcr Resultado,
        Guid? MotivoId,
        string? Severidade,
        decimal? QuantidadeAfetada,
        string? Justificativa,
        Guid? AprovadoPor,
        bool StatusFiscalIgnorado,
        string? Lote
    ) : ICommand;

    public class DecidirAnaliseAcrCommandValidator : AbstractValidator<DecidirAnaliseAcrCommand>
    {
        public DecidirAnaliseAcrCommandValidator() => RuleFor(c => c.AnaliseId).NotEmpty();
    }

    /// <summary>
    /// Avalia a analise por amostragem AQL: dado N + nivel + AQL + severidade + defeituosos observados,
    /// o motor decide aceitar/rejeitar e a decisao e registrada (com os mesmos efeitos de DecidirAnalise).
    /// </summary>
    public record AvaliarAcrPorAmostragemCommand(
        Guid AnaliseId,
        long TamanhoLote,
        ENivelInspecao Nivel,
        decimal Aql,
        ESeveridadeAql Severidade,
        int Defeituosos,
        Guid? MotivoId,
        string? SeveridadeMotivo,
        Guid? AprovadoPor,
        string? Lote
    ) : ICommand;

    public class AvaliarAcrPorAmostragemCommandValidator : AbstractValidator<AvaliarAcrPorAmostragemCommand>
    {
        public AvaliarAcrPorAmostragemCommandValidator()
        {
            RuleFor(c => c.AnaliseId).NotEmpty();
            RuleFor(c => c.TamanhoLote).GreaterThanOrEqualTo(1);
            RuleFor(c => c.Defeituosos).GreaterThanOrEqualTo(0);
        }
    }
}
