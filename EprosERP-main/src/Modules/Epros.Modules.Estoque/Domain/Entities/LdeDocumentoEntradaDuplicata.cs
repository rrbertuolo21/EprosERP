using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Duplicata vinculada ao documento de entrada (EF Logística de Entrada §15.5 `lde_documento_entrada_duplicata`).
    /// TituloPagarId é gerado por Contas a Pagar quando aplicável (referência externa por Guid).
    /// </summary>
    public class LdeDocumentoEntradaDuplicata : EntidadeSaaSBase
    {
        public Guid DocumentoEntradaId { get; private set; }
        public string? Numero { get; private set; }
        public DateTime? DataVencimento { get; private set; }
        public decimal? Valor { get; private set; }
        public Guid? TituloPagarId { get; private set; }

        // Navegação intra-módulo
        public LdeDocumentoEntrada? Documento { get; private set; }

        protected LdeDocumentoEntradaDuplicata() { } // EF Core

        public LdeDocumentoEntradaDuplicata(Guid documentoEntradaId, string? numero, DateTime? dataVencimento, decimal? valor, Guid? tituloPagarId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DocumentoEntradaId = documentoEntradaId;
            Numero = numero;
            DataVencimento = dataVencimento;
            Valor = valor;
            TituloPagarId = tituloPagarId;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LdeDocumentoEntradaDuplicata>()
                .Requires()
                .AreNotEquals(DocumentoEntradaId, Guid.Empty, nameof(DocumentoEntradaId), "O documento da duplicata de entrada é obrigatório [Origem: LdeDocumentoEntradaDuplicata]"));
        }
    }
}
