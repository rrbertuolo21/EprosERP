using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item retornado pelo terceiro (EF Subcontratação §7.4 `sub_retorno_item`). SUB-004: registra produtos e
    /// quantidades retornadas, com quantidade aprovada, perda, sucata e rendimento. ProdutoId é referência
    /// externa por FK Guid. Tratamento de perda/sucata/rendimento é pendência de validação (§15).
    /// </summary>
    public class SubRetornoItem : EntidadeSaaSBase
    {
        public Guid RetornoId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal QuantidadeRetorno { get; private set; }
        public decimal? QuantidadeAprovada { get; private set; }
        public decimal? QuantidadePerda { get; private set; }
        public decimal? QuantidadeSucata { get; private set; }
        public decimal? Rendimento { get; private set; }

        // Navegação intra-módulo
        public SubRetorno? Retorno { get; private set; }

        protected SubRetornoItem() { } // EF Core

        public SubRetornoItem(Guid retornoId, Guid produtoId, decimal quantidadeRetorno, decimal? quantidadeAprovada, decimal? quantidadePerda, decimal? quantidadeSucata, decimal? rendimento, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            RetornoId = retornoId;
            ProdutoId = produtoId;
            QuantidadeRetorno = quantidadeRetorno;
            QuantidadeAprovada = quantidadeAprovada;
            QuantidadePerda = quantidadePerda;
            QuantidadeSucata = quantidadeSucata;
            Rendimento = rendimento;
            Validar();
        }

        public void Validar()
        {
            Clear();
            if (ProdutoId == Guid.Empty)
                AddNotification("ProdutoId", "O produto retornado é obrigatório [SUB-004] [Origem: SubRetornoItem]");
            if (QuantidadeRetorno <= 0m)
                AddNotification("QuantidadeRetorno", "A quantidade retornada deve ser maior que zero [SUB-004] [Origem: SubRetornoItem]");
        }
    }
}
