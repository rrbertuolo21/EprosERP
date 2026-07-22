using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaItemImpostoIbsCbsTributacaoRegular (Reforma Tributária -
    /// tributação regular do IBS/CBS). FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaItemImpostoIbsCbsTributacaoRegular : EntidadeSaaSBase
    {
        public Guid VendaItemImpostoIbsCbsId { get; private set; }
        public string Cst { get; private set; } = string.Empty;
        public string CClassTrib { get; private set; } = string.Empty;
        public decimal AliquotaEfetivaIbsEstadual { get; private set; }
        public decimal ValorIbsEstadual { get; private set; }
        public decimal AliquotaEfetivaIbsMunicipal { get; private set; }
        public decimal ValorIbsMunicipal { get; private set; }
        public decimal AliquotaEfetivaCbs { get; private set; }
        public decimal ValorCbs { get; private set; }

        // Navegação intra-módulo
        public VendaItemImpostoIbsCbs VendaItemImpostoIbsCbs { get; private set; } = null!;

        protected VendaItemImpostoIbsCbsTributacaoRegular() { } // EF Core

        public VendaItemImpostoIbsCbsTributacaoRegular(Guid vendaItemImpostoIbsCbsId, string cst, string cClassTrib, decimal aliquotaEfetivaIbsEstadual, decimal valorIbsEstadual, decimal aliquotaEfetivaIbsMunicipal, decimal valorIbsMunicipal, decimal aliquotaEfetivaCbs, decimal valorCbs,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaItemImpostoIbsCbsId = vendaItemImpostoIbsCbsId;
            Cst = cst;
            CClassTrib = cClassTrib;
            AliquotaEfetivaIbsEstadual = aliquotaEfetivaIbsEstadual;
            ValorIbsEstadual = valorIbsEstadual;
            AliquotaEfetivaIbsMunicipal = aliquotaEfetivaIbsMunicipal;
            ValorIbsMunicipal = valorIbsMunicipal;
            AliquotaEfetivaCbs = aliquotaEfetivaCbs;
            ValorCbs = valorCbs;
        }

        public void Alterar(decimal aliquotaEfetivaIbsEstadual, decimal valorIbsEstadual, decimal aliquotaEfetivaIbsMunicipal, decimal valorIbsMunicipal, decimal aliquotaEfetivaCbs, decimal valorCbs, string alteradoPor)
        {
            AliquotaEfetivaIbsEstadual = aliquotaEfetivaIbsEstadual;
            ValorIbsEstadual = valorIbsEstadual;
            AliquotaEfetivaIbsMunicipal = aliquotaEfetivaIbsMunicipal;
            ValorIbsMunicipal = valorIbsMunicipal;
            AliquotaEfetivaCbs = aliquotaEfetivaCbs;
            ValorCbs = valorCbs;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaItemImpostoIbsCbsTributacaoRegular.Duplicar (novo Id/FK).</summary>
        public VendaItemImpostoIbsCbsTributacaoRegular Duplicar(Guid novoImpostoIbsCbsId, string criadoPor)
            => new(novoImpostoIbsCbsId, Cst, CClassTrib, AliquotaEfetivaIbsEstadual, ValorIbsEstadual,
                   AliquotaEfetivaIbsMunicipal, ValorIbsMunicipal, AliquotaEfetivaCbs, ValorCbs, TenantId, criadoPor);
    }
}
