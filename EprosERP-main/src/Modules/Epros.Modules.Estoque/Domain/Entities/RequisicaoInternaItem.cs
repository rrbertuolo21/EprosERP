using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item de requisição interna (EF Movimentação Manual e Ajustes §15.14).
    /// </summary>
    public class RequisicaoInternaItem : EntidadeSaaSBase
    {
        public Guid RequisicaoInternaId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }

        // Navegação intra-módulo
        public RequisicaoInterna? Requisicao { get; private set; }
        public Produto? Produto { get; private set; }

        protected RequisicaoInternaItem() { } // EF Core

        public RequisicaoInternaItem(Guid requisicaoInternaId, Guid produtoId, decimal quantidade, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            RequisicaoInternaId = requisicaoInternaId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
        }

        public void Validar() { }
    }
}
