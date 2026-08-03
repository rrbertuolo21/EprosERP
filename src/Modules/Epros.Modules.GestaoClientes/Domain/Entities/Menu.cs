using System;
using System.Collections.Generic;
using Flunt.Notifications;

namespace Epros.Modules.GestaoClientes.Domain.Entities
{
    // Catálogo GLOBAL do sistema (estrutura de menu compartilhada por todos os tenants; a
    // personalização por tenant é feita via PerfilAcesso, não neste catálogo). Marcado IGlobalEntity
    // como classificação explícita de fronteira de tenant (REG-001 / barreira de entidade órfã).
    public class Menu : Notifiable<Notification>, Epros.Shared.Domain.Entities.IGlobalEntity
    {
        public Guid Id { get; private set; }
        public string Descricao { get; private set; } = string.Empty;
        public string? Icon { get; private set; }
        public string? To { get; private set; }
        public int Ordem { get; private set; }
        public string? Modulo { get; private set; }
        // 1.10 (PERMISSOES_DE_MENU) — capacidade RBAC exigida para VER este item ("recurso:acao", o MESMO
        // par cobrado pelos [AbacAuthorize]/AbacFilter). Nulo = item estrutural sem gate próprio (só aparece
        // se tiver filho visível). É o elo que faz o menu ser PROJEÇÃO das capacidades efetivas (LC-1/LC-2).
        public string? CapacidadeRequerida { get; private set; }
        public List<MenuItemNivel1> ItensNivel1 { get; private set; } = new();

        protected Menu() { } // EF Core

        public Menu(string descricao, string? icon, string? to, int ordem, string? modulo, string? capacidadeRequerida = null)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                AddNotification(nameof(Descricao), "A descrição é obrigatória.");

            Id = Guid.NewGuid();
            Descricao = descricao;
            Icon = icon;
            To = to;
            Ordem = ordem;
            Modulo = modulo;
            CapacidadeRequerida = NormalizarCapacidade(capacidadeRequerida);
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
            CapacidadeRequerida = NormalizarCapacidade(capacidadeRequerida);
        }

        // Capacidade é comparada em minúsculas (mesma convenção de CapacidadeCatalogoSeeder.NomeCapacidade).
        private static string? NormalizarCapacidade(string? cap)
            => string.IsNullOrWhiteSpace(cap) ? null : cap.Trim().ToLowerInvariant();
    }
}
