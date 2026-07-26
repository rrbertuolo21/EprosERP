using System;
using System.Collections.Generic;
using Flunt.Notifications;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    public class MenuItemNivel1 : Notifiable<Notification>
    {
        public Guid Id { get; private set; }
        public Guid MenuId { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string? Icon { get; private set; }
        public string? To { get; private set; }
        public int Ordem { get; private set; }
        public string? Modulo { get; private set; }
        public List<MenuItemNivel2> ItensNivel2 { get; private set; } = new();

        protected MenuItemNivel1() { } // EF Core

        public MenuItemNivel1(Guid menuId, string descricao, string? icon, string? to, int ordem, string? modulo)
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
        }

        public void Alterar(string descricao, string? icon, string? to, int ordem, string? modulo)
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
        }
    }
}
