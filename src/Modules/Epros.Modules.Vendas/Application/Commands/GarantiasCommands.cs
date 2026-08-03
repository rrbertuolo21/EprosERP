using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ===================== Garantias (VEN-GAR) =====================
    // Fonte: EF_7_VENDAS_GARANTIAS_V1. Estilo B (ICommand) — mesmo do CRM/GSV neste módulo.

    public record CriarGarantiaPoliticaCommand(
        string? Nome,
        string? Descricao,
        int Duracao,
        EGarantiaTipoDuracao TipoDuracao,
        decimal? LimiteUso = null,
        EGarantiaUnidadeUso UnidadeUso = EGarantiaUnidadeUso.Nenhuma) : ICommand;

    public record AtualizarGarantiaPoliticaCommand(
        Guid Id,
        string? Nome,
        string? Descricao,
        int Duracao,
        EGarantiaTipoDuracao TipoDuracao,
        decimal? LimiteUso = null,
        EGarantiaUnidadeUso UnidadeUso = EGarantiaUnidadeUso.Nenhuma) : ICommand;

    public record InativarGarantiaPoliticaCommand(Guid Id) : ICommand;

    public record AplicarGarantiaCoberturaCommand(
        Guid GarantiaPoliticaId,
        Guid? VendaId,
        Guid? VendaItemId,
        Guid? ProdutoId,
        Guid? ClienteId,
        string? NumeroSerieLote,
        DateTime? DataOrigem,
        string? Observacao,
        decimal? UsoOrigem = null) : ICommand;

    /// <summary>GAR-016: registra nova leitura de uso (km/horas) numa cobertura, reapurando a situação.</summary>
    public record RegistrarUsoGarantiaCoberturaCommand(Guid Id, decimal UsoAtual) : ICommand;
}
