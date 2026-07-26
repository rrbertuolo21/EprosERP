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
        public string? XCorrecao { get; private set; } // texto da Carta de Correção (xCorrecao) — evento 110110
        /// <summary>
        /// Justificativa (xJust) do evento de cancelamento (110111). Campo próprio do registro de
        /// cancelamento (REG-CANC-014 / EF §11) — antes reusava-se XCorrecao (smell semântico corrigido).
        /// </summary>
        public string? Justificativa { get; private set; }
        public string? Xml { get; private set; }
        /// <summary>Caminho (≤500) do PDF do comprovante do evento de cancelamento — REG-CANC-018.</summary>
        public string? PdfCaminho { get; private set; }
        /// <summary>Caminho (≤500) do XML do evento de cancelamento — REG-CANC-019.</summary>
        public string? XmlCaminho { get; private set; }

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
            string criadoPor,
            string? justificativa = null,
            string? pdfCaminho = null,
            string? xmlCaminho = null)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<EventoDocumentoFiscal>()
                .Requires()
                .IsTrue(tipoEvento == "Cancelamento" || tipoEvento == "CartaCorrecao", nameof(TipoEvento), "Tipo de evento inválido.")
                .IsGreaterThan(statusSefaz, 0, nameof(StatusSefaz), "Status SEFAZ inválido.")
                .IsTrue(pdfCaminho == null || pdfCaminho.Length <= 500, nameof(PdfCaminho), "Caminho do PDF deve ter no máximo 500 caracteres.") // REG-CANC-018
                .IsTrue(xmlCaminho == null || xmlCaminho.Length <= 500, nameof(XmlCaminho), "Caminho do XML deve ter no máximo 500 caracteres.") // REG-CANC-019
            );

            DocumentoFiscalId = documentoFiscalId;
            TipoEvento = tipoEvento;
            StatusSefaz = statusSefaz;
            XMotivo = xMotivo;
            Protocolo = protocolo;
            SequenciaEvento = sequenciaEvento;
            XCorrecao = xCorrecao;
            Justificativa = justificativa;
            Xml = xml;
            PdfCaminho = pdfCaminho;
            XmlCaminho = xmlCaminho;
            DHRecebimento = DateTime.UtcNow;
        }

        /// <summary>
        /// Grava os caminhos (≤500) do XML/PDF do comprovante do evento após persistência no
        /// armazenamento de arquivos — REG-CANC-018/019. Não altera o estado fiscal do evento.
        /// </summary>
        public void DefinirCaminhosArquivos(string? xmlCaminho, string? pdfCaminho, string alteradoPor)
        {
            if (xmlCaminho != null) XmlCaminho = xmlCaminho;
            if (pdfCaminho != null) PdfCaminho = pdfCaminho;
            MarcarAlterado(alteradoPor);
        }
    }
}
