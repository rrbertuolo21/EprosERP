using System;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Capacidade granular (RBAC) - APP-TEN-003 11.5. Chave granular além do menu.</summary>
    public class Capacidade : EntidadeSaaSBase
    {
        public string Name { get; private set; } = string.Empty;
        public string? Label { get; private set; }
        public string? Module { get; private set; }
        public string? AddOn { get; private set; }
        public string? PermissionKey { get; private set; }

        protected Capacidade() { } // EF Core

        public Capacidade(
            string name,
            string? label,
            string? module,
            string? addOn,
            string? permissionKey,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Name = name;
            Label = label;
            Module = module;
            AddOn = addOn;
            PermissionKey = permissionKey;
            Validar();
        }

        public void Alterar(
            string name,
            string? label,
            string? module,
            string? addOn,
            string? permissionKey,
            string alteradoPor)
        {
            Name = name;
            Label = label;
            Module = module;
            AddOn = addOn;
            PermissionKey = permissionKey;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Capacidade>()
                .Requires()
                .IsNotNullOrEmpty(Name, nameof(Name), "Name é obrigatório [Origem: Capacidade]")
                .HasMaxLen(Name ?? string.Empty, 100, nameof(Name), "Name deve ter no máximo 100 caracteres [Origem: Capacidade]")
                .HasMaxLen(Label ?? string.Empty, 100, nameof(Label), "Label deve ter no máximo 100 caracteres [Origem: Capacidade]")
                .HasMaxLen(Module ?? string.Empty, 100, nameof(Module), "Module deve ter no máximo 100 caracteres [Origem: Capacidade]")
                .HasMaxLen(AddOn ?? string.Empty, 100, nameof(AddOn), "AddOn deve ter no máximo 100 caracteres [Origem: Capacidade]")
                .HasMaxLen(PermissionKey ?? string.Empty, 100, nameof(PermissionKey), "PermissionKey deve ter no máximo 100 caracteres [Origem: Capacidade]")
            );
        }
    }
}
