using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaItemImpostoValorAproximado (valor aproximado dos tributos - IBPT).
    /// FK long -> Guid; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaItemImpostoValorAproximado : EntidadeSaaSBase
    {
        public Guid VendaItemId { get; private set; }
        public decimal AliquotaNacionalFederal { get; private set; }
        public decimal AliquotaImportadoFederal { get; private set; }
        public decimal AliquotaEstadual { get; private set; }
        public decimal AliquotaMunicipal { get; private set; }
        public string? Versao { get; private set; }
        public string? Fonte { get; private set; }

        // Navegação intra-módulo
        public VendaItem VendaItem { get; private set; } = null!;

        protected VendaItemImpostoValorAproximado() { } // EF Core

        public VendaItemImpostoValorAproximado(Guid vendaItemId, decimal aliquotaNacionalFederal, decimal aliquotaImportadoFederal, decimal aliquotaEstadual, decimal aliquotaMunicipal, string versao, string fonte,
            string tenantId, string criadoPor) : base(tenantId, criadoPor)
        {
            VendaItemId = vendaItemId;
            AliquotaNacionalFederal = aliquotaNacionalFederal;
            AliquotaImportadoFederal = aliquotaImportadoFederal;
            AliquotaEstadual = aliquotaEstadual;
            AliquotaMunicipal = aliquotaMunicipal;
            Versao = versao;
            Fonte = fonte;
        }

        public void Alterar(decimal aliquotaNacionalFederal, decimal aliquotaImportadoFederal, decimal aliquotaEstadual, decimal aliquotaMunicipal, string versao, string fonte, string alteradoPor)
        {
            AliquotaNacionalFederal = aliquotaNacionalFederal;
            AliquotaImportadoFederal = aliquotaImportadoFederal;
            AliquotaEstadual = aliquotaEstadual;
            AliquotaMunicipal = aliquotaMunicipal;
            Versao = versao;
            Fonte = fonte;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaItemImpostoValorAproximado.Duplicar (novo Id/FK).</summary>
        public VendaItemImpostoValorAproximado Duplicar(Guid novoItemId, string criadoPor)
            => new(novoItemId, AliquotaNacionalFederal, AliquotaImportadoFederal, AliquotaEstadual, AliquotaMunicipal,
                   Versao ?? string.Empty, Fonte ?? string.Empty, TenantId, criadoPor);
    }
}
