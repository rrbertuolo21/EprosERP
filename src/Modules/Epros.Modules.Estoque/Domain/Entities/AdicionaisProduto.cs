using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Vínculo N:N entre Produto e Adicionais. Porte fiel do legado Epros.ERP.Domain.Entities.Cadastros.Produtos.AdicionaisProduto.
    /// </summary>
    public class AdicionaisProduto : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public Guid AdicionaisId { get; private set; }

        // Navegação intra-módulo
        public Produto? Produto { get; private set; }
        public Adicionais? Adicionais { get; private set; }

        protected AdicionaisProduto() { } // EF Core

        public AdicionaisProduto(Guid produtoId, Guid adicionaisId, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            AdicionaisId = adicionaisId;
            Validar();
        }

        public void Validar()
        {
            if (Adicionais != null && Adicionais.Notifications != null)
            {
                AddNotifications(Adicionais.Notifications);
            }
        }

        public void Alterar(Guid adicionaisId, string usuario)
        {
            AdicionaisId = adicionaisId;
            MarcarAlterado(usuario);
            Validar();
        }
    }
}
