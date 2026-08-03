using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>
    /// 1.08A — Recibo de pagamento de uma fatura da assinatura SaaS. Documento SIMPLES (decisão do Rafael:
    /// recibo agora, NFS-e depois): nº, pagador, valor, data, meio e fatura de referência. É gerado na
    /// quitação da fatura e fica disponível para o cliente baixar.
    ///
    /// ⛔ NÃO é NFS-e: a emissão fiscal (ISS/NFS-e da mensalidade) está DIFERIDA e depende da skill
    /// <c>fiscal-nfse</c> + overlay <c>negocio-siser</c> + validação do contador (pedido de ingestão já
    /// aberto). Este recibo NÃO substitui a nota fiscal e não carrega apuração de tributo.
    /// </summary>
    public class ReciboPagamento : EntidadeSaaSBase
    {
        /// <summary>Número legível do recibo (ex.: REC-20260731-AB12CD34).</summary>
        public string Numero { get; private set; } = string.Empty;

        public Guid FaturaId { get; private set; }
        public Guid ClienteId { get; private set; }
        public Guid? PagamentoFaturaId { get; private set; }

        public decimal Valor { get; private set; }
        public DateTime DataPagamento { get; private set; }

        /// <summary>Meio de pagamento (PIX, Transferencia, Manual, Cartao...).</summary>
        public string MeioPagamento { get; private set; } = string.Empty;

        public string? PagadorNome { get; private set; }
        public string? PagadorDocumento { get; private set; }

        protected ReciboPagamento() { } // EF Core

        public ReciboPagamento(
            string numero,
            Guid faturaId,
            Guid clienteId,
            Guid? pagamentoFaturaId,
            decimal valor,
            DateTime dataPagamento,
            string meioPagamento,
            string? pagadorNome,
            string? pagadorDocumento,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<ReciboPagamento>()
                .Requires()
                .IsNotNullOrEmpty(numero, nameof(Numero), "Número do recibo é obrigatório")
                .AreNotEquals(faturaId, Guid.Empty, nameof(FaturaId), "FaturaId é obrigatório")
                .AreNotEquals(clienteId, Guid.Empty, nameof(ClienteId), "ClienteId é obrigatório")
                .IsGreaterThan(valor, 0, nameof(Valor), "Valor do recibo deve ser maior que zero")
                .IsNotNullOrEmpty(meioPagamento, nameof(MeioPagamento), "Meio de pagamento é obrigatório")
            );

            Numero = numero;
            FaturaId = faturaId;
            ClienteId = clienteId;
            PagamentoFaturaId = pagamentoFaturaId;
            Valor = valor;
            DataPagamento = dataPagamento;
            MeioPagamento = meioPagamento;
            PagadorNome = pagadorNome;
            PagadorDocumento = pagadorDocumento;
        }

        /// <summary>
        /// Fábrica: cria um recibo para a fatura quitada gerando um número legível único.
        /// </summary>
        public static ReciboPagamento Emitir(
            Fatura fatura,
            Guid? pagamentoFaturaId,
            decimal valorPago,
            string meioPagamento,
            string? pagadorNome,
            string? pagadorDocumento,
            string criadoPor)
        {
            var numero = $"REC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant()}";
            return new ReciboPagamento(
                numero: numero,
                faturaId: fatura.Id,
                clienteId: fatura.ClienteId,
                pagamentoFaturaId: pagamentoFaturaId,
                valor: valorPago,
                dataPagamento: fatura.DataPagamento ?? DateTime.UtcNow,
                meioPagamento: meioPagamento,
                pagadorNome: pagadorNome,
                pagadorDocumento: pagadorDocumento,
                tenantId: fatura.TenantId,
                criadoPor: criadoPor);
        }
    }
}
