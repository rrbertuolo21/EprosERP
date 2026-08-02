using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Vínculo rastreável entre o item de inventário e o movimento de estoque gerado na aprovação
    /// (EF Inventário §16.5). INV-014 / CA-007: o ajuste aprovado gera movimento rastreável.
    /// [DECIDIDO D1/D3] O movimento passa pelo motor único (kardex) e é sempre por diferença.
    /// </summary>
    public class InventarioMovimentoAjuste : EntidadeSaaSBase
    {
        public Guid InventarioId { get; private set; }
        public Guid InventarioItemId { get; private set; }
        public Guid ProdutoId { get; private set; }
        /// <summary>Fato gerador (kardex) que originou o movimento — trilha de auditoria (D1).</summary>
        public Guid? FatoGeradorEstoqueId { get; private set; }
        public ETipoAplicacaoAjusteInventario TipoAplicacao { get; private set; }
        /// <summary>Quantidade aplicada ao saldo (com sinal: + entrada, − saída).</summary>
        public decimal QuantidadeAplicada { get; private set; }

        protected InventarioMovimentoAjuste() { } // EF Core

        public InventarioMovimentoAjuste(Guid inventarioId, Guid inventarioItemId, Guid produtoId, Guid? fatoGeradorEstoqueId, decimal quantidadeAplicada, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            InventarioId = inventarioId;
            InventarioItemId = inventarioItemId;
            ProdutoId = produtoId;
            FatoGeradorEstoqueId = fatoGeradorEstoqueId;
            TipoAplicacao = ETipoAplicacaoAjusteInventario.AjustePorDiferenca;
            QuantidadeAplicada = quantidadeAplicada;
        }
    }
}
