using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Pais : EntidadeSaaSBase, IGlobalEntity
    {
        public string Nome { get; private set; } = string.Empty;
        public string CodigoIsoAlpha2 { get; private set; } = string.Empty;
        public string CodigoIsoAlpha3 { get; private set; } = string.Empty;
        public string CodigoNumerico { get; private set; } = string.Empty;
        public string? Capital { get; private set; }
        public string? CodigoDiscagem { get; private set; }
        public bool Ativo { get; private set; }

        protected Pais() { } // EF Core

        public Pais(
            string nome,
            string codigoIsoAlpha2,
            string codigoIsoAlpha3,
            string codigoNumerico,
            string? capital,
            string? codigoDiscagem,
            string criadoPor)
            : base("system", criadoPor)
        {
            AddNotifications(new Contract<Pais>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome do país é obrigatório.")
                .HasMaxLen(nome, 60, nameof(Nome), "O Nome do país deve ter no máximo 60 caracteres.")
                .IsNotNullOrEmpty(codigoIsoAlpha2, nameof(CodigoIsoAlpha2), "O Código ISO Alpha-2 é obrigatório.")
                .IsTrue(!string.IsNullOrEmpty(codigoIsoAlpha2) && codigoIsoAlpha2.Length == 2, nameof(CodigoIsoAlpha2), "O Código ISO Alpha-2 deve ter exatamente 2 caracteres.")
                .IsNotNullOrEmpty(codigoIsoAlpha3, nameof(CodigoIsoAlpha3), "O Código ISO Alpha-3 é obrigatório.")
                .IsTrue(!string.IsNullOrEmpty(codigoIsoAlpha3) && codigoIsoAlpha3.Length == 3, nameof(CodigoIsoAlpha3), "O Código ISO Alpha-3 deve ter exatamente 3 caracteres.")
                .IsNotNullOrEmpty(codigoNumerico, nameof(CodigoNumerico), "O Código Numérico é obrigatório.")
                .IsTrue(!string.IsNullOrEmpty(codigoNumerico) && codigoNumerico.Length == 3, nameof(CodigoNumerico), "O Código Numérico deve ter exatamente 3 caracteres.")
            );

            Nome = nome;
            CodigoIsoAlpha2 = codigoIsoAlpha2.ToUpperInvariant();
            CodigoIsoAlpha3 = codigoIsoAlpha3.ToUpperInvariant();
            CodigoNumerico = codigoNumerico;
            Capital = capital;
            CodigoDiscagem = codigoDiscagem;
            Ativo = true;
        }

        public void Atualizar(string nome, string? capital, string? codigoDiscagem, string alteradoPor)
        {
            AddNotifications(new Contract<Pais>()
                .Requires()
                .IsNotNullOrEmpty(nome, nameof(Nome), "O Nome do país é obrigatório.")
                .HasMaxLen(nome, 60, nameof(Nome), "O Nome do país deve ter no máximo 60 caracteres.")
            );
            if (IsValid)
            {
                Nome = nome;
                Capital = capital;
                CodigoDiscagem = codigoDiscagem;
                MarcarAlterado(alteradoPor);
            }
        }

        public void Inativar(string alteradoPor)
        {
            Ativo = false;
            MarcarAlterado(alteradoPor);
        }

        public void Reativar(string alteradoPor)
        {
            Ativo = true;
            MarcarAlterado(alteradoPor);
        }
    }
}
