using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Imposto IBS/CBS (reforma tributária) de um item de compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraItemImpostoIbsCbs.
    /// </summary>
    public class CompraItemImpostoIbsCbs : EntidadeSaaSBase
    {
        public Guid CompraItemId { get; private set; }
        public string Cst { get; private set; } = string.Empty;
        public string CClassTrib { get; private set; } = string.Empty;

        public decimal AliquotaEstadual { get; private set; }
        public decimal AliquotaMunicipal { get; private set; }
        public decimal AliquotaCbs { get; private set; }

        public decimal AliquotaEstadualReducao { get; private set; }
        public decimal AliquotaMunicipalReducao { get; private set; }
        public decimal AliquotaCbsReducao { get; private set; }

        public decimal AliquotaEstadualDiferimento { get; private set; }
        public decimal AliquotaMunicipalDiferimento { get; private set; }
        public decimal AliquotaCbsDiferimento { get; private set; }

        public decimal AliquotaEfetivaEstadual { get; private set; }
        public decimal AliquotaEfetivaMunicipal { get; private set; }
        public decimal AliquotaEfetivaCbs { get; private set; }

        public decimal ValorBaseDeCalculo { get; private set; }
        public decimal ValorImpostoDevidoEstadual { get; private set; }
        public decimal ValorImpostoDevidoMunicipal { get; private set; }
        public decimal ValorImpostoDevidoCbs { get; private set; }

        // Navegação intra-módulo
        public CompraItem? CompraItem { get; private set; }

        protected CompraItemImpostoIbsCbs() { } // EF Core

        public CompraItemImpostoIbsCbs(Guid compraItemId, string cst, string cClassTrib, decimal aliquotaEstadual, decimal aliquotaMunicipal, decimal aliquotaCbs, decimal aliquotaEstadualReducao, decimal aliquotaMunicipalReducao, decimal aliquotaCbsReducao, decimal aliquotaEstadualDiferimento, decimal aliquotaMunicipalDiferimento, decimal aliquotaCbsDiferimento, decimal aliquotaEfetivaEstadual, decimal aliquotaEfetivaMunicipal, decimal aliquotaEfetivaCbs, decimal valorBaseDeCalculo, decimal valorImpostoDevidoEstadual, decimal valorImpostoDevidoMunicipal, decimal valorImpostoDevidoCbs, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraItemId = compraItemId;
            Cst = cst;
            CClassTrib = cClassTrib;
            AliquotaEstadual = aliquotaEstadual;
            AliquotaMunicipal = aliquotaMunicipal;
            AliquotaCbs = aliquotaCbs;
            AliquotaEstadualReducao = aliquotaEstadualReducao;
            AliquotaMunicipalReducao = aliquotaMunicipalReducao;
            AliquotaCbsReducao = aliquotaCbsReducao;
            AliquotaEstadualDiferimento = aliquotaEstadualDiferimento;
            AliquotaMunicipalDiferimento = aliquotaMunicipalDiferimento;
            AliquotaCbsDiferimento = aliquotaCbsDiferimento;
            AliquotaEfetivaEstadual = aliquotaEfetivaEstadual;
            AliquotaEfetivaMunicipal = aliquotaEfetivaMunicipal;
            AliquotaEfetivaCbs = aliquotaEfetivaCbs;
            ValorBaseDeCalculo = valorBaseDeCalculo;
            ValorImpostoDevidoEstadual = valorImpostoDevidoEstadual;
            ValorImpostoDevidoMunicipal = valorImpostoDevidoMunicipal;
            ValorImpostoDevidoCbs = valorImpostoDevidoCbs;
        }

        public void Alterar(decimal aliquotaEstadual, decimal aliquotaMunicipal, decimal aliquotaCbs, decimal aliquotaEstadualReducao, decimal aliquotaMunicipalReducao, decimal aliquotaCbsReducao, decimal aliquotaEstadualDiferimento, decimal aliquotaMunicipalDiferimento, decimal aliquotaCbsDiferimento, decimal aliquotaEfetivaEstadual, decimal aliquotaEfetivaMunicipal, decimal aliquotaEfetivaCbs, decimal valorBaseDeCalculo, decimal valorImpostoDevidoEstadual, decimal valorImpostoDevidoMunicipal, decimal valorImpostoDevidoCbs, string usuario)
        {
            AliquotaEstadual = aliquotaEstadual;
            AliquotaMunicipal = aliquotaMunicipal;
            AliquotaCbs = aliquotaCbs;
            AliquotaEstadualReducao = aliquotaEstadualReducao;
            AliquotaMunicipalReducao = aliquotaMunicipalReducao;
            AliquotaCbsReducao = aliquotaCbsReducao;
            AliquotaEstadualDiferimento = aliquotaEstadualDiferimento;
            AliquotaMunicipalDiferimento = aliquotaMunicipalDiferimento;
            AliquotaCbsDiferimento = aliquotaCbsDiferimento;
            AliquotaEfetivaEstadual = aliquotaEfetivaEstadual;
            AliquotaEfetivaMunicipal = aliquotaEfetivaMunicipal;
            AliquotaEfetivaCbs = aliquotaEfetivaCbs;
            ValorBaseDeCalculo = valorBaseDeCalculo;
            ValorImpostoDevidoEstadual = valorImpostoDevidoEstadual;
            ValorImpostoDevidoMunicipal = valorImpostoDevidoMunicipal;
            ValorImpostoDevidoCbs = valorImpostoDevidoCbs;
            MarcarAlterado(usuario);
        }
    }
}
