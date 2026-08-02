using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    /// <summary>RLT-003: cria um lote rastreável (código obrigatório).</summary>
    public record CriarLoteCommand(
        Guid EmpresaId,
        Guid ProdutoId,
        string CodigoLote,
        decimal QuantidadeRecebida,
        EOrigemLote Origem,
        Guid? LocalId = null,
        Guid? FichaEntradaId = null,
        DateTime? DataFabricacao = null,
        DateTime? DataValidade = null,
        string? Observacao = null
    ) : ICommand;

    /// <summary>RLT-004/005: registra um número serial (único no escopo produto/tenant).</summary>
    public record RegistrarNumeroSerialCommand(
        Guid ProdutoId,
        string Numero,
        Guid? LoteId = null,
        Guid? FichaEntradaId = null
    ) : ICommand;

    /// <summary>RLT-012: bloqueia (parte do) lote, exigindo motivo.</summary>
    public record BloquearLoteCommand(Guid LoteId, ETipoBloqueioLote TipoBloqueio, string Motivo, decimal? Quantidade = null) : ICommand;

    /// <summary>RLT-013: desbloqueia o lote.</summary>
    public record DesbloquearLoteCommand(Guid LoteId, string? MotivoDesbloqueio = null) : ICommand;

    /// <summary>RLT-008/011: abre um recall (bloqueia automaticamente o lote para consumo).</summary>
    public record AbrirRecallLoteCommand(Guid LoteId, string Motivo, string? CodigoRecall = null) : ICommand;

    /// <summary>Encerra (conclui) um recall.</summary>
    public record ConcluirRecallLoteCommand(Guid RecallId) : ICommand;
}
