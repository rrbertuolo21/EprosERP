using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    // ============================================================================================
    //  Comandos que DISPARAM os motores de domínio da Produção que estavam órfãos (sem caller):
    //  BomExplosaoService, CustoRollupService, CapacidadeService, SequenciamentoService.
    //  São cálculos sob demanda (compute-and-return): a BOM/centro vêm do EF; o resultado volta
    //  no CommandResult. Persistência de necessidade/sugestão é do fluxo MRP (CalcularMrpCommand).
    // ============================================================================================

    /// <summary>PRD-BOM — explode a estrutura vigente da raiz informada (multinível, com desperdício).</summary>
    public record ExplodirBomCommand(
        Guid EstruturaRaizId,
        decimal QuantidadeSolicitada,
        DateTime? DataReferencia = null) : ICommand;

    public class ExplodirBomCommandValidator : AbstractValidator<ExplodirBomCommand>
    {
        public ExplodirBomCommandValidator()
        {
            RuleFor(c => c.EstruturaRaizId).NotEmpty();
            RuleFor(c => c.QuantidadeSolicitada).GreaterThan(0);
        }
    }

    /// <summary>
    /// PRD-CST — custo PREVISTO por rollup multinível. Taxas de MOD/CIF são valida-contador
    /// (default 0 = nada presumido); horas/valoração de material de folha ficam como resíduo (integração Estoque).
    /// </summary>
    public record CalcularCustoRollupCommand(
        Guid EstruturaRaizId,
        decimal TaxaMaoDeObraPorHora = 0m,
        decimal TaxaCifPorHoraMaquina = 0m,
        DateTime? DataReferencia = null) : ICommand;

    public class CalcularCustoRollupCommandValidator : AbstractValidator<CalcularCustoRollupCommand>
    {
        public CalcularCustoRollupCommandValidator()
        {
            RuleFor(c => c.EstruturaRaizId).NotEmpty();
            RuleFor(c => c.TaxaMaoDeObraPorHora).GreaterThanOrEqualTo(0);
            RuleFor(c => c.TaxaCifPorHoraMaquina).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>PRD-PLN/CTR — avalia carga × capacidade de um centro de trabalho (gargalo/ATP).</summary>
    public record AvaliarCapacidadeCommand(
        Guid CentroTrabalhoId,
        decimal CargaMinutos,
        int DiasUteis) : ICommand;

    public class AvaliarCapacidadeCommandValidator : AbstractValidator<AvaliarCapacidadeCommand>
    {
        public AvaliarCapacidadeCommandValidator()
        {
            RuleFor(c => c.CentroTrabalhoId).NotEmpty();
            RuleFor(c => c.CargaMinutos).GreaterThanOrEqualTo(0);
            RuleFor(c => c.DiasUteis).GreaterThanOrEqualTo(0);
        }
    }

    /// <summary>Operação de entrada do sequenciamento (APS capacidade finita).</summary>
    public record OperacaoSequenciamentoDto(
        Guid Id,
        Guid CentroTrabalhoId,
        int Prioridade,
        decimal DuracaoMinutos,
        decimal SetupMinutos,
        List<Guid>? Precedencias = null,
        DateTime? JanelaInicio = null);

    /// <summary>PRD-ESC — sequencia operações por precedência/prioridade com capacidade finita por centro.</summary>
    public record SequenciarOperacoesCommand(
        List<OperacaoSequenciamentoDto> Operacoes,
        DateTime? InicioHorizonte = null) : ICommand;

    public class SequenciarOperacoesCommandValidator : AbstractValidator<SequenciarOperacoesCommand>
    {
        public SequenciarOperacoesCommandValidator()
        {
            RuleFor(c => c.Operacoes).NotEmpty().WithMessage("Informe ao menos uma operação para sequenciar.");
        }
    }
}
