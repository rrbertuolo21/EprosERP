using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Add-on / módulo habilitável do catálogo modular (APP-CAT 11.7). Sem tenant (IGlobalEntity).</summary>
    public class AddOn : EntidadeSaaSBase, IGlobalEntity
    {
        public string NomeModulo { get; private set; } = string.Empty;
        public string? Alias { get; private set; }
        public decimal PrecoMensal { get; private set; }
        public decimal PrecoAnual { get; private set; }
        public string? Midia { get; private set; }
        public bool Habilitado { get; private set; }
        public bool Admin { get; private set; }
        public Guid? ParentAddOnId { get; private set; }

        protected AddOn() { } // EF Core

        public AddOn(
            string nomeModulo,
            string? alias,
            decimal precoMensal,
            decimal precoAnual,
            string? midia,
            bool habilitado,
            bool admin,
            Guid? parentAddOnId,
            string criadoPor)
            : base("system", criadoPor)
        {
            NomeModulo = nomeModulo;
            Alias = alias;
            PrecoMensal = precoMensal;
            PrecoAnual = precoAnual;
            Midia = midia;
            Habilitado = habilitado;
            Admin = admin;
            ParentAddOnId = parentAddOnId;
            Validar();
        }

        public void Alterar(string? alias, decimal precoMensal, decimal precoAnual, string? midia, Guid? parentAddOnId, string alteradoPor)
        {
            Alias = alias;
            PrecoMensal = precoMensal;
            PrecoAnual = precoAnual;
            Midia = midia;
            ParentAddOnId = parentAddOnId;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Habilitar(string alteradoPor)
        {
            Habilitado = true;
            MarcarAlterado(alteradoPor);
        }

        public void Desabilitar(string alteradoPor)
        {
            Habilitado = false;
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<AddOn>()
                .Requires()
                .IsNotNullOrEmpty(NomeModulo, nameof(NomeModulo), "NomeModulo é obrigatório [Origem: AddOn]")
                .HasMaxLen(NomeModulo ?? string.Empty, 100, nameof(NomeModulo), "NomeModulo deve ter no máximo 100 caracteres [Origem: AddOn]")
                .HasMaxLen(Alias ?? string.Empty, 100, nameof(Alias), "Alias deve ter no máximo 100 caracteres [Origem: AddOn]")
                .HasMaxLen(Midia ?? string.Empty, 500, nameof(Midia), "Midia deve ter no máximo 500 caracteres [Origem: AddOn]")
                .IsGreaterOrEqualsThan(PrecoMensal, 0, nameof(PrecoMensal), "PrecoMensal deve ser maior ou igual a zero [Origem: AddOn]")
                .IsGreaterOrEqualsThan(PrecoAnual, 0, nameof(PrecoAnual), "PrecoAnual deve ser maior ou igual a zero [Origem: AddOn]")
            );
        }
    }
}
