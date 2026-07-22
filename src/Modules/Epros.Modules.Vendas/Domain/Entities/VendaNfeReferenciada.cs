using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaNfeReferenciada (chave de NF-e referenciada).
    /// FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaNfeReferenciada : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public string Chave { get; private set; } = string.Empty;

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaNfeReferenciada() { } // EF Core

        public VendaNfeReferenciada(Guid vendaId, string chave, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            Chave = chave;
        }

        public void Alterar(string chave, string alteradoPor)
        {
            Chave = chave;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaNfeReferenciada.Duplicar (novo Id/FK).</summary>
        public VendaNfeReferenciada Duplicar(Guid novaVendaId, string criadoPor)
            => new(novaVendaId, Chave, TenantId, criadoPor);
    }
}
