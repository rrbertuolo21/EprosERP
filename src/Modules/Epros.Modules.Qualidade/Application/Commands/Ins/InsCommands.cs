using System;
using Epros.Modules.Qualidade.Domain.Enums;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Qualidade.Application.Commands.Ins
{
    // ============ QLD-INS — ciclo de vida do plano + execucao ============

    /// <summary>Adiciona uma caracteristica ao plano (so em Rascunho/EmAnalise).</summary>
    public record AdicionarCaracteristicaPlanoCommand(
        Guid PlanoId,
        int Sequencia,
        string Nome,
        ETipoCaracteristica TipoCaracteristica,
        ETipoDadoCaracteristica TipoDado,
        bool Obrigatoria,
        Guid? AtributoId,
        Guid? UnidadeMedidaId,
        string? ValorNominal,
        decimal? LimiteInferior,
        decimal? LimiteSuperior,
        string? CriterioQualitativo
    ) : ICommand;

    public class AdicionarCaracteristicaPlanoCommandValidator : AbstractValidator<AdicionarCaracteristicaPlanoCommand>
    {
        public AdicionarCaracteristicaPlanoCommandValidator()
        {
            RuleFor(c => c.PlanoId).NotEmpty();
            RuleFor(c => c.Sequencia).GreaterThanOrEqualTo(1);
            RuleFor(c => c.Nome).NotEmpty().MaximumLength(255);
        }
    }

    /// <summary>Adiciona uma regra de amostragem (AQL/percentual/fixa/100%) ao plano.</summary>
    public record AdicionarRegraAmostragemCommand(
        Guid PlanoId,
        ETipoAmostragem TipoAmostragem,
        Guid? CaracteristicaId,
        string? NivelInspecao,
        string? Aql,
        decimal? FaixaLoteMin,
        decimal? FaixaLoteMax,
        int? TamanhoAmostra,
        int? CriterioAceite,
        int? CriterioRejeicao,
        string? Severidade
    ) : ICommand;

    public class AdicionarRegraAmostragemCommandValidator : AbstractValidator<AdicionarRegraAmostragemCommand>
    {
        public AdicionarRegraAmostragemCommandValidator()
        {
            RuleFor(c => c.PlanoId).NotEmpty();
            RuleFor(c => c.Aql).NotEmpty().When(c => c.TipoAmostragem == ETipoAmostragem.AQL)
                .WithMessage("O AQL e obrigatorio para amostragem do tipo AQL.");
        }
    }

    /// <summary>Ativa o plano (RN-INS-004/007: exige >=1 caracteristica; so Ativo executa).</summary>
    public record AtivarPlanoInspecaoCommand(Guid PlanoId) : ICommand;

    public class AtivarPlanoInspecaoCommandValidator : AbstractValidator<AtivarPlanoInspecaoCommand>
    {
        public AtivarPlanoInspecaoCommandValidator() => RuleFor(c => c.PlanoId).NotEmpty();
    }

    /// <summary>Transiciona o status do plano (Suspenso/Encerrado exigem motivo).</summary>
    public record AlterarStatusPlanoInspecaoCommand(
        Guid PlanoId,
        EStatusRegistroQualidade NovoStatus,
        string? Motivo
    ) : ICommand;

    public class AlterarStatusPlanoInspecaoCommandValidator : AbstractValidator<AlterarStatusPlanoInspecaoCommand>
    {
        public AlterarStatusPlanoInspecaoCommandValidator() => RuleFor(c => c.PlanoId).NotEmpty();
    }

    /// <summary>
    /// Abre uma execucao de inspecao para uma referencia (recebimento/lote/OP). Quando o nivel + AQL
    /// sao informados (ou herdados de regra AQL do plano), o tamanho da amostra e calculado pelo motor AQL.
    /// </summary>
    public record ExecutarInspecaoCommand(
        Guid PlanoId,
        EReferenciaExecucao ReferenciaTipo,
        string? ReferenciaId,
        decimal QuantidadeLote,
        Guid? InspetorId,
        string? NivelInspecao,
        decimal? Aql,
        string? Severidade
    ) : ICommand;

    public class ExecutarInspecaoCommandValidator : AbstractValidator<ExecutarInspecaoCommand>
    {
        public ExecutarInspecaoCommandValidator()
        {
            RuleFor(c => c.PlanoId).NotEmpty();
            RuleFor(c => c.QuantidadeLote).GreaterThan(0);
        }
    }
}
