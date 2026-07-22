using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Junção N:N entre ProdutoGrupo e Empresa. Restaura o relacionamento do legado
    /// (ProdutoGrupo.ICollection&lt;Empresa&gt;) por EmpresaId (Guid), sem navegação cross-module —
    /// Empresa vive no módulo GestaoClientes.
    /// </summary>
    public class ProdutoGrupoEmpresa : EntidadeSaaSBase
    {
        public Guid ProdutoGrupoId { get; private set; }
        public Guid EmpresaId { get; private set; }

        // Navegação intra-módulo
        public ProdutoGrupo? ProdutoGrupo { get; private set; }

        protected ProdutoGrupoEmpresa() { } // EF Core

        public ProdutoGrupoEmpresa(Guid produtoGrupoId, Guid empresaId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoGrupoId = produtoGrupoId;
            EmpresaId = empresaId;
        }
    }
}
