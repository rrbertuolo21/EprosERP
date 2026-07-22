using System;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Ficha (kardex) de saída de estoque de um produto por empresa, vinculada à ficha de entrada consumida.
    /// Porte fiel do legado Epros.ERP.Domain.Entities.Estoque.ProdutoFichaEstoqueSaida.
    /// EmpresaId referencia o módulo de plataforma/empresas por FK Guid (sem navegação cruzada).
    /// </summary>
    public class ProdutoFichaEstoqueSaida : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid FatoGeradorEstoqueId { get; private set; }
        public Guid ProdutoFichaEstoqueEntradaId { get; private set; }
        public decimal QuantidadeMovimentada { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal ValorTotal { get; private set; }
        public decimal ValorCustoMedio { get; private set; }
        public decimal ValorTotalCustoMedio { get; private set; }

        // Navegação intra-módulo
        public Produto? Produto { get; private set; }
        public FatoGeradorEstoque? FatoGeradorEstoque { get; private set; }
        public ProdutoFichaEstoqueEntrada? ProdutoFichaEstoqueEntrada { get; private set; }

        protected ProdutoFichaEstoqueSaida() { } // EF Core

        public ProdutoFichaEstoqueSaida(Guid empresaId, Guid produtoId, Guid fatoGeradorEstoqueId, Guid produtoFichaEstoqueEntradaId, decimal quantidadeMovimentada, decimal valorUnitario, decimal valorCustoMedio, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            ProdutoId = produtoId;
            FatoGeradorEstoqueId = fatoGeradorEstoqueId;
            ProdutoFichaEstoqueEntradaId = produtoFichaEstoqueEntradaId;
            QuantidadeMovimentada = quantidadeMovimentada;
            ValorUnitario = valorUnitario;
            ValorCustoMedio = valorCustoMedio;

            CalcularValorTotal();
            CalcularValorTotalCustoMedio();
        }

        public void Validar() { }

        public void AtualizarValorUnitario(decimal valorUnitario)
        {
            ValorUnitario = valorUnitario;
        }

        public void CalcularValorTotal()
        {
            ValorTotal = ValorUnitario * QuantidadeMovimentada;
        }

        public void AtualizarCustoMedio(decimal valorCustoMedio)
        {
            ValorCustoMedio = valorCustoMedio;
        }

        public void CalcularValorTotalCustoMedio()
        {
            ValorTotalCustoMedio = ValorCustoMedio * QuantidadeMovimentada;
        }
    }
}
