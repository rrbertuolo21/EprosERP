using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    /// <summary>
    /// Item adicional que pode ser vinculado a produtos. Porte fiel do legado Epros.ERP.Domain.Entities.Cadastros.Produtos.Adicionais.
    /// </summary>
    public class Adicionais : EntidadeSaaSBase
    {
        public string Descricao { get; private set; } = string.Empty;
        public decimal ValorPreco { get; private set; }

        // Navegação intra-módulo
        public ICollection<AdicionaisProduto> AdicionaisProdutos { get; private set; } = new List<AdicionaisProduto>();

        protected Adicionais() { } // EF Core

        public Adicionais(string descricao, decimal valorPreco, string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            Descricao = descricao;
            ValorPreco = valorPreco;
            Validar();
        }

        public void Alterar(string descricao, decimal valorPreco, string usuario)
        {
            Descricao = descricao;
            ValorPreco = valorPreco;
            MarcarAlterado(usuario);
            Validar();
        }

        public void Validar()
        {
            AddNotifications(new Contract<Adicionais>()
                .Requires()
                .IsLowerOrEqualsThan((Descricao ?? "").Length, 60, nameof(Descricao), "O campo Descricao deve ter no máximo 60 caracteres [Origem: Adicionais]")
            );
        }
    }
}
