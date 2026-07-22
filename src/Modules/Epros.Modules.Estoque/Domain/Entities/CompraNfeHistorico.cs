using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Histórico da NF-e da compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraNfeHistorico (estrutura mínima do legado: apenas CompraId).
    /// </summary>
    public class CompraNfeHistorico : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }

        // Navegação intra-módulo
        public Compra? Compra { get; private set; }

        protected CompraNfeHistorico() { } // EF Core

        public CompraNfeHistorico(Guid compraId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
        }
    }
}
