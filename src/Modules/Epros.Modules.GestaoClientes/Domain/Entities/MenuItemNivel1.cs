using System;
using System.Collections.Generic;
using Flunt.Notifications;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    // Catálogo GLOBAL (item de menu nível 1). Ver Menu — IGlobalEntity classifica a fronteira (REG-001).
    public class MenuItemNivel1 : Notifiable<Notification>, Epros.Shared.Domain.Entities.IGlobalEntity
    {
        public Guid Id { get; private set; }
        public Guid MenuId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string? Icon { get; private set; }
        public string? To { get; private set; }
        public int Ordem { get; private set; }
        public string? Modulo { get; private set; }
        // 1.10 — capacidade RBAC exigida para ver este item (ver Menu.CapacidadeRequerida).
        public string? CapacidadeRequerida { get; private set; }
        public List<MenuItemNivel2> ItensNivel2 { get; private set; } = new();

        protected MenuItemNivel1() { } // EF Core

        public MenuItemNivel1(Guid menuId, string descricao, string? icon, string? to, int ordem, string? modulo, string? capacidadeRequerida = null)
        {
            if (menuId == Guid.Empty)
                AddNotification(nameof(MenuId), "O ID do menu principal é obrigatório.");
            if (string.IsNullOrWhiteSpace(descricao))
                AddNotification(nameof(Descricao), "A descrição é obrigatória.");

            Id = Guid.NewGuid();
            MenuId = menuId;
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
