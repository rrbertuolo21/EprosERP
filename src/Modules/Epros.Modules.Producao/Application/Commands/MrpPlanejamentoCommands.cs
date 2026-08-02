using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using FluentValidation;

namespace Epros.Modules.Producao.Application.Commands
{
    // ===================== PRD-MRP — MRP / Planejamento Integrado IBP =====================
    // Motor MRP (explosão BOM, netting, sugestões) é lacuna controlada — ver DP-MRP-002..006 na EF.

    public record CriarMrpPlanejamentoCommand(string Codigo, Guid ResponsavelId) : ICommand;

    public class CriarMrpPlanejamentoCommandValidator : AbstractValidator<CriarMrpPlanejamentoCommand>
    {
        public CriarMrpPlanejamentoCommandValidator()
        {
            RuleFor(c => c.Codigo).NotEmpty().WithMessage("O código do planejamento é obrigatório. (RN-MRP-002)");
            RuleFor(c => c.ResponsavelId).NotEmpty().WithMessage("O responsável é obrigatório. (RN-MRP-004)");
        }
    }

    public record SubmeterMrpPlanejamentoCommand(Guid Id) : ICommand;
    public record AprovarMrpPlanejamentoCommand(Guid Id) : ICommand;
    public record RejeitarMrpPlanejamentoCommand(Guid Id, string Motivo) : ICommand;
    public record InativarMrpPlanejamentoCommand(Guid Id) : ICommand;
    public record ReativarMrpPlanejamentoCommand(Guid Id) : ICommand;
    public record EncerrarMrpPlanejamentoCommand(Guid Id) : ICommand;

    // ===================== Motor MRP (PD3/PD21) =====================

    /// <summary>Item de demanda de entrada do MRP (produto final ou item avulso).</summary>
    public record MrpDemandaDto(Guid ItemId, decimal Quantidade, DateTime? DataReferencia = null, Guid? VariacaoId = null);

    /// <summary>Parâmetros de suprimento por item (disponibilidade/recebimentos/segurança/lote).</summary>
    public record MrpParametroItemDto(
        Guid ItemId,
        decimal Disponibilidade = 0m,
        decimal RecebimentosProgramados = 0m,
        decimal EstoqueSeguranca = 0m,
        decimal LoteMinimo = 0m,
        decimal LoteMultiplo = 0m);

    /// <summary>
    /// PD3/PD21 — dispara o motor MRP: explode a BOM vigente, faz o netting e persiste necessidades e sugestões.
    /// Recalcula preservando sugestões já convertidas (idempotência PRD-INT-CA-002).
    /// </summary>
    public record CalcularMrpCommand(
        Guid PlanejamentoId,
        List<MrpDemandaDto> Demandas,
        List<MrpParametroItemDto>? Parametros = null) : ICommand;

    /// <summary>Submete a sugestão do MRP para aprovação (PD20).</summary>
    public record SubmeterSugestaoMrpCommand(Guid SugestaoId) : ICommand;
    /// <summary>Aprova a sugestão do MRP.</summary>
    public record AprovarSugestaoMrpCommand(Guid SugestaoId) : ICommand;
    /// <summary>Converte a sugestão aprovada em documento (pedido de compra / ordem de produção).</summary>
    public record ConverterSugestaoMrpCommand(Guid SugestaoId, Guid DocumentoGeradoId) : ICommand;
    /// <summary>Cancela a sugestão do MRP.</summary>
    public record CancelarSugestaoMrpCommand(Guid SugestaoId, string Motivo) : ICommand;
}
