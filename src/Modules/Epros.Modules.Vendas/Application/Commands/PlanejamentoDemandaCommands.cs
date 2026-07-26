using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ===================== Planejamento de Demanda (VEN-PDM) =====================
    // Fonte: EF_7_VENDAS_PLANEJAMENTO_DE_DEMANDA_V1. Estilo B (ICommand).

    public record CriarDemandaPrevisaoCommand(
        Guid? EmpresaId,
        string? Codigo,
        string Nome,
        DateTime PeriodoInicio,
        DateTime PeriodoFim,
        string? Observacoes) : ICommand;

    public record AdicionarDemandaItemCommand(
        Guid PrevisaoId,
        Guid? CenarioId,
        Guid? VersaoId,
        Guid ProdutoId,
        string Periodo,
        decimal? QuantidadeHistorica,
        decimal QuantidadePrevista,
        Guid? UnidadeMedidaId,
        string? Observacoes) : ICommand;

    public record AjustarDemandaItemCommand(Guid ItemId, decimal QuantidadeAjustada) : ICommand;

    public record CriarDemandaCenarioCommand(Guid PrevisaoId, string Nome, string? Tipo, string? Descricao) : ICommand;

    public record AprovarDemandaPrevisaoCommand(Guid PrevisaoId, Guid? CenarioVigenteId, Guid? VersaoVigenteId, string? Observacoes) : ICommand;

    /// <summary>Publica a demanda aprovada para estoque/produção (EF §16 — bloqueia previsão não aprovada).</summary>
    public record PublicarDemandaCommand(Guid PrevisaoId) : ICommand;
}
