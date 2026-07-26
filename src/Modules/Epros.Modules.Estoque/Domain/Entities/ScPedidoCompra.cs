using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Pedido de compra (EF Sourcing e Compras §5.9 / §9.3 `sc_pedido_compra`).
    /// TipoPedidoId e FornecedorId são FKs Guid (fornecedor é referência externa).
    /// CotacaoId opcional quando o pedido nasce de uma cotação vencedora. SC-060: pedido possui itens.
    /// </summary>
    public class ScPedidoCompra : EntidadeSaaSBase
    {
        public Guid TipoPedidoId { get; private set; }
        public Guid FornecedorId { get; private set; }
        public Guid? CotacaoId { get; private set; }
        public DateTime? DataPedido { get; private set; }
        public DateTime? DataPrevistaEntrega { get; private set; }
        public DateTime? DataPrevisaoPagamento { get; private set; }
        public string LocalEntrega { get; private set; } = string.Empty;
        public string LocalCobranca { get; private set; } = string.Empty;
        public string Contato { get; private set; } = string.Empty;
        public string FormaPagamento { get; private set; } = string.Empty;
        public int? QuantidadeParcelas { get; private set; }
        public int? DiasPrimeiroVencimento { get; private set; }
        public int? DiasIntervalo { get; private set; }

        // Navegação intra-módulo
        public ScTipoPedido? TipoPedido { get; private set; }
        public ICollection<ScPedidoCompraItem> Itens { get; private set; } = new List<ScPedidoCompraItem>();

        protected ScPedidoCompra() { } // EF Core

        public ScPedidoCompra(
            Guid tipoPedidoId, Guid fornecedorId, Guid? cotacaoId,
            DateTime? dataPedido, DateTime? dataPrevistaEntrega, DateTime? dataPrevisaoPagamento,
            string localEntrega, string localCobranca, string contato, string formaPagamento,
            int? quantidadeParcelas, int? diasPrimeiroVencimento, int? diasIntervalo,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            TipoPedidoId = tipoPedidoId;
            FornecedorId = fornecedorId;
            CotacaoId = cotacaoId;
            DataPedido = dataPedido;
            DataPrevistaEntrega = dataPrevistaEntrega;
            DataPrevisaoPagamento = dataPrevisaoPagamento;
            LocalEntrega = localEntrega ?? string.Empty;
            LocalCobranca = localCobranca ?? string.Empty;
            Contato = contato ?? string.Empty;
            FormaPagamento = formaPagamento ?? string.Empty;
            QuantidadeParcelas = quantidadeParcelas;
            DiasPrimeiroVencimento = diasPrimeiroVencimento;
            DiasIntervalo = diasIntervalo;
            Validar();
        }

        public void AdicionarItem(ScPedidoCompraItem item) => Itens.Add(item);

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ScPedidoCompra>()
                .Requires()
                .AreNotEquals(TipoPedidoId, Guid.Empty, nameof(TipoPedidoId), "O tipo de pedido é obrigatório [Origem: ScPedidoCompra]")
                .AreNotEquals(FornecedorId, Guid.Empty, nameof(FornecedorId), "O fornecedor do pedido é obrigatório [SC-045] [Origem: ScPedidoCompra]")
                .IsNotNullOrEmpty(LocalEntrega, nameof(LocalEntrega), "O local de entrega do pedido é obrigatório [Origem: ScPedidoCompra]")
                .IsNotNullOrEmpty(LocalCobranca, nameof(LocalCobranca), "O local de cobrança do pedido é obrigatório [Origem: ScPedidoCompra]")
                .IsNotNullOrEmpty(Contato, nameof(Contato), "O contato do pedido é obrigatório [Origem: ScPedidoCompra]")
                .IsNotNullOrEmpty(FormaPagamento, nameof(FormaPagamento), "A forma de pagamento do pedido é obrigatória [Origem: ScPedidoCompra]"));
        }
    }
}
