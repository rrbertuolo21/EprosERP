using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Linha de detalhe do documento de entrada (EF Logística de Entrada §15.4 `lde_documento_entrada_item`).
    /// Dados tributários por item são fronteira fiscal (armazenados como estrutura opcional).
    /// </summary>
    public class LdeDocumentoEntradaItem : EntidadeSaaSBase
    {
        public Guid DocumentoEntradaId { get; private set; }
        public Guid? ProdutoId { get; private set; }
        public decimal? QuantidadeDocumento { get; private set; }
        public decimal? ValorItem { get; private set; }
        public string? DadosTributariosItem { get; private set; }

        // Navegação intra-módulo
        public LdeDocumentoEntrada? Documento { get; private set; }

        protected LdeDocumentoEntradaItem() { } // EF Core

        public LdeDocumentoEntradaItem(Guid documentoEntradaId, Guid? produtoId, decimal? quantidadeDocumento, decimal? valorItem, string? dadosTributariosItem, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            DocumentoEntradaId = documentoEntradaId;
            ProdutoId = produtoId;
            QuantidadeDocumento = quantidadeDocumento;
            ValorItem = valorItem;
            DadosTributariosItem = dadosTributariosItem;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<LdeDocumentoEntradaItem>()
                .Requires()
                .AreNotEquals(DocumentoEntradaId, Guid.Empty, nameof(DocumentoEntradaId), "O documento do item de entrada é obrigatório [Origem: LdeDocumentoEntradaItem]"));
        }
    }
}
