using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Fiscal.Domain.Entities
{
    public class EventoDocumentoFiscal : EntidadeSaaSBase
    {
        public Guid DocumentoFiscalId { get; private set; }
        public string TipoEvento { get; private set; } = string.Empty; // "Cancelamento" ou "CartaCorrecao"
        public int StatusSefaz { get; private set; }
        public string? XMotivo { get; private set; }
        public DateTime? DHRecebimento { get; private set; }
        public string? Protocolo { get; private set; }
        public int SequenciaEvento { get; private set; }
        public string? XCorrecao { get; private set; }
        public string? Xml { get; private set; }

        protected EventoDocumentoFiscal() { } // EF Core

        public EventoDocumentoFiscal(
            Guid documentoFiscalId,
            string tipoEvento,
            int statusSefaz,
            string? xMotivo,
            string? protocolo,
            int sequenciaEvento,
            string? xCorrecao,
            string? xml,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EventoDocumentoFiscal>()
                .Requires()
                .IsTrue(tipoEvento == "Cancelamento" || tipoEvento == "CartaCorrecao", nameof(TipoEvento), "Tipo de evento inválido.")
                .IsGreaterThan(statusSefaz, 0, nameof(StatusSefaz), "Status SEFAZ inválido.")
            );

            DocumentoFiscalId = documentoFiscalId;
            TipoEvento = tipoEvento;
            StatusSefaz = statusSefaz;
            XMotivo = xMotivo;
            Protocolo = protocolo;
            SequenciaEvento = sequenciaEvento;
            XCorrecao = xCorrecao;
            Xml = xml;
            DHRecebimento = DateTime.UtcNow;
        }
    }
}
