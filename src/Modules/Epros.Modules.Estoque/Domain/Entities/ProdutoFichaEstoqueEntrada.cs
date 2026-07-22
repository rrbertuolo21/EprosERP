using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Ficha (kardex) de entrada de estoque de um produto por empresa. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Estoque.ProdutoFichaEstoqueEntrada.
    /// EmpresaId referencia o módulo de plataforma/empresas por FK Guid (sem navegação cruzada).
    /// </summary>
    public class ProdutoFichaEstoqueEntrada : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public Guid FatoGeradorEstoqueId { get; private set; }
        public ETipoEstoque TipoEstoque { get; private set; }
        public decimal QuantidadeMovimentada { get; private set; }
        public decimal ValorUnitario { get; private set; }
        public decimal QuantidadeSaldo { get; private set; }
        public decimal ValorSaldo { get; private set; }

        // Navegação intra-módulo
        public Produto? Produto { get; private set; }
        public FatoGeradorEstoque? FatoGeradorEstoque { get; private set; }
        public ICollection<ProdutoFichaEstoqueSaida> ProdutoFichaEstoqueSaidas { get; private set; } = new List<ProdutoFichaEstoqueSaida>();

        protected ProdutoFichaEstoqueEntrada() { } // EF Core

        public ProdutoFichaEstoqueEntrada(Guid empresaId, Guid produtoId, Guid fatoGeradorEstoqueId, ETipoEstoque tipoEstoque, decimal quantidadeMovimentada, decimal valorUnitario, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            ProdutoId = produtoId;
            FatoGeradorEstoqueId = fatoGeradorEstoqueId;
            TipoEstoque = tipoEstoque;
            QuantidadeMovimentada = quantidadeMovimentada;
            ValorUnitario = valorUnitario;

            QuantidadeSaldo = quantidadeMovimentada;

            CalcularValorSaldo();
        }

        public void Validar() { }

        public void CalcularValorSaldo()
        {
            ValorSaldo = ValorUnitario * QuantidadeSaldo;
        }

        public void AtualizarQuantidadeSaldo(decimal quantidade)
        {
            QuantidadeSaldo -= quantidade;
            CalcularValorSaldo();
        }

        public void EstornarQuantidade(decimal quantidade)
        {
            QuantidadeSaldo += quantidade;
            CalcularValorSaldo();
        }
    }
}
