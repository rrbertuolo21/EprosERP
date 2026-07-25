using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Dados funcionais de transporte vinculados ao documento de entrada
    /// (EF Logística de Entrada §15.6 `lde_documento_entrada_transporte`).
    /// Detalhamento de frete fica no submódulo Transporte/Frete; ModalidadeFrete preservado como código.
    /// </summary>
    public class LdeDocumentoEntradaTransporte : EntidadeSaaSBase
    {
        public Guid DocumentoEntradaId { get; private set; }
        public Guid? TransportadorId { get; private set; }
        public int? ModalidadeFrete { get; private set; }
        public string? ReferenciaTransporte { get; private set; }

        protected LdeDocumentoEntradaTransporte() { } // EF Core

        public LdeDocumentoEntradaTransporte(Guid documentoEntradaId, Guid? transportadorId, int? modalidadeFrete, string? referenciaTransporte, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DocumentoEntradaId = documentoEntradaId;
            TransportadorId = transportadorId;
            ModalidadeFrete = modalidadeFrete;
            ReferenciaTransporte = referenciaTransporte;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LdeDocumentoEntradaTransporte>()
                .Requires()
                .AreNotEquals(DocumentoEntradaId, Guid.Empty, nameof(DocumentoEntradaId), "O documento do transporte de entrada é obrigatório [Origem: LdeDocumentoEntradaTransporte]"));
        }
    }
}
