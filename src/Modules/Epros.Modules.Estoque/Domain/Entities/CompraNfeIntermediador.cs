using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Intermediador da CompraNfe (marketplace). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraNfeIntermediador. O ValueObject Documento foi achatado para string.
    /// </summary>
    public class CompraNfeIntermediador : EntidadeSaaSBase
    {
        public Guid CompraNfeId { get; private set; }
        public string Documento { get; private set; } = string.Empty;
        public string? IdentificadorIntermediador { get; private set; }

        // Navegação intra-módulo
        public CompraNfe? CompraNfe { get; private set; }

        protected CompraNfeIntermediador() { } // EF Core

        public CompraNfeIntermediador(Guid compraNfeId, string documento, string? identificadorIntermediador, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraNfeId = compraNfeId;
            Documento = documento ?? string.Empty;
            IdentificadorIntermediador = identificadorIntermediador;
        }
    }
}
