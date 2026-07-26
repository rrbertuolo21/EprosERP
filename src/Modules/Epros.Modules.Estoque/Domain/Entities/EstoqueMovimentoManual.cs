using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Movimento manual de estoque (entrada/saída lançada manualmente). Porte fiel do legado
    /// Epros.ERP.Domain.Entities.Estoque.EstoqueMovimentoManual, expandido conforme a EF
    /// Movimentação Manual e Ajustes §15.2 (situação e motivo) e §11 (máquina de estados).
    /// </summary>
    public class EstoqueMovimentoManual : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public ETipoEstoque TipoEstoque { get; private set; }
        public ETipoMovimento TipoMovimento { get; private set; }
        public decimal QuantidadeMovimentada { get; private set; }
        public decimal ValorUnitario { get; private set; }

        /// <summary>Situação do movimento (Rascunho, Aplicado, Cancelado, Estornado) — EF §15.2/§11.</summary>
        public EStatusMovimentoEstoque Situacao { get; private set; } = EStatusMovimentoEstoque.Rascunho;

        /// <summary>Motivo operacional; obrigatório em ajuste/estorno — EF §15.2.</summary>
        public string? Motivo { get; private set; }

        // Navegação intra-módulo
        public Produto? Produto { get; private set; }
        public FatoGeradorEstoque? FatoGeradorEstoque { get; private set; }

        protected EstoqueMovimentoManual() { } // EF Core

        public EstoqueMovimentoManual(Guid produtoId, ETipoEstoque tipoEstoque, ETipoMovimento tipoMovimento, decimal quantidadeMovimentada, decimal valorUnitario, string tenantId, string criadoPor, string? motivo = null)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            TipoEstoque = tipoEstoque;
            TipoMovimento = tipoMovimento;
            QuantidadeMovimentada = quantidadeMovimentada;
            ValorUnitario = valorUnitario;
            Motivo = motivo;
            Situacao = EStatusMovimentoEstoque.Rascunho;
            Validar();
        }

        /// <summary>
        /// Validações obrigatórias MVM-001..005 (produto, tipo de estoque, tipo de movimento,
        /// quantidade e valor unitário). EF §8.
        /// </summary>
        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<EstoqueMovimentoManual>()
                .Requires()
                .IsGreaterThan(QuantidadeMovimentada, 0m, nameof(QuantidadeMovimentada), "A quantidade movimentada deve ser maior que zero [MVM-004] [Origem: EstoqueMovimentoManual]")
                .IsGreaterThan(ValorUnitario, -0.01m, nameof(ValorUnitario), "O valor unitário é obrigatório e não pode ser negativo [MVM-005] [Origem: EstoqueMovimentoManual]"));

            if (ProdutoId == Guid.Empty)
                AddNotification("ProdutoId", "O produto é obrigatório [MVM-001] [Origem: EstoqueMovimentoManual]");
        }

        public void Alterar(Guid produtoId, ETipoEstoque tipoEstoque, ETipoMovimento tipoMovimento, decimal quantidadeMovimentada, decimal valorUnitario, string usuario, string? motivo = null)
        {
            ProdutoId = produtoId;
            TipoEstoque = tipoEstoque;
            TipoMovimento = tipoMovimento;
            QuantidadeMovimentada = quantidadeMovimentada;
            ValorUnitario = valorUnitario;
            Motivo = motivo;
            MarcarAlterado(usuario);
            Validar();
        }

        /// <summary>Marca o movimento como aplicado (ficha e saldo atualizados) — EF §11.</summary>
        public void Aplicar(string usuario)
        {
            Situacao = EStatusMovimentoEstoque.Aplicado;
            MarcarAlterado(usuario);
        }

        /// <summary>Cancela um movimento ainda sem efeito final — EF §11.</summary>
        public void Cancelar(string usuario)
        {
            Situacao = EStatusMovimentoEstoque.Cancelado;
            MarcarAlterado(usuario);
        }

        /// <summary>Marca o movimento aplicado como estornado (efeito revertido por compensação) — EF §11 / MVM-030.</summary>
        public void Estornar(string usuario, string motivo)
        {
            Situacao = EStatusMovimentoEstoque.Estornado;
            Motivo = motivo;
            MarcarAlterado(usuario);
        }
    }
}
