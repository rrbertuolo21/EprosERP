using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    public class CartaoDeCreditoFatura : EntidadeSaaSBase
    {
        public Guid CartaoDeCreditoId { get; private set; }
        public DateTime DataLancamento { get; private set; }
        public DateTime DataVencimento { get; private set; }
        public decimal Valor { get; private set; }
        public bool Pago { get; private set; }

        // Navigation
        public CartaoDeCredito CartaoDeCredito { get; private set; } = null!;

        protected CartaoDeCreditoFatura() { } // EF Core

        public CartaoDeCreditoFatura(
            Guid cartaoDeCreditoId,
            DateTime dataLancamento,
            DateTime dataVencimento,
            decimal valor,
            bool pago,
            string tenantId,
            string criadoPor) : base(tenantId, criadoPor)
        {
            CartaoDeCreditoId = cartaoDeCreditoId;
            DataLancamento = dataLancamento;
            DataVencimento = dataVencimento;
            Valor = valor;
            Pago = pago;
            Validar();
        }

        public void Alterar(
            Guid cartaoDeCreditoId,
            DateTime dataLancamento,
            DateTime dataVencimento,
            decimal valor,
            bool pago,
            string alteradoPor)
        {
            CartaoDeCreditoId = cartaoDeCreditoId;
            DataLancamento = dataLancamento;
            DataVencimento = dataVencimento;
            Valor = valor;
            Pago = pago;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<CartaoDeCreditoFatura>()
                .Requires()
                .AreNotEquals(CartaoDeCreditoId, Guid.Empty, nameof(CartaoDeCreditoId), "O cartão de crédito é obrigatório")
                .IsGreaterThan(Valor, 0m, nameof(Valor), "O valor deve ser maior que zero")
            );
        }
    }
}
