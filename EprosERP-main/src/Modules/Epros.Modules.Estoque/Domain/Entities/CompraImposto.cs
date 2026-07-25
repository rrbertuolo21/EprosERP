using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Imposto no nível da compra (alíquota de crédito de ICMS). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraImposto.
    /// </summary>
    public class CompraImposto : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public decimal ValorAliquotaCreditoIcms { get; private set; }

        // Navegação intra-módulo
        public Compra? Compra { get; private set; }

        protected CompraImposto() { } // EF Core

        public CompraImposto(Guid compraId, decimal valorAliquotaCreditoIcms, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            ValorAliquotaCreditoIcms = valorAliquotaCreditoIcms;
        }

        public void Alterar(decimal valorAliquotaCreditoIcms, string usuario)
        {
            ValorAliquotaCreditoIcms = valorAliquotaCreditoIcms;
            MarcarAlterado(usuario);
        }
    }
}
