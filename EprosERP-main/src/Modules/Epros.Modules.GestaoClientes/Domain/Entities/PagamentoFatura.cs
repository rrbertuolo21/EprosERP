using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class PagamentoFatura : EntidadeSaaSBase
    {
        public Guid FaturaId { get; private set; }
        public string TipoPagamento { get; private set; } = string.Empty; // PIX, Boleto, Manual, etc.
        public PagamentoFaturaStatus Status { get; private set; } = PagamentoFaturaStatus.Pending;
        public decimal ValorPago { get; private set; }
        public decimal? ValorTarifa { get; private set; }
        public string? IdentificadorPagamento { get; private set; }
        public bool PagoManualmente { get; private set; }
        public DateTime? DataPagamento { get; private set; }

        protected PagamentoFatura() { } // EF Core

        public PagamentoFatura(
            Guid faturaId,
            string tipoPagamento,
            PagamentoFaturaStatus status,
            decimal valorPago,
            decimal? valorTarifa,
            string? identificadorPagamento,
            bool pagoManualmente,
            DateTime? dataPagamento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<PagamentoFatura>()
                .Requires()
                .AreNotEquals(faturaId, Guid.Empty, nameof(FaturaId), "FaturaId é obrigatório")
                .IsNotNullOrEmpty(tipoPagamento, nameof(TipoPagamento), "Tipo de pagamento é obrigatório")
                .IsGreaterThan(valorPago, 0, nameof(ValorPago), "Valor pago deve ser maior que zero")
            );

            FaturaId = faturaId;
            TipoPagamento = tipoPagamento;
            Status = status;
            ValorPago = valorPago;
            ValorTarifa = valorTarifa;
            IdentificadorPagamento = identificadorPagamento;
            PagoManualmente = pagoManualmente;
            DataPagamento = dataPagamento;
        }

        public void Liquidar(decimal valorRealPago, decimal? tarifa, string alteradoPor)
        {
            Status = PagamentoFaturaStatus.Paid;
            ValorPago = valorRealPago;
            ValorTarifa = tarifa;
            DataPagamento = DateTime.UtcNow;
            MarcarAlterado(alteradoPor);
        }

        public void MarcarFalha(string alteradoPor)
        {
            Status = PagamentoFaturaStatus.Failed;
            MarcarAlterado(alteradoPor);
        }
    }
}
