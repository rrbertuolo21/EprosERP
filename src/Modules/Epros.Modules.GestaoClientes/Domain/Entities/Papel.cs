using System;
using System.Collections.Generic;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    /// <summary>Papel funcional (RBAC) - APP-TEN-003 11.4. Complementa o PerfilAcesso, dono do RBAC.</summary>
    public class Papel : EntidadeSaaSBase
    {
        public string Name { get; private set; } = string.Empty;
        public string? Label { get; private set; }
        public string? GuardName { get; private set; }
        public bool Editable { get; private set; }
        public bool RoleSystem { get; private set; }
        public ETipoPapel? RoleType { get; private set; }
        public string? RoleHomepage { get; private set; }
        public string? Modules { get; private set; }

        public List<PapelCapacidade> Capacidades { get; private set; } = new();

        protected Papel() { } // EF Core

        public Papel(
            string name,
            string? label,
            string? guardName,
            bool editable,
            bool roleSystem,
            ETipoPapel? roleType,
            string? roleHomepage,
            string? modules,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            Name = name;
            Label = label;
            GuardName = guardName;
            Editable = editable;
            RoleSystem = roleSystem;
            RoleType = roleType;
            RoleHomepage = roleHomepage;
            Modules = modules;
            Validar();
        }

        public void Alterar(
            string name,
            string? label,
            string? guardName,
            bool editable,
            ETipoPapel? roleType,
            string? roleHomepage,
            string? modules,
            string alteradoPor)
        {
            // REG-043: papéis protegidos do sistema não devem ser rebaixados sem governança.
            if (RoleSystem)
            {
                AddNotification(nameof(RoleSystem), "Papel de sistema protegido não pode ser alterado [Origem: Papel]");
                return;
            }

            Name = name;
            Label = label;
            GuardName = guardName;
            Editable = editable;
            RoleType = roleType;
            RoleHomepage = roleHomepage;
            Modules = modules;
            MarcarAlterado(alteradoPor);
            Validar();
        }

        public void DefinirCapacidades(List<PapelCapacidade> capacidades, string alteradoPor)
        {
            Capacidades.Clear();
            foreach (var cap in capacidades)
            {
                if (cap.IsValid) Capacidades.Add(cap);
                else AddNotifications(cap.Notifications);
            }
            MarcarAlterado(alteradoPor);
        }

        public void Validar()
        {
            Clear();
            AddNotifications(new Contract<Papel>()
                .Requires()
                .IsNotNullOrEmpty(Name, nameof(Name), "Name é obrigatório [Origem: Papel]")
                .HasMaxLen(Name ?? string.Empty, 100, nameof(Name), "Name deve ter no máximo 100 caracteres [Origem: Papel]")
                .HasMaxLen(Label ?? string.Empty, 100, nameof(Label), "Label deve ter no máximo 100 caracteres [Origem: Papel]")
                .HasMaxLen(GuardName ?? string.Empty, 100, nameof(GuardName), "GuardName deve ter no máximo 100 caracteres [Origem: Papel]")
                .HasMaxLen(RoleHomepage ?? string.Empty, 250, nameof(RoleHomepage), "RoleHomepage deve ter no máximo 250 caracteres [Origem: Papel]")
            );
        }
    }
}
