using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaNfHistorico (histórico de mensagens de retorno da SEFAZ).
    /// No legado era classe simples com Id long + TenantId + DataCadastro; aqui herda
    /// EntidadeSaaSBase (Id/TenantId/CriadoEm/auditoria já vêm da base).
    /// </summary>
    public class VendaNfHistorico : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaNfHistorico() { } // EF Core

        public VendaNfHistorico(Guid vendaId, string descricao, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            Descricao = descricao;
        }
    }
}
