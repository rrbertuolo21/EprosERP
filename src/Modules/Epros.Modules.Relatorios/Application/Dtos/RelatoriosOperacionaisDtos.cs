using System;
using System.Collections.Generic;

namespace Epros.Modules.Relatorios.Application.Dtos
{
    // ---------------------------------------------------------------------
    // RPT-OPB — DTOs de resultado dos relatorios operacionais (Openbook).
    // Contratos de resultado (EF secao 10/11): estruturas de leitura, nao persistentes.
    // ---------------------------------------------------------------------

    // ---- ESTOQUE -------------------------------------------------------------
    public sealed record PosicaoEstoqueLinhaDto(
        Guid ProdutoId,
        string Sku,
        string Produto,
        Guid? CategoriaId,
        string? Categoria,
        decimal SaldoEstoque,
        decimal EstoqueMinimo,
        decimal EstoqueReservado,
        decimal ValorSaldo,
        decimal CustoMedio,
        bool AbaixoDoMinimo);

    public sealed record PosicaoEstoqueRelatorioDto(
        IReadOnlyList<PosicaoEstoqueLinhaDto> Linhas,
        decimal TotalSaldoEstoque,
        decimal TotalValorSaldo,
        int QuantidadeItens);

    public sealed record GiroEstoqueLinhaDto(
        Guid ProdutoId,
        string Produto,
        decimal QuantidadeSaida,
        decimal SaldoAtual,
        decimal Giro); // saidas / saldo medio (aproximado por saldo atual)

    public sealed record GiroEstoqueRelatorioDto(
        IReadOnlyList<GiroEstoqueLinhaDto> Linhas,
        decimal TotalSaida);

    // ---- VENDAS --------------------------------------------------------------
    public sealed record VendasPorPeriodoLinhaDto(
        DateTime Dia,
        int QuantidadeVendas,
        decimal ValorTotal);

    public sealed record VendasPorPeriodoRelatorioDto(
        IReadOnlyList<VendasPorPeriodoLinhaDto> Linhas,
        decimal ValorTotal,
        int QuantidadeVendas);

    public sealed record VendasPorProdutoLinhaDto(
        Guid ProdutoId,
        string Produto,
        decimal Quantidade,
        decimal ValorTotal);

    public sealed record VendasPorProdutoRelatorioDto(
        IReadOnlyList<VendasPorProdutoLinhaDto> Linhas,
        decimal ValorTotal);

    public sealed record VendasPorClienteLinhaDto(
        Guid? ClienteId,
        int QuantidadeVendas,
        decimal ValorTotal);

    public sealed record VendasPorClienteRelatorioDto(
        IReadOnlyList<VendasPorClienteLinhaDto> Linhas,
        decimal ValorTotal);

    // ---- COMPRAS -------------------------------------------------------------
    public sealed record ComprasPorItemLinhaDto(
        Guid ProdutoId,
        string Produto,
        decimal Quantidade,
        decimal ValorTotal);

    public sealed record ComprasPorItemRelatorioDto(
        IReadOnlyList<ComprasPorItemLinhaDto> Linhas,
        decimal ValorTotal);

    public sealed record ComprasPorParceiroLinhaDto(
        string FornecedorCnpj,
        string Fornecedor,
        int QuantidadeCompras,
        decimal ValorTotal);

    public sealed record ComprasPorParceiroRelatorioDto(
        IReadOnlyList<ComprasPorParceiroLinhaDto> Linhas,
        decimal ValorTotal);

    // ---- FINANCEIRO — TITULOS (CP/CR) ---------------------------------------
    /// <summary>Uma faixa (bucket) de aging de titulos.</summary>
    public sealed record AgingFaixaDto(
        string Faixa,
        int QuantidadeTitulos,
        decimal SaldoDevido);

    /// <summary>
    /// Resultado de aging de Contas a Pagar / Contas a Receber.
    /// Faixas: A vencer, 1-30, 31-60, 61-90, Acima de 90 (dias vencidos).
    /// Definicao de "vencido": DataVencimento &lt; referencia E saldo devido &gt; 0
    /// (EF RELATORIOS_OPERACIONAIS RN-016).
    /// </summary>
    public sealed record AgingRelatorioDto(
        string Natureza, // "ContasAPagar" | "ContasAReceber"
        DateTime DataReferencia,
        IReadOnlyList<AgingFaixaDto> Faixas,
        decimal SaldoTotal,
        decimal SaldoVencido,
        decimal SaldoAVencer);

    public sealed record TituloLinhaDto(
        Guid TituloId,
        string Documento,
        Guid PessoaId,
        string? NomePessoa,
        DateTime DataEmissao,
        DateTime DataVencimento,
        int Situacao,
        decimal ValorTitulo,
        decimal SaldoDevido,
        bool Vencido);

    public sealed record TitulosRelatorioDto(
        string Natureza,
        IReadOnlyList<TituloLinhaDto> Linhas,
        decimal ValorTotal,
        decimal SaldoDevidoTotal,
        int QuantidadeTitulos);

    // ---- FINANCEIRO — PAGAMENTOS / FLUXO ------------------------------------
    public sealed record PagamentoLinhaDto(
        DateTime Data,
        decimal ValorPago,
        int TipoPagamento);

    public sealed record PagamentosRelatorioDto(
        string Natureza, // "Efetuados" | "Recebidos"
        IReadOnlyList<PagamentoLinhaDto> Linhas,
        decimal ValorTotal,
        int Quantidade);

    public sealed record FluxoCaixaLinhaDto(
        DateTime Dia,
        decimal Entradas,
        decimal Saidas,
        decimal Saldo);

    public sealed record FluxoCaixaRelatorioDto(
        IReadOnlyList<FluxoCaixaLinhaDto> Linhas,
        decimal TotalEntradas,
        decimal TotalSaidas,
        decimal SaldoLiquido);
}
