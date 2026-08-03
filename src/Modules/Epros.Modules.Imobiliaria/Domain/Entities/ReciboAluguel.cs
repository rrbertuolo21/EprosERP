using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Imobiliaria.Domain.Entities
{
    /// <summary>
    /// Recibo do aluguel (ID8/NF-05). Numero pela numeracao central (T9); documento/assinatura
    /// governados pelo GED (T10). Emitido a partir de uma cobranca baixada. A exigencia fiscal do
    /// recibo de aluguel e valida-contador (NF-05) — aqui produzimos o comprovante operacional.
    /// </summary>
    public class ReciboAluguel : EntidadeSaaSBase
    {
        public Guid CobrancaId { get; private set; }
        public Guid LocacaoId { get; private set; }
        /// <summary>Numero sequencial atomico do servico central de numeracao (T9).</summary>
        public long Numero { get; private set; }
        public decimal Valor { get; private set; }
        public DateTime DataEmissao { get; private set; }
        /// <summary>Referencia do documento no GED (T10) — assinatura ICP quando aplicavel.</summary>
        public string? DocumentoRef { get; private set; }

        protected ReciboAluguel() { } // EF Core

        public ReciboAluguel(
            Guid cobrancaId,
            Guid locacaoId,
            long numero,
            decimal valor,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            CobrancaId = cobrancaId;
            LocacaoId = locacaoId;
            Numero = numero;
            Valor = valor;
            DataEmissao = DateTime.UtcNow;
            Validar();
        }

        public void VincularDocumento(string documentoRef, string usuario)
        {
            DocumentoRef = documentoRef;
            MarcarAlterado(usuario);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<ReciboAluguel>()
                .Requires()
                .AreNotEquals(CobrancaId, Guid.Empty, nameof(CobrancaId),
                    "O recibo exige cobranca de origem. [Origem: ReciboAluguel]")
                .IsGreaterThan(Numero, 0, nameof(Numero),
                    "O numero do recibo deve ser positivo. [Origem: ReciboAluguel] (NF-05)")
                .IsGreaterThan(Valor, 0, nameof(Valor),
                    "O valor do recibo deve ser positivo. [Origem: ReciboAluguel]"));
        }
    }
}
