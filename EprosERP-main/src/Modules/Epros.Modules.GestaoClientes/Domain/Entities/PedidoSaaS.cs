using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PedidoSaaS : EntidadeSaaSBase
    {
        public Guid ClienteId { get; private set; }
        public Guid PlanoId { get; private set; }
        public Guid? CupomId { get; private set; }
        public decimal ValorBase { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorTotal { get; private set; }
        public string Moeda { get; private set; } = "BRL";
        public string MetodoPagamento { get; private set; } = string.Empty; // PIX, Manual, Gateway, Transferencia
        public PedidoSaaSStatus Status { get; private set; } = PedidoSaaSStatus.Pending;
        public Guid? AssinaturaCriadaId { get; private set; }

        protected PedidoSaaS() { } // EF Core

        public PedidoSaaS(
            Guid clienteId,
            Guid planoId,
            Guid? cupomId,
            decimal valorBase,
            decimal valorDesconto,
            string moeda,
            string metodoPagamento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PedidoSaaS>()
                .Requires()
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .AreNotEquals(planoId, Guid.Empty, nameof(PlanoId), "PlanoId é obrigatório")
                .IsGreaterThan(valorBase, 0, nameof(ValorBase), "Valor base do pedido deve ser maior que zero")
                .IsNotNullOrEmpty(moeda, nameof(Moeda), "Moeda é obrigatória")
                .IsNotNullOrEmpty(metodoPagamento, nameof(MetodoPagamento), "Método de pagamento é obrigatório")
            );

            ClienteId = clienteId;
            PlanoId = planoId;
            CupomId = cupomId;
            ValorBase = valorBase;
            ValorDesconto = valorDesconto;
            ValorTotal = Math.Max(0, valorBase - valorDesconto);
            Moeda = moeda;
            MetodoPagamento = metodoPagamento;
            Status = PedidoSaaSStatus.Pending;
        }

        public void Liquidar(Guid assinaturaId, string alteradoPor)
        {
            Status = PedidoSaaSStatus.Succeeded;
            AssinaturaCriadaId = assinaturaId;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarFalha(string alteradoPor)
        {
            Status = PedidoSaaSStatus.Failed;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarReembolsado(string alteradoPor)
        {
            Status = PedidoSaaSStatus.Refunded;
            MarcarAlterado(alteradoPor);
        }
    }
}
