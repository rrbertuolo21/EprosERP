using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.ESG.Application.Commands
{
    // ===== ESG-ECO (Economia Circular) =====

    public record DevolucaoItemInput(
        string? Codigo,
        string? Nome,
        string? Ncm,
        string? Cfop,
        decimal? ValorUnitario,
        decimal? Quantidade,
        bool ItemParcial,
        string? UnidadeMedida
    );

    public record ImportarDevolucaoCommand(
        Guid? ContactId,
        Guid? NaturezaId,
        decimal? ValorIntegral,
        decimal? ValorDevolvido,
        string? Motivo,
        string? Observacao,
        string? Estado,
        bool DevolucaoParcial,
        string? ChaveNfEntrada,
        string? NumeroNf,
        decimal? ValorFrete,
        decimal? ValorDesconto,
        Guid? BusinessId,
        Guid? LocationId,
        string? Tipo,
        List<DevolucaoItemInput> Itens
    ) : ICommand;

    public record CriarFluxoCircularCommand(
        string Codigo,
        string Descricao,
        string Tipo,
        Guid? DevolucaoId,
        Guid ResponsavelId
    ) : ICommand;

    public record RegistrarTriagemCommand(
        Guid FluxoId,
        Guid? ItemDevolucaoId,
        decimal QuantidadeRecebida,
        string Unidade,
        string Condicao,
        string DestinoProposto,
        string? Motivo,
        Guid ResponsavelId
    ) : ICommand;

    public record RegistrarDestinoCommand(
        Guid TriagemId,
        string TipoDestino,
        decimal Quantidade,
        string Unidade,
        DateTime DataExecucao,
        Guid ResponsavelId,
        Guid? EvidenciaArquivoId,
        string? Observacao
    ) : ICommand;

    public record DefinirMetaCircularCommand(
        Guid FluxoId,
        string TipoIndicador,
        DateTime PeriodoInicio,
        DateTime PeriodoFim,
        decimal ValorMeta,
        string Unidade,
        string? Formula,
        Guid ResponsavelId
    ) : ICommand;

    public record RegistrarMedicaoCircularCommand(
        Guid FluxoId,
        string TipoIndicador,
        string Periodo,
        decimal? Numerador,
        decimal? Denominador,
        string Unidade,
        string? Fonte
    ) : ICommand;
}
