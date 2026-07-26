using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaAutorizacaoXml (documentos autorizados a baixar o XML da venda).
    /// FK long -> Guid; VO Documento -> string; herda EntidadeSaaSBase.
    /// </summary>
    public class VendaAutorizacaoXml : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public string Documento { get; private set; } = string.Empty;

        // Navegação intra-módulo
        public Venda Venda { get; private set; } = null!;

        protected VendaAutorizacaoXml() { } // EF Core

        public VendaAutorizacaoXml(Guid vendaId, string documento, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            Documento = documento;
        }

        public void Alterar(string documento, string alteradoPor)
        {
            Documento = documento;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Porte fiel de VendaAutorizacaoXml.Duplicar (novo Id/FK).</summary>
        public VendaAutorizacaoXml Duplicar(Guid novaVendaId, string criadoPor)
            => new(novaVendaId, Documento, TenantId, criadoPor);
    }
}
