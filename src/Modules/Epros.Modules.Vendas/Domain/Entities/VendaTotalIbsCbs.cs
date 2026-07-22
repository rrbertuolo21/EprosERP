using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaTotalIbsCbs (Reforma Tributária - totais IBS/CBS da venda).
    /// FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaTotalIbsCbs : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public decimal ValorBaseDeCalculo { get; private set; }
        public decimal ValorImpostoDevidoEstadual { get; private set; }
        public decimal ValorImpostoDevidoMunicipal { get; private set; }
        public decimal ValorImpostoDevidoCbs { get; private set; }

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaTotalIbsCbs() { } // EF Core

        public VendaTotalIbsCbs(Guid vendaId, decimal valorBaseDeCalculo, decimal valorImpostoDevidoEstadual, decimal valorImpostoDevidoMunicipal, decimal valorImpostoDevidoCbs,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            ValorBaseDeCalculo = valorBaseDeCalculo;
            ValorImpostoDevidoEstadual = valorImpostoDevidoEstadual;
            ValorImpostoDevidoMunicipal = valorImpostoDevidoMunicipal;
            ValorImpostoDevidoCbs = valorImpostoDevidoCbs;
        }

        public void Alterar(decimal valorBaseDeCalculo, decimal valorImpostoDevidoEstadual, decimal valorImpostoDevidoMunicipal, decimal valorImpostoDevidoCbs, string alteradoPor)
        {
            ValorBaseDeCalculo = valorBaseDeCalculo;
            ValorImpostoDevidoEstadual = valorImpostoDevidoEstadual;
            ValorImpostoDevidoMunicipal = valorImpostoDevidoMunicipal;
            ValorImpostoDevidoCbs = valorImpostoDevidoCbs;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaTotalIbsCbs.Duplicar (novo Id/FK).</summary>
        public VendaTotalIbsCbs Duplicar(Guid novaVendaId, string criadoPor)
            => new(novaVendaId, ValorBaseDeCalculo, ValorImpostoDevidoEstadual, ValorImpostoDevidoMunicipal, ValorImpostoDevidoCbs, TenantId, criadoPor);
    }
}
