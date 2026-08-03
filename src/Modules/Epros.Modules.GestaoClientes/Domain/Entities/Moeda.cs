using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class Moeda : EntidadeSaaSBase, IGlobalEntity
    {
        public string CodigoISO { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public string Simbolo { get; private set; } = string.Empty;
        public int CasasDecimais { get; private set; }

        protected Moeda() { } // EF Core

        public Moeda(string codigoISO, string simbolo, int casasDecimais, string criadoPor, string nome = "")
            : base("system", criadoPor)
        {
            AddNotifications(new Contract<Moeda>()
                .Requires()
                .IsNotNullOrEmpty(codigoISO, nameof(CodigoISO), "Código ISO é obrigatório.")
                .IsTrue(!string.IsNullOrEmpty(codigoISO) && codigoISO.Length == 3, nameof(CodigoISO), "Código ISO deve ter exatamente 3 caracteres.")
                .IsNotNullOrEmpty(simbolo, nameof(Simbolo), "Símbolo da moeda é obrigatório.")
                .HasMaxLen(simbolo, 5, nameof(Simbolo), "Símbolo deve ter no máximo 5 caracteres.")
                .IsGreaterThan(casasDecimais, -1, nameof(CasasDecimais), "Casas decimais deve ser maior ou igual a zero.")
            );

            CodigoISO = codigoISO.ToUpperInvariant();
            Nome = string.IsNullOrWhiteSpace(nome) ? codigoISO.ToUpperInvariant() : nome;
            Simbolo = simbolo;
            CasasDecimais = casasDecimais;
        }

        public void Atualizar(string codigoISO, string simbolo, int casasDecimais, string alteradoPor, string nome = "")
        {
            AddNotifications(new Contract<Moeda>()
                .Requires()
                .IsNotNullOrEmpty(codigoISO, nameof(CodigoISO), "Código ISO é obrigatório.")
                .IsTrue(!string.IsNullOrEmpty(codigoISO) && codigoISO.Length == 3, nameof(CodigoISO), "Código ISO deve ter exatamente 3 caracteres.")
                .IsNotNullOrEmpty(simbolo, nameof(Simbolo), "Símbolo da moeda é obrigatório.")
                .HasMaxLen(simbolo, 5, nameof(Simbolo), "Símbolo deve ter no máximo 5 caracteres.")
                .IsGreaterThan(casasDecimais, -1, nameof(CasasDecimais), "Casas decimais deve ser maior ou igual a zero.")
            );

            if (IsValid)
            {
                CodigoISO = codigoISO.ToUpperInvariant();
                if (!string.IsNullOrWhiteSpace(nome)) Nome = nome;
                Simbolo = simbolo;
                CasasDecimais = casasDecimais;
                MarcarAlterado(alteradoPor);
            }
        }
    }
}
