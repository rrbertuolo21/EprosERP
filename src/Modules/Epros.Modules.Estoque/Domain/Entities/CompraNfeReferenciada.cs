using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Chave de NF-e referenciada pela compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraNfeReferenciada.
    /// </summary>
    public class CompraNfeReferenciada : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public string Chave { get; private set; } = string.Empty;

        // Navegação intra-módulo
        public Compra? Compra { get; private set; }

        protected CompraNfeReferenciada() { } // EF Core

        public CompraNfeReferenciada(Guid compraId, string chave, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            Chave = chave ?? string.Empty;
        }

        public void Alterar(string chave, string usuario)
        {
            Chave = chave ?? string.Empty;
            MarcarAlterado(usuario);
        }
    }
}
