using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    // ===== Funcionalidade (catálogo global) =====
    public record CriarFuncionalidadeCommand(string Title, string Description) : ICommand;
    public record AtualizarFuncionalidadeCommand(Guid Id, string Title, string Description) : ICommand;
    public record DeletarFuncionalidadeCommand(Guid Id) : ICommand;

    // ===== Add-on / módulo (catálogo modular) =====
    public record CriarAddOnCommand(
        string NomeModulo,
        string? Alias,
        decimal PrecoMensal,
        decimal PrecoAnual,
        string? Midia,
        bool Habilitado,
        bool Admin,
        Guid? ParentAddOnId
    ) : ICommand;

    public record AtualizarAddOnCommand(
        Guid Id,
        string? Alias,
        decimal PrecoMensal,
        decimal PrecoAnual,
        string? Midia,
        Guid? ParentAddOnId
    ) : ICommand;

    public record HabilitarAddOnCommand(Guid Id) : ICommand;
    public record DesabilitarAddOnCommand(Guid Id) : ICommand;

    // ===== Módulo ativo por usuário =====
    public record AtivarModuloUsuarioCommand(Guid UsuarioId, string Modulo) : ICommand;
}
