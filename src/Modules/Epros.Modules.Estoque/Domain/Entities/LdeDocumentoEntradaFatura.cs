using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Fatura vinculada ao documento de entrada (EF Logística de Entrada §15.7 `lde_documento_entrada_fatura`).
    /// </summary>
    public class LdeDocumentoEntradaFatura : EntidadeSaaSBase
    {
        public Guid DocumentoEntradaId { get; private set; }
        public string? Numero { get; private set; }
        public decimal? ValorOriginal { get; private set; }
        public decimal? ValorDesconto { get; private set; }
        public decimal? ValorLiquido { get; private set; }

        protected LdeDocumentoEntradaFatura() { } // EF Core

        public LdeDocumentoEntradaFatura(Guid documentoEntradaId, string? numero, decimal? valorOriginal, decimal? valorDesconto, decimal? valorLiquido, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DocumentoEntradaId = documentoEntradaId;
            Numero = numero;
            ValorOriginal = valorOriginal;
            ValorDesconto = valorDesconto;
            ValorLiquido = valorLiquido;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LdeDocumentoEntradaFatura>()
                .Requires()
                .AreNotEquals(DocumentoEntradaId, Guid.Empty, nameof(DocumentoEntradaId), "O documento da fatura de entrada é obrigatório [Origem: LdeDocumentoEntradaFatura]"));
        }
    }
}
