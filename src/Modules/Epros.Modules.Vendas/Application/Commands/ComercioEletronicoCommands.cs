using System;
using System.Collections.Generic;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ===================== Comércio Eletrônico (VEN-ECM) =====================
    // Fonte: EF_7_VENDAS_COMERCIO_ELETRONICO_V1. Estilo B (ICommand).

    public record SalvarEcoConfiguracaoLojaCommand(
        string? Nome, string? Rua, string? Numero, string? Bairro, string? Cidade, string? Uf, string? Cep,
        string? Telefone, string? Email, decimal FreteGratisValor, string? PagamentoPublicKey,
        string? PagamentoAccessToken, string? TokenLoja) : ICommand;

    public record CriarEcoClienteCommand(string Nome, string? Sobrenome, string? Cpf, string? InscricaoEstadual, string Email, string? Telefone, string SenhaHash) : ICommand;

    public record CriarEcoCupomCommand(string Codigo, decimal Valor, EEcoTipoCupom Tipo) : ICommand;

    public record ItemPedidoInput(Guid ProdutoId, decimal Quantidade, Guid? VariacaoId, decimal ValorUnitario);

    public record CriarEcoPedidoCommand(
        Guid ClienteId,
        Guid EnderecoId,
        decimal ValorFrete,
        string? TipoFrete,
        string? CupomDesconto,
        string? Observacao,
        List<ItemPedidoInput> Itens) : ICommand;

    public record IniciarPagamentoPedidoCommand(
        Guid PedidoId,
        EEcoFormaPagamento Forma,
        string? TransacaoId,
        string? QrCode,
        string? QrCodeBase64,
        string? LinkBoleto) : ICommand;

    public record ConfirmarPagamentoPedidoCommand(Guid PedidoId, string? StatusPagamento, string? StatusDetalhe) : ICommand;

    /// <summary>ECO-035/ECO-037: converte o pedido e-commerce em Venda preservando o vínculo; emissão fiscal fica no Fiscal.</summary>
    public record ConverterEcoPedidoEmVendaCommand(Guid PedidoId) : ICommand;

    public record AtualizarRastreioPedidoCommand(Guid PedidoId, string CodigoRastreio) : ICommand;
}
