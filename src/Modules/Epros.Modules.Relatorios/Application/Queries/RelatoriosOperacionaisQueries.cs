using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Relatorios.Application.Queries
{
    // ---------------------------------------------------------------------
    // RPT-OPB — Relatorios Operacionais (Openbook). Queries somente leitura.
    // Todas retornam CommandResult (padrao do projeto). O isolamento por empresa
    // (tenant) e aplicado pelo global query filter do ContextBase (RN-001).
    // ---------------------------------------------------------------------

    // ---- ESTOQUE -------------------------------------------------------------
    /// <summary>Resumo de posicao de estoque (EF tela /app/reports/stock-summary).</summary>
    public sealed record PosicaoEstoqueQuery(
        Guid? CategoriaId = null,
        Guid? ProdutoId = null,
        bool SomenteAbaixoMinimo = false) : IQuery<CommandResult>;

    /// <summary>Giro de estoque por produto no periodo (saidas / saldo).</summary>
    public sealed record GiroEstoqueQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        Guid? ProdutoId = null) : IQuery<CommandResult>;

    // ---- VENDAS --------------------------------------------------------------
    /// <summary>Vendas agregadas por dia no periodo (EF /app/reports/sales-by-item base).</summary>
    public sealed record VendasPorPeriodoQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        bool IncluirCanceladas = false) : IQuery<CommandResult>;

    /// <summary>Vendas por produto/item (EF /app/reports/sales-by-item).</summary>
    public sealed record VendasPorProdutoQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        Guid? ProdutoId = null,
        bool IncluirCanceladas = false) : IQuery<CommandResult>;

    /// <summary>Vendas por cliente (EF /app/reports/sales-customer). Cliente = zero/null => todos (RN-014).</summary>
    public sealed record VendasPorClienteQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        Guid? ClienteId = null,
        bool IncluirCanceladas = false) : IQuery<CommandResult>;

    // ---- COMPRAS -------------------------------------------------------------
    /// <summary>Compras por item (EF /app/reports/purchase-by-item).</summary>
    public sealed record ComprasPorItemQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        Guid? ProdutoId = null,
        bool IncluirCanceladas = false) : IQuery<CommandResult>;

    /// <summary>Compras por parceiro/fornecedor (EF /app/reports/purchase-supplier).</summary>
    public sealed record ComprasPorParceiroQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        string? FornecedorCnpj = null,
        bool IncluirCanceladas = false) : IQuery<CommandResult>;

    // ---- FINANCEIRO ----------------------------------------------------------
    /// <summary>Aging de Contas a Receber (EF /app/reports/receivables-summary).</summary>
    public sealed record AgingContasReceberQuery(
        DateTime? DataReferencia = null) : IQuery<CommandResult>;

    /// <summary>Aging de Contas a Pagar (EF /app/reports/payable-summary).</summary>
    public sealed record AgingContasPagarQuery(
        DateTime? DataReferencia = null) : IQuery<CommandResult>;

    /// <summary>Resumo de titulos a receber (lista com saldo e situacao de vencimento).</summary>
    public sealed record ResumoContasReceberQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        int? Situacao = null,
        bool SomenteEmAberto = false) : IQuery<CommandResult>;

    /// <summary>Resumo de titulos a pagar.</summary>
    public sealed record ResumoContasPagarQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        int? Situacao = null,
        bool SomenteEmAberto = false) : IQuery<CommandResult>;

    /// <summary>Pagamentos recebidos no periodo (EF /app/reports/payment-received).</summary>
    public sealed record PagamentosRecebidosQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null) : IQuery<CommandResult>;

    /// <summary>Pagamentos efetuados no periodo (EF /app/reports/payment-made).</summary>
    public sealed record PagamentosEfetuadosQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null) : IQuery<CommandResult>;

    /// <summary>Fluxo de caixa por dia (entradas x saidas) a partir de MovimentoFinanceiro.</summary>
    public sealed record FluxoCaixaQuery(
        DateTime? Inicio = null,
        DateTime? Fim = null,
        bool IncluirPlanejamento = false) : IQuery<CommandResult>;
}
