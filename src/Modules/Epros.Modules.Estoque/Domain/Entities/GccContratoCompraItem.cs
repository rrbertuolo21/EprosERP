using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item de contrato de compra (EF Gestão de Contratos de Compra §16.2 `gcc_contrato_compra_item`).
    /// GCC-004: exige produto. GCC-005: exige preço unitário. GCC-006: exige quantidade comprometida.
    /// GCC-008: saldo compara contratado versus consumido. ProdutoId é referência externa por FK Guid.
    /// </summary>
    public class GccContratoCompraItem : EntidadeSaaSBase
    {
        public Guid ContratoCompraId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal PrecoUnitario { get; private set; }
        public decimal QuantidadeComprometida { get; private set; }
        public decimal QuantidadeConsumida { get; private set; }
        public decimal ValorConsumido { get; private set; }
        public decimal SaldoQuantidade { get; private set; }
        public decimal SaldoValor { get; private set; }

        // Navegação intra-módulo
        public GccContratoCompra? Contrato { get; private set; }

        protected GccContratoCompraItem() { } // EF Core

        public GccContratoCompraItem(Guid contratoCompraId, Guid produtoId, decimal precoUnitario, decimal quantidadeComprometida, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ContratoCompraId = contratoCompraId;
            ProdutoId = produtoId;
            PrecoUnitario = precoUnitario;
            QuantidadeComprometida = quantidadeComprometida;
            QuantidadeConsumida = 0m;
            ValorConsumido = 0m;
            RecalcularSaldo();
            Validar();
        }

        /// <summary>Registra consumo do item (GCC-008): reduz saldo por quantidade e valor.</summary>
        public void RegistrarConsumo(decimal quantidade, decimal valor, string usuario)
        {
            QuantidadeConsumida += quantidade;
            ValorConsumido += valor;
            RecalcularSaldo();
            MarcarAlterado(usuario);
        }

        /// <summary>Aditivo de PREÇO (CD5): ajusta o preço unitário contratado e recompõe o saldo em valor.</summary>
        public void AjustarPreco(decimal novoPreco, string usuario)
        {
            if (novoPreco <= 0m)
            {
                AddNotification("PrecoUnitario", "O novo preço do aditivo deve ser maior que zero [GCC-020] [Origem: GccContratoCompraItem]");
                return;
            }
            PrecoUnitario = novoPreco;
            RecalcularSaldo();
            MarcarAlterado(usuario);
        }

        /// <summary>Aditivo de QUANTIDADE (CD5): aumenta a quantidade comprometida e recompõe o saldo.</summary>
        public void AumentarComprometido(decimal quantidadeAdicional, string usuario)
        {
            if (quantidadeAdicional <= 0m)
            {
                AddNotification("QuantidadeComprometida", "A quantidade adicional do aditivo deve ser maior que zero [GCC-021] [Origem: GccContratoCompraItem]");
                return;
            }
            QuantidadeComprometida += quantidadeAdicional;
            RecalcularSaldo();
            MarcarAlterado(usuario);
        }

        private void RecalcularSaldo()
        {
            SaldoQuantidade = QuantidadeComprometida - QuantidadeConsumida;
            SaldoValor = (PrecoUnitario * QuantidadeComprometida) - ValorConsumido;
        }

        public void Validar()
        {
            Clear();
            if (ProdutoId == Guid.Empty)
                AddNotification("ProdutoId", "O produto do item contratual é obrigatório [GCC-004] [Origem: GccContratoCompraItem]");
            if (PrecoUnitario <= 0m)
                AddNotification("PrecoUnitario", "O preço unitário é obrigatório [GCC-005] [Origem: GccContratoCompraItem]");
            if (QuantidadeComprometida <= 0m)
                AddNotification("QuantidadeComprometida", "A quantidade comprometida é obrigatória [GCC-006] [Origem: GccContratoCompraItem]");
        }
    }
}
