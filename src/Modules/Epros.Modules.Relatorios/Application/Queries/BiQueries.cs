using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Relatorios.Application.Queries
{
    // ---------------------------------------------------------------------
    // RPT-ONM — BI (OneManager). KPIs gerenciais e series temporais multi-modulo.
    // Somente leitura; isolamento por tenant via global query filter (RN-BI-001).
    // ---------------------------------------------------------------------

    /// <summary>KPI de faturamento no periodo (soma de vendas nao canceladas).</summary>
    public sealed record KpiFaturamentoQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null) : IQuery<CommandResult>;

    /// <summary>Serie temporal de faturamento (granularidade dia ou mes).</summary>
    public sealed record SerieFaturamentoQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        string Granularidade = "Dia") : IQuery<CommandResult>;

    /// <summary>KPI de margem bruta (receita - custo) / receita, a partir dos itens de venda.</summary>
    public sealed record KpiMargemQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null) : IQuery<CommandResult>;

    /// <summary>KPI de inadimplencia (saldo vencido / saldo total de recebiveis).</summary>
    public sealed record KpiInadimplenciaQuery(
        DateTime? DataReferencia = null) : IQuery<CommandResult>;

    /// <summary>KPI de ruptura de estoque (itens zerados / abaixo do minimo).</summary>
    public sealed record KpiRupturaQuery() : IQuery<CommandResult>;

    /// <summary>Top N produtos por valor ou quantidade vendida no periodo.</summary>
    public sealed record TopProdutosQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        int TopN = 10,
        string Criterio = "Valor") : IQuery<CommandResult>;

    /// <summary>Painel gerencial consolidado (faturamento, margem, inadimplencia, ruptura, caixa).</summary>
    public sealed record PainelGerencialQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null) : IQuery<CommandResult>;
}
