using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Valor aproximado de tributos (Lei da Transparência) de um item de compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraItemImpostoValorAproximado.
    /// </summary>
    public class CompraItemImpostoValorAproximado : EntidadeSaaSBase
    {
        public Guid CompraItemId { get; private set; }
        public decimal AliquotaNacionalFederal { get; private set; }  // (15,2)
        public decimal AliquotaImportadoFederal { get; private set; } // (15,2)
        public decimal AliquotaEstadual { get; private set; }  // (15,2)
        public decimal AliquotaMunicipal { get; private set; }  // (15,2)
        public string? Versao { get; private set; }  // 10
        public string? Fonte { get; private set; }  // 60

        // Navegação intra-módulo
        public CompraItem? CompraItem { get; private set; }

        protected CompraItemImpostoValorAproximado() { } // EF Core

        public CompraItemImpostoValorAproximado(Guid compraItemId, decimal aliquotaNacionalFederal, decimal aliquotaImportadoFederal, decimal aliquotaEstadual, decimal aliquotaMunicipal, string? versao, string? fonte, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraItemId = compraItemId;
            AliquotaNacionalFederal = aliquotaNacionalFederal;
            AliquotaImportadoFederal = aliquotaImportadoFederal;
            AliquotaEstadual = aliquotaEstadual;
            AliquotaMunicipal = aliquotaMunicipal;
            Versao = versao;
            Fonte = fonte;
        }

        public void Alterar(decimal aliquotaNacionalFederal, decimal aliquotaImportadoFederal, decimal aliquotaEstadual, decimal aliquotaMunicipal, string? versao, string? fonte, string usuario)
        {
            AliquotaNacionalFederal = aliquotaNacionalFederal;
            AliquotaImportadoFederal = aliquotaImportadoFederal;
            AliquotaEstadual = aliquotaEstadual;
            AliquotaMunicipal = aliquotaMunicipal;
            Versao = versao;
            Fonte = fonte;
            MarcarAlterado(usuario);
        }
    }
}
