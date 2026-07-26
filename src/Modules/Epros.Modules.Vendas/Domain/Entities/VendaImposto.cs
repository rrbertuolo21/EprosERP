using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaImposto (crédito de ICMS da venda). FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaImposto : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public decimal ValorAliquotaCreditoIcms { get; private set; }

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaImposto() { } // EF Core

        public VendaImposto(Guid vendaId, decimal valorAliquotaCreditoIcms, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            ValorAliquotaCreditoIcms = valorAliquotaCreditoIcms;
        }

        public void Alterar(decimal valorAliquotaCreditoIcms, string alteradoPor)
        {
            ValorAliquotaCreditoIcms = valorAliquotaCreditoIcms;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaImposto.Duplicar (novo Id/FK).</summary>
        public VendaImposto Duplicar(Guid novaVendaId, string criadoPor)
            => new(novaVendaId, ValorAliquotaCreditoIcms, TenantId, criadoPor);
    }
}
