using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Totais de IBS/CBS (reforma tributária) de uma compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraTotalIbsCbs.
    /// </summary>
    public class CompraTotalIbsCbs : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public decimal ValorBaseDeCalculo { get; private set; }
        public decimal ValorImpostoDevidoEstadual { get; private set; }
        public decimal ValorImpostoDevidoMunicipal { get; private set; }
        public decimal ValorImpostoDevidoCbs { get; private set; }

        // Navegação intra-módulo
        public Compra? Compra { get; private set; }

        protected CompraTotalIbsCbs() { } // EF Core

        public CompraTotalIbsCbs(Guid compraId, decimal valorBaseDeCalculo, decimal valorImpostoDevidoEstadual, decimal valorImpostoDevidoMunicipal, decimal valorImpostoDevidoCbs, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            ValorBaseDeCalculo = valorBaseDeCalculo;
            ValorImpostoDevidoEstadual = valorImpostoDevidoEstadual;
            ValorImpostoDevidoMunicipal = valorImpostoDevidoMunicipal;
            ValorImpostoDevidoCbs = valorImpostoDevidoCbs;
        }

        public void Alterar(decimal valorBaseDeCalculo, decimal valorImpostoDevidoEstadual, decimal valorImpostoDevidoMunicipal, decimal valorImpostoDevidoCbs, string usuario)
        {
            ValorBaseDeCalculo = valorBaseDeCalculo;
            ValorImpostoDevidoEstadual = valorImpostoDevidoEstadual;
            ValorImpostoDevidoMunicipal = valorImpostoDevidoMunicipal;
            ValorImpostoDevidoCbs = valorImpostoDevidoCbs;
            MarcarAlterado(usuario);
        }
    }
}
