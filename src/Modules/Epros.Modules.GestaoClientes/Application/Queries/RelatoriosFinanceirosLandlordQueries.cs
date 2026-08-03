using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    // 1.08G — Relatórios financeiros AGREGADOS do Landlord (Siser cobrando os tenants).
    //
    // ⛔ ESCOPO: apenas AGREGADOS FACTUAIS (regime de CAIXA) sobre dados que a 1.08A já grava.
    // NÃO há reconhecimento de receita por competência/diferimento, NEM apuração fiscal de comissão —
    // isso é ONDA 2 (skill de negócio + validação de contador). Aqui é somatório factual, por período.
    //
    // Segurança: operação LANDLORD — os handlers exigem operador interno (tenant "system") e o
    // controller aplica AbacAuthorize. Todos os intervalos são [Inicio, Fim] inclusivos nas duas pontas.

    /// <summary>1.08G — Receita por período (CAIXA): faturas pagas agrupadas por mês. Bruto/Tarifa/Líquido.</summary>
    public record ObterRelatorioReceitaPorPeriodoQuery(DateTime Inicio, DateTime Fim)
        : IQuery<RelatorioReceitaPeriodoDto>;

    /// <summary>1.08G — Inadimplência por período: faturas vencidas e não pagas, por mês/tenant.</summary>
    public record ObterRelatorioInadimplenciaPorPeriodoQuery(DateTime Inicio, DateTime Fim)
        : IQuery<RelatorioInadimplenciaPeriodoDto>;

    /// <summary>1.08G — Comissão por período (CAIXA): soma factual de ValorComissaoRevenda/Vendedor, por mês/revenda.</summary>
    public record ObterRelatorioComissaoPorPeriodoQuery(DateTime Inicio, DateTime Fim)
        : IQuery<RelatorioComissaoPeriodoDto>;

    // ===== DTOs de saída =====

    public record RelatorioReceitaPeriodoDto(
        DateTime Inicio,
        DateTime Fim,
        decimal TotalBruto,
        decimal TotalTarifa,
        decimal TotalLiquido,
        int TotalFaturas,
        IReadOnlyList<ReceitaMesDto> Meses
    );

    /// <summary>Linha mensal de receita CAIXA. Bruto = valor das faturas pagas; Tarifa = ValorTarifa dos
    /// pagamentos liquidados; Líquido = Bruto − Tarifa. Fatos da 1.08A; sem competência (Onda 2).</summary>
    public record ReceitaMesDto(
        int Ano,
        int Mes,
        decimal Bruto,
        decimal Tarifa,
        decimal Liquido,
        int QuantidadeFaturas
    );

    public record RelatorioInadimplenciaPeriodoDto(
        DateTime Inicio,
        DateTime Fim,
        decimal ValorTotal,
        int TotalFaturas,
        IReadOnlyList<InadimplenciaMesTenantDto> Itens
    );

    /// <summary>Linha de inadimplência por mês de vencimento e tenant (faturas vencidas e não pagas).</summary>
    public record InadimplenciaMesTenantDto(
        int Ano,
        int Mes,
        string TenantId,
        decimal ValorTotal,
        int QuantidadeFaturas
    );

    public record RelatorioComissaoPeriodoDto(
        DateTime Inicio,
        DateTime Fim,
        decimal TotalComissaoRevenda,
        decimal TotalComissaoVendedor,
        int TotalFaturas,
        IReadOnlyList<ComissaoMesRevendaDto> Itens
    );

    /// <summary>Linha de comissão por mês (do pagamento) e revenda. Somatório FACTUAL dos campos já gravados;
    /// a apuração/retenção fiscal da comissão é Onda 2 (skill + contador).</summary>
    public record ComissaoMesRevendaDto(
        int Ano,
        int Mes,
        Guid? RevendaId,
        decimal ComissaoRevenda,
        decimal ComissaoVendedor,
        int QuantidadeFaturas
    );
}
