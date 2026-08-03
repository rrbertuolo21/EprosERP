using System;
using Flunt.Notifications;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    // Catálogo GLOBAL (item de menu nível 2). Ver Menu — IGlobalEntity classifica a fronteira (REG-001).
    public class MenuItemNivel2 : Notifiable<Notification>, Epros.Shared.Domain.Entities.IGlobalEntity
    {
        public Guid Id { get; private set; }
        public Guid MenuItemNivel1Id { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string? Icon { get; private set; }
        public string? To { get; private set; }
        public int Ordem { get; private set; }
        public string? Modulo { get; private set; }
        // 1.10 — capacidade RBAC exigida para ver este item (ver Menu.CapacidadeRequerida).
        public string? CapacidadeRequerida { get; private set; }

        protected MenuItemNivel2() { } // EF Core

        public MenuItemNivel2(Guid menuItemNivel1Id, string descricao, string? icon, string? to, int ordem, string? modulo, string? capacidadeRequerida = null)
        {
            if (menuItemNivel1Id == Guid.Empty)
                AddNotification(nameof(MenuItemNivel1Id), "O ID do item de nível 1 é obrigatório.");
            if (string.IsNullOrWhiteSpace(descricao))
                AddNotification(nameof(Descricao), "A descrição é obrigatória.");

            Id = Guid.NewGuid();
            MenuItemNivel1Id = menuItemNivel1Id;
            Descricao = descricao;
            Icon = icon;
            To = to;
            Ordem = ordem;
            Modulo = modulo;
            CapacidadeRequerida = string.IsNullOrWhiteSpace(capacidadeRequerida) ? null : capacidadeRequerida.Trim().ToLowerInvariant();
        }

        public void Alterar(string descricao, string? icon, string? to, int ordem, string? modulo, string? capacidadeRequerida = null)
        {
            Clear();
            if (string.IsNullOrWhiteSpace(descricao))
                AddNotification(nameof(Descricao), "A descrição é obrigatória.");
            if (!IsValid) return;

            Descricao = descricao;
            Icon = icon;
            To = to;
            Ordem = ordem;
            Modulo = modulo;
            CapacidadeRequerida = string.IsNullOrWhiteSpace(capacidadeRequerida) ? null : capacidadeRequerida.Trim().ToLowerInvariant();
        }
    }
}
