using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaNfeHistorico (Epros.ERP.Domain.Entities.Vendas.VendaNfeHistorico).
    /// No legado era classe mínima que herdava Entity e expunha somente o vínculo com a venda
    /// (long VendaId). Aqui herda EntidadeSaaSBase (Id/TenantId/CriadoEm/auditoria vêm da base)
    /// e a FK long -> Guid. Entidade distinta de VendaNfHistorico (que carrega Descricao).
    /// </summary>
    public class VendaNfeHistorico : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaNfeHistorico() { } // EF Core

        public VendaNfeHistorico(Guid vendaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
        }
    }
}
