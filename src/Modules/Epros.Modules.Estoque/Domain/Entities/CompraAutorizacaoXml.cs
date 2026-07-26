using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// CPF/CNPJ autorizado a fazer download do XML da NF-e da compra. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Compras.CompraAutorizacaoXml. O ValueObject Documento do legado
    /// foi achatado para a string Documento (padrão do módulo).
    /// </summary>
    public class CompraAutorizacaoXml : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public string Documento { get; private set; } = string.Empty;

        // Navegação intra-módulo
        public Compra? Compra { get; private set; }

        protected CompraAutorizacaoXml() { } // EF Core

        public CompraAutorizacaoXml(Guid compraId, string documento, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            CompraId = compraId;
            Documento = documento ?? string.Empty;
        }

        public void Alterar(string documento, string usuario)
        {
            Documento = documento ?? string.Empty;
            MarcarAlterado(usuario);
        }
    }
}
