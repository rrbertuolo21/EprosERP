using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Saldo de estoque de um produto por empresa. Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Estoque.EstoqueProduto.
    /// EmpresaId referencia o módulo de plataforma/empresas por FK Guid (sem navegação cruzada).
    /// </summary>
    public class EstoqueProduto : EntidadeSaaSBase
    {
        public Guid EmpresaId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal QuantidadeSaldoEstoque { get; private set; }
        public decimal QuantidadeEstoqueMinimo { get; private set; }
        public decimal QuantidadeEstoqueMaximo { get; private set; }
        public decimal QuantidadeEstoqueReservado { get; private set; }
        public decimal ValorSaldo { get; private set; }
        public decimal ValorCustoMedio { get; private set; }
        public ETipoCusteioEstoque TipoCusteioEstoque { get; private set; } = ETipoCusteioEstoque.CustoMedio;

        // Navegação intra-módulo
        public Produto? Produto { get; private set; }

        protected EstoqueProduto() { } // EF Core

        public EstoqueProduto(Guid empresaId, Guid produtoId, decimal quantidadeSaldoEstoque, decimal quantidadeEstoqueMinimo, decimal quantidadeEstoqueMaximo, decimal quantidadeEstoqueReservado, decimal valorSaldo, ETipoCusteioEstoque tipoCusteioEstoque, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            EmpresaId = empresaId;
            ProdutoId = produtoId;
            QuantidadeSaldoEstoque = quantidadeSaldoEstoque;
            QuantidadeEstoqueMinimo = quantidadeEstoqueMinimo;
            QuantidadeEstoqueMaximo = quantidadeEstoqueMaximo;
            QuantidadeEstoqueReservado = quantidadeEstoqueReservado;
            ValorSaldo = valorSaldo;
            TipoCusteioEstoque = tipoCusteioEstoque;

            AtualizarValorCustoMedio();
        }

        public void Validar() { }

        public void SomarQuantidadeSaldoEstoque(decimal quantidade)
        {
            QuantidadeSaldoEstoque += quantidade;
        }

        public void DiminuirQuantidadeSaldoEstoque(decimal quantidade)
        {
            QuantidadeSaldoEstoque -= quantidade;
        }

        public void AtualizarValorSaldo(decimal valor)
        {
            ValorSaldo = valor;
        }

        public void AtualizarValorCustoMedio()
        {
            // D13 (ESTOQUE EST01, valida-contador): com quantidade zero (ou negativa, quando o produto
            // permite estoque negativo) NUNCA divide por zero e MANTÉM o último custo unitário. A próxima
            // entrada sobre saldo zero assume naturalmente o custo dessa entrada (ValorSaldo/Quantidade).
            if (QuantidadeSaldoEstoque <= decimal.Zero)
                return;

            ValorCustoMedio = ValorSaldo / QuantidadeSaldoEstoque;
        }

        public void SomarQuantidadeEstoqueReservado(decimal quantidade)
        {
            QuantidadeEstoqueReservado += quantidade;
        }

        public void DiminuirQuantidadeEstoqueReservado(decimal quantidade)
        {
            QuantidadeEstoqueReservado -= quantidade;
        }

        public void Alterar(Guid empresaId, Guid produtoId, decimal quantidadeSaldoEstoque, decimal quantidadeEstoqueMinimo, decimal quantidadeEstoqueMaximo, decimal quantidadeEstoqueReservado, decimal valorSaldo, ETipoCusteioEstoque tipoCusteioEstoque, string usuario)
        {
            EmpresaId = empresaId;
            ProdutoId = produtoId;
            QuantidadeSaldoEstoque = quantidadeSaldoEstoque;
            QuantidadeEstoqueMinimo = quantidadeEstoqueMinimo;
            QuantidadeEstoqueMaximo = quantidadeEstoqueMaximo;
            QuantidadeEstoqueReservado = quantidadeEstoqueReservado;
            ValorSaldo = valorSaldo;
            TipoCusteioEstoque = tipoCusteioEstoque;
            MarcarAlterado(usuario);
            AtualizarValorCustoMedio();
        }

        public void AlterarDadosAutorizados(Guid empresaId, Guid produtoId, decimal quantidadeEstoqueMinimo, decimal quantidadeEstoqueMaximo, ETipoCusteioEstoque tipoCusteioEstoque, string usuario)
        {
            EmpresaId = empresaId;
            ProdutoId = produtoId;
            QuantidadeEstoqueMinimo = quantidadeEstoqueMinimo;
            QuantidadeEstoqueMaximo = quantidadeEstoqueMaximo;
            TipoCusteioEstoque = tipoCusteioEstoque;
            MarcarAlterado(usuario);
        }
    }
}
