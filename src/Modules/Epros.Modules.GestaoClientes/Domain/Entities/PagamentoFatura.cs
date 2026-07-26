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

        // Dados da cobrança PIX gerada no gateway (melhoria: o legado não persistia).
        public string? QrCode { get; private set; }         // PIX "copia e cola" (payload EMV)
        public string? QrCodeBase64 { get; private set; }   // imagem do QR em base64
        public string? TicketUrl { get; private set; }      // URL do comprovante/checkout no gateway

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

        /// <summary>
        /// Registra os dados da cobrança PIX retornados pelo gateway (payment id, QR e ticket).
        /// Mantém o pagamento em <see cref="PagamentoFaturaStatus.Pending"/> até a confirmação via webhook.
        /// </summary>
        public void RegistrarCobrancaPix(string? identificadorPagamento, string? qrCode, string? qrCodeBase64, string? ticketUrl, string alteradoPor)
        {
            if (!string.IsNullOrWhiteSpace(identificadorPagamento))
                IdentificadorPagamento = identificadorPagamento;
            QrCode = qrCode;
            QrCodeBase64 = qrCodeBase64;
            TicketUrl = ticketUrl;
            Status = PagamentoFaturaStatus.Pending;
            MarcarAlterado(alteradoPor);
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
