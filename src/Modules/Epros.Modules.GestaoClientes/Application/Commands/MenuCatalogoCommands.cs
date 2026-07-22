using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    // ===== Menu principal =====
    public record CriarMenuCommand(string Descricao, string? Icon, string? To, int Ordem, string? Modulo) : ICommand;
    public record AtualizarMenuCommand(Guid Id, string Descricao, string? Icon, string? To, int Ordem, string? Modulo) : ICommand;
    public record DeletarMenuCommand(Guid Id) : ICommand;

    // ===== Item de nível 1 =====
    public record CriarMenuItemNivel1Command(Guid MenuId, string Descricao, string? Icon, string? To, int Ordem, string? Modulo) : ICommand;
    public record AtualizarMenuItemNivel1Command(Guid Id, string Descricao, string? Icon, string? To, int Ordem, string? Modulo) : ICommand;
    public record DeletarMenuItemNivel1Command(Guid Id) : ICommand;

    // ===== Item de nível 2 =====
    public record CriarMenuItemNivel2Command(Guid MenuItemNivel1Id, string Descricao, string? Icon, string? To, int Ordem, string? Modulo) : ICommand;
    public record AtualizarMenuItemNivel2Command(Guid Id, string Descricao, string? Icon, string? To, int Ordem, string? Modulo) : ICommand;
    public record DeletarMenuItemNivel2Command(Guid Id) : ICommand;
}
