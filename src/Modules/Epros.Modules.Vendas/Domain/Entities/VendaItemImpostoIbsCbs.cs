using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaItemImpostoIbsCbs (Reforma Tributária - IBS/CBS por item).
    /// FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaItemImpostoIbsCbs : EntidadeSaaSBase
    {
        public Guid VendaItemId { get; private set; }
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

        public VendaItemImpostoIbsCbsTributacaoRegular? VendaItemImpostoIbsCbsTributacaoRegular { get; private set; }

        // Navegação intra-módulo
        public VendaItem VendaItem { get; private set; } = null!;

        protected VendaItemImpostoIbsCbs() { } // EF Core

        public VendaItemImpostoIbsCbs(Guid vendaItemId, string cst, string cClassTrib, decimal aliquotaEstadual, decimal aliquotaMunicipal, decimal aliquotaCbs, decimal aliquotaEstadualReducao, decimal aliquotaMunicipalReducao, decimal aliquotaCbsReducao, decimal aliquotaEstadualDiferimento, decimal aliquotaMunicipalDiferimento, decimal aliquotaCbsDiferimento, decimal aliquotaEfetivaEstadual, decimal aliquotaEfetivaMunicipal, decimal aliquotaEfetivaCbs, decimal valorBaseDeCalculo, decimal valorImpostoDevidoEstadual, decimal valorImpostoDevidoMunicipal, decimal valorImpostoDevidoCbs, VendaItemImpostoIbsCbsTributacaoRegular? vendaItemImpostoIbsCbsTributacaoRegular,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaItemId = vendaItemId;
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
            VendaItemImpostoIbsCbsTributacaoRegular = vendaItemImpostoIbsCbsTributacaoRegular;
        }

        public void DefinirTributacaoRegular(VendaItemImpostoIbsCbsTributacaoRegular tributacaoRegular)
            => VendaItemImpostoIbsCbsTributacaoRegular = tributacaoRegular;

        /// <summary>Porte fiel de VendaItemImpostoIbsCbs.Duplicar (novo Id/FK, incl. tributação regular).</summary>
        public VendaItemImpostoIbsCbs Duplicar(Guid novoItemId, string criadoPor)
        {
            var clone = new VendaItemImpostoIbsCbs(novoItemId, Cst, CClassTrib, AliquotaEstadual, AliquotaMunicipal, AliquotaCbs,
                AliquotaEstadualReducao, AliquotaMunicipalReducao, AliquotaCbsReducao, AliquotaEstadualDiferimento,
                AliquotaMunicipalDiferimento, AliquotaCbsDiferimento, AliquotaEfetivaEstadual, AliquotaEfetivaMunicipal,
                AliquotaEfetivaCbs, ValorBaseDeCalculo, ValorImpostoDevidoEstadual, ValorImpostoDevidoMunicipal,
                ValorImpostoDevidoCbs, null, TenantId, criadoPor);
            if (VendaItemImpostoIbsCbsTributacaoRegular is not null)
                clone.DefinirTributacaoRegular(VendaItemImpostoIbsCbsTributacaoRegular.Duplicar(clone.Id, criadoPor));
            return clone;
        }

        public void Alterar(decimal aliquotaEstadual, decimal aliquotaMunicipal, decimal aliquotaCbs, decimal aliquotaEstadualReducao, decimal aliquotaMunicipalReducao, decimal aliquotaCbsReducao, decimal aliquotaEstadualDiferimento, decimal aliquotaMunicipalDiferimento, decimal aliquotaCbsDiferimento, decimal aliquotaEfetivaEstadual, decimal aliquotaEfetivaMunicipal, decimal aliquotaEfetivaCbs, decimal valorBaseDeCalculo, decimal valorImpostoDevidoEstadual, decimal valorImpostoDevidoMunicipal, decimal valorImpostoDevidoCbs, string alteradoPor)
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
            MarcarAlterado(alteradoPor);
        }
    }
}
