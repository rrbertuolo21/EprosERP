using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    // ============ TIPOS ============

    public record CriarScTipoRequisicaoCommand(string Descricao) : ICommand;
    public record CriarScTipoPedidoCommand(string Descricao) : ICommand;

    // ============ REQUISIÇÃO (EF §9.1) ============

    /// <summary>Item de requisição. ItemCotado é indicador funcional preservado do material.</summary>
    public record ScRequisicaoItemInput(
        Guid ProdutoId,
        decimal? Quantidade = null,
        decimal? QuantidadeCotada = null,
        string ItemCotado = "N"
    );

    /// <summary>SC-057: gravação recria itens conforme material.</summary>
    public record CriarScRequisicaoCommand(
        Guid TipoRequisicaoId,
        Guid ColaboradorId,
        DateTime? DataRequisicao,
        List<ScRequisicaoItemInput> Itens
    ) : ICommand;

    /// <summary>SC-056: exclusão de requisição exclui seus itens (soft delete).</summary>
    public record ExcluirScRequisicaoCommand(Guid Id) : ICommand;

    // ============ COTAÇÃO (EF §9.2) ============

    public record ScCotacaoItemInput(
        Guid ProdutoId,
        Guid? CotacaoFornecedorRef = null,
        decimal? Quantidade = null,
        decimal? ValorUnitario = null,
        decimal? ValorDesconto = null,
        decimal? ValorTotal = null
    );

    public record ScCotacaoFornecedorInput(
        Guid FornecedorId,
        string PrazoEntrega,
        string CondicoesPagamento,
        decimal Subtotal = 0m,
        decimal Desconto = 0m,
        decimal Total = 0m
    );

    /// <summary>SC-059: cotação possui estrutura hierárquica por fornecedor e detalhes.</summary>
    public record CriarScCotacaoCommand(
        DateTime? DataCotacao,
        string Descricao,
        string Situacao,
        List<ScCotacaoFornecedorInput> Fornecedores,
        List<ScCotacaoItemInput> Itens
    ) : ICommand;

    // ============ PEDIDO DE COMPRA (EF §9.3) ============

    public record ScPedidoCompraItemInput(
        Guid ProdutoId,
        decimal Quantidade,
        decimal ValorUnitario,
        decimal ValorDesconto = 0m,
        decimal ValorFrete = 0m,
        decimal ValorSeguro = 0m,
        decimal ValorOutrasDespesas = 0m,
        decimal ValorIpi = 0m,
        decimal ValorIcms = 0m,
        decimal ValorTotal = 0m
    );

    /// <summary>SC-060: pedido possui itens vinculados.</summary>
    public record CriarScPedidoCompraCommand(
        Guid TipoPedidoId,
        Guid FornecedorId,
        Guid? CotacaoId,
        DateTime? DataPedido,
        DateTime? DataPrevistaEntrega,
        DateTime? DataPrevisaoPagamento,
        string LocalEntrega,
        string LocalCobranca,
        string Contato,
        string FormaPagamento,
        int? QuantidadeParcelas,
        int? DiasPrimeiroVencimento,
        int? DiasIntervalo,
        List<ScPedidoCompraItemInput> Itens
    ) : ICommand;

    /// <summary>CD2 — escolhe o fornecedor vencedor da cotação após o mapa comparativo (SRC-020/021).</summary>
    public record SelecionarVencedorCotacaoCommand(Guid CotacaoId, Guid FornecedorId) : ICommand;
}
