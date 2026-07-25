using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    // ---------------- Avaria (EF §7.9 / §15.12) ----------------

    /// <summary>Registra e aplica avaria/perda como saída controlada do estoque (MVM-026).</summary>
    public record CriarAvariaEstoqueCommand(
        Guid EmpresaId,
        Guid ProdutoId,
        string? Codigo,
        string Nome,
        Guid CategoriaId,
        decimal PrecoCompra,
        decimal Quantidade,
        DateTime DataAvaria,
        string? Nota,
        string? Referencia
    ) : ICommand;

    // ---------------- Transferência (EF §7.7 / §15.6-15.7) ----------------

    public record TransferenciaItemInput(
        Guid ProdutoId,
        decimal Quantidade,
        decimal? ValorUnitario = null,
        string? Lote = null,
        DateTime? DataValidade = null
    );

    /// <summary>Cria transferência entre locais, validando origem≠destino (MVM-020) e saldo (MVM-021).</summary>
    public record CriarTransferenciaEstoqueCommand(
        Guid EmpresaId,
        Guid LocalOrigemId,
        Guid LocalDestinoId,
        DateTime DataTransferencia,
        decimal? ValorFrete,
        string? Observacao,
        List<TransferenciaItemInput> Itens
    ) : ICommand;

    // ---------------- Requisição interna (EF §7.10 / §15.13-15.14) ----------------

    public record RequisicaoInternaItemInput(Guid ProdutoId, decimal Quantidade);

    /// <summary>Cria requisição interna (cabeçalho + itens). Saída efetiva ocorre no atendimento (MVM-027).</summary>
    public record CriarRequisicaoInternaCommand(
        Guid ColaboradorId,
        DateTime DataRequisicao,
        EStatusRequisicaoInterna Situacao,
        List<RequisicaoInternaItemInput> Itens
    ) : ICommand;

    // ---------------- Saldo inicial (EF §7.5-7.6 / §15.8-15.9) ----------------

    public record SaldoInicialItemInput(
        string ProdutoCodigo,
        Guid? ProdutoId,
        string? LocalNome,
        Guid? LocalId,
        decimal Quantidade,
        decimal CustoUnitario,
        string? Lote = null,
        DateTime? DataValidade = null
    );

    /// <summary>
    /// Registra/importa saldo inicial por produto (MVM-023/024). Gera resumo de linhas
    /// aceitas/rejeitadas (CA-MVM-011) e cria ficha/saldo para as linhas válidas.
    /// </summary>
    public record ImportarSaldoInicialCommand(
        Guid EmpresaId,
        string ArquivoNome,
        List<SaldoInicialItemInput> Itens
    ) : ICommand;
}
