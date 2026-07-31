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

        // 1.01 — ciclo PIX / liquidação (EF 11.9).
        public DateTime? DataExpiracao { get; private set; }      // expiração da cobrança PIX
        public decimal? ValorRecebido { get; private set; }        // líquido pós-tarifa
        public DateTime? DataLiberacaoFundos { get; private set; } // liberação dos fundos pelo gateway

        // Dados da cobrança PIX gerada no gateway (melhoria: o legado não persistia).
        public string? QrCode { get; private set; }         // PIX "copia e cola" (payload EMV)
        public string? QrCodeBase64 { get; private set; }   // imagem do QR em base64
        public string? TicketUrl { get; private set; }      // URL do comprovante/checkout no gateway

        // 1.08B — Dados da cobrança BOLETO gerada no gateway (concilia pelo MESMO webhook unificado).
        public string? LinhaDigitavel { get; private set; }     // linha digitável do boleto
        public string? CodigoBarras { get; private set; }       // código de barras do boleto
        public string? UrlBoleto { get; private set; }          // URL do PDF/visualização do boleto
        public DateTime? DataVencimentoBoleto { get; private set; }

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
        public void RegistrarCobrancaPix(string? identificadorPagamento, string? qrCode, string? qrCodeBase64, string? ticketUrl, string alteradoPor, DateTime? dataExpiracao = null)
        {
            if (!string.IsNullOrWhiteSpace(identificadorPagamento))
                IdentificadorPagamento = identificadorPagamento;
            QrCode = qrCode;
            QrCodeBase64 = qrCodeBase64;
            TicketUrl = ticketUrl;
            if (dataExpiracao.HasValue)
                DataExpiracao = dataExpiracao;
            Status = PagamentoFaturaStatus.Pending;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>
        /// 1.08B — Registra os dados de um BOLETO gerado no gateway (linha digitável, código de barras,
        /// vencimento e URL do PDF). Mantém o pagamento em <see cref="PagamentoFaturaStatus.Pending"/> até a
        /// confirmação via webhook unificado (mesmo caminho de conciliação do PIX real).
        /// </summary>
        public void RegistrarCobrancaBoleto(string? identificadorPagamento, string? linhaDigitavel, string? codigoBarras, string? urlBoleto, DateTime? dataVencimento, string alteradoPor)
        {
            if (!string.IsNullOrWhiteSpace(identificadorPagamento))
                IdentificadorPagamento = identificadorPagamento;
            LinhaDigitavel = linhaDigitavel;
            CodigoBarras = codigoBarras;
            UrlBoleto = urlBoleto;
            DataVencimentoBoleto = dataVencimento;
            Status = PagamentoFaturaStatus.Pending;
            MarcarAlterado(alteradoPor);
        }

        public void Liquidar(decimal valorRealPago, decimal? tarifa, string alteradoPor, decimal? valorRecebido = null, DateTime? dataLiberacaoFundos = null)
        {
            Status = PagamentoFaturaStatus.Paid;
            ValorPago = valorRealPago;
            ValorTarifa = tarifa;
            // líquido: usa o informado; senão deriva de valorPago - tarifa.
            ValorRecebido = valorRecebido ?? (valorRealPago - (tarifa ?? 0m));
            DataLiberacaoFundos = dataLiberacaoFundos;
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
