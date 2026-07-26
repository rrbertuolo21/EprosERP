using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    /// <summary>
    /// Boleto emitido a partir de fatura ou título a receber (EF FIN-SF §7.4 / §11 sf_boleto).
    /// Não armazena dados sensíveis de cartão (RSF-045). Guarda nosso número, linha digitável e arquivo.
    /// </summary>
    public class Boleto : EntidadeSaaSBase
    {
        public Guid FaturaCobrancaId { get; private set; }
        public Guid ContaEmissoraId { get; private set; }
        public long NossoNumero { get; private set; }
        public string? NumeroDocumento { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime DataVencimento { get; private set; }
        public string? LinhaDigitavel { get; private set; }
        public string? Arquivo { get; private set; }
        public decimal Multa { get; private set; }
        public decimal Juros { get; private set; }
        public string? Instrucao1 { get; private set; }
        public string? Instrucao2 { get; private set; }
        public string? Instrucao3 { get; private set; }
        public string? Instrucao4 { get; private set; }

        public FaturaCobranca FaturaCobranca { get; private set; } = null!;
        public ContaEmissora ContaEmissora { get; private set; } = null!;

        protected Boleto() { } // EF Core

        public Boleto(
            Guid faturaCobrancaId, Guid contaEmissoraId, long nossoNumero, string? numeroDocumento, decimal valor,
            DateTime dataVencimento, string? linhaDigitavel, string? arquivo, decimal multa, decimal juros,
            string? instrucao1, string? instrucao2, string? instrucao3, string? instrucao4,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            FaturaCobrancaId = faturaCobrancaId; ContaEmissoraId = contaEmissoraId; NossoNumero = nossoNumero;
            NumeroDocumento = numeroDocumento; Valor = valor; DataVencimento = dataVencimento;
            LinhaDigitavel = linhaDigitavel; Arquivo = arquivo; Multa = multa; Juros = juros;
            Instrucao1 = instrucao1; Instrucao2 = instrucao2; Instrucao3 = instrucao3; Instrucao4 = instrucao4;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Boleto>()
                .Requires()
                .AreNotEquals(FaturaCobrancaId, Guid.Empty, nameof(FaturaCobrancaId), "A fatura do boleto é obrigatória.")
                .AreNotEquals(ContaEmissoraId, Guid.Empty, nameof(ContaEmissoraId), "A conta emissora do boleto é obrigatória.")
            );
        }
    }
}
