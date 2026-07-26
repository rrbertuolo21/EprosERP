using System;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Registro de avaria/perda de produto (EF Movimentação Manual e Ajustes §15.12).
    /// MVM-026: gera saída controlada do estoque. CategoriaId referencia CategoriaProduto (intra-módulo).
    /// </summary>
    public class AvariaEstoque : EntidadeSaaSBase
    {
        public Guid ProdutoId { get; private set; }
        public string? Codigo { get; private set; }
        public string Nome { get; private set; } = string.Empty;
        public Guid CategoriaId { get; private set; }
        public decimal PrecoCompra { get; private set; }
        public decimal Quantidade { get; private set; }
        public DateTime DataAvaria { get; private set; }
        public string? Nota { get; private set; }
        public string? Referencia { get; private set; }
        public EStatusRegistroEstoque Situacao { get; private set; } = EStatusRegistroEstoque.Rascunho;

        // Navegação intra-módulo
        public Produto? Produto { get; private set; }

        protected AvariaEstoque() { } // EF Core

        public AvariaEstoque(Guid produtoId, string? codigo, string nome, Guid categoriaId, decimal precoCompra, decimal quantidade, DateTime dataAvaria, string? nota, string? referencia, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            ProdutoId = produtoId;
            Codigo = codigo;
            Nome = nome ?? string.Empty;
            CategoriaId = categoriaId;
            PrecoCompra = precoCompra;
            Quantidade = quantidade;
            DataAvaria = dataAvaria;
            Nota = nota;
            Referencia = referencia;
            Situacao = EStatusRegistroEstoque.Rascunho;
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AvariaEstoque>()
                .Requires()
                .IsNotNullOrEmpty(Nome, nameof(Nome), "O nome do produto avariado é obrigatório [Origem: AvariaEstoque]")
                .IsGreaterThan(Quantidade, 0m, nameof(Quantidade), "A quantidade avariada deve ser maior que zero [Origem: AvariaEstoque]"));

            if (ProdutoId == Guid.Empty)
                AddNotification("ProdutoId", "O produto é obrigatório [Origem: AvariaEstoque]");
            if (CategoriaId == Guid.Empty)
                AddNotification("CategoriaId", "A categoria é obrigatória [Origem: AvariaEstoque]");
        }

        public void Aplicar(string usuario)
        {
            Situacao = EStatusRegistroEstoque.Aplicado;
            MarcarAlterado(usuario);
        }
    }
}
