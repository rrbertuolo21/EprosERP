using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Financeiro.Domain.Entities
{
    public class Banco : EntidadeSaaSBase, IGlobalEntity
    {
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;

        protected Banco() { } // EF Core

        public Banco(string codigo, string descricao, string criadoPor) : base("system", criadoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            Validar();
        }

        public void Alterar(string codigo, string descricao, string alteradoPor)
        {
            Codigo = codigo;
            Descricao = descricao;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Banco>()
                .Requires()
                .IsNotNullOrEmpty(Codigo, nameof(Codigo), "O código é obrigatório")
                .IsLowerOrEqualsThan(Codigo ?? string.Empty, 3, nameof(Codigo), "O código deve ter no máximo 3 caracteres")
                .IsNotNullOrEmpty(Descricao, nameof(Descricao), "A descrição é obrigatória")
                .IsLowerOrEqualsThan(Descricao ?? string.Empty, 250, nameof(Descricao), "A descrição deve ter no máximo 250 caracteres")
            );
        }
    }
}
