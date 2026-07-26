using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PagamentoGlobal : EntidadeSaaSBase
    {
        public Guid AssinaturaId { get; private set; }
        public Guid? PedidoId { get; private set; }
        public Guid? FaturaId { get; private set; }
        public DateTime DataPagamento { get; private set; }
        public decimal Valor { get; private set; }
        public string Gateway { get; private set; } = string.Empty; // PIX, Manual, Transferencia, MercadoPago, etc.
        public string TransactionId { get; private set; } = string.Empty;

        protected PagamentoGlobal() { } // EF Core

        public PagamentoGlobal(
            Guid assinaturaId,
            Guid? pedidoId,
            Guid? faturaId,
            DateTime dataPagamento,
            decimal valor,
            string gateway,
            string transactionId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PagamentoGlobal>()
                .Requires()
                .AreNotEquals(assinaturaId, Guid.Empty, nameof(AssinaturaId), "AssinaturaId é obrigatória")
                .IsGreaterThan(valor, 0, nameof(Valor), "Valor pago deve ser maior que zero")
                .IsNotNullOrEmpty(gateway, nameof(Gateway), "Gateway/Método de pagamento é obrigatório")
                .IsNotNullOrEmpty(transactionId, nameof(TransactionId), "ID da transação é obrigatório")
            );

            AssinaturaId = assinaturaId;
            PedidoId = pedidoId;
            FaturaId = faturaId;
            DataPagamento = dataPagamento;
            Valor = valor;
            Gateway = gateway;
            TransactionId = transactionId;
        }
    }
}
