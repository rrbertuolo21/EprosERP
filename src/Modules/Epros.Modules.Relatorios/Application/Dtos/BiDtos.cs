using System;
using System.Collections.Generic;

namespace Epros.Modules.Relatorios.Application.Dtos
{
    // ---------------------------------------------------------------------
    // RPT-ONM — DTOs de resultado do BI (OneManager): KPIs e series temporais.
    // Metrica certificada carrega formula, unidade e granularidade
    // (EF RELATORIOS/BI_E_ANALYTICS RN-BI-002).
    // ---------------------------------------------------------------------

    /// <summary>Valor unico de KPI com metadados de metrica (formula/unidade).</summary>
    public sealed record KpiValorDto(
        string Kpi,
        decimal Valor,
        string Unidade,
        string Formula,
        DateTime? Inicio,
        DateTime? Fim);

    /// <summary>Um ponto de uma serie temporal (granularidade diaria/mensal).</summary>
    public sealed record SerieTemporalPontoDto(
        DateTime Periodo,
        decimal Valor);

    public sealed record SerieTemporalDto(
        string Kpi,
        string Unidade,
        string Granularidade, // "Dia" | "Mes"
        IReadOnlyList<SerieTemporalPontoDto> Pontos,
        decimal Total);

    /// <summary>KPI de margem: receita, custo e margem percentual.</summary>
    public sealed record MargemKpiDto(
        decimal Receita,
        decimal Custo,
        decimal MargemValor,
        decimal MargemPercentual,
        DateTime? Inicio,
        DateTime? Fim);

    /// <summary>KPI de inadimplencia: saldo vencido sobre saldo total de recebiveis.</summary>
    public sealed record InadimplenciaKpiDto(
        decimal SaldoTotal,
        decimal SaldoVencido,
        decimal PercentualInadimplencia,
        DateTime DataReferencia);

    /// <summary>KPI de ruptura: itens com saldo zerado ou abaixo do minimo.</summary>
    public sealed record RupturaKpiDto(
        int TotalItens,
        int ItensEmRuptura,
        int ItensAbaixoMinimo,
        decimal PercentualRuptura);

    /// <summary>Painel gerencial consolidado multi-modulo (visao unica do gestor).</summary>
    public sealed record PainelGerencialDto(
        decimal FaturamentoPeriodo,
        MargemKpiDto Margem,
        InadimplenciaKpiDto Inadimplencia,
        RupturaKpiDto Ruptura,
        decimal SaldoCaixaLiquido,
        DateTime? Inicio,
        DateTime? Fim);

    public sealed record TopProdutoDto(
        Guid ProdutoId,
        string Produto,
        decimal Quantidade,
        decimal ValorTotal);

    public sealed record TopProdutosDto(
        string Criterio, // "Valor" | "Quantidade"
        int TopN,
        IReadOnlyList<TopProdutoDto> Itens);
}
