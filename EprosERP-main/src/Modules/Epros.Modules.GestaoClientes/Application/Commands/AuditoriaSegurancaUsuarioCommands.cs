using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    // APP-TEN-003 (Usuarios e Papeis) — grant/deny direto de capacidade.
    // HistoricoLogin e SessaoImpersonacao pertencem ao módulo Aplicativo (Identity); ver Epros.Modules.Aplicativo.

    // ===== Grant/deny direto de capacidade (usuario_capacidade) =====
    public record DefinirUsuarioCapacidadeCommand(Guid UsuarioId, Guid CapacidadeId, bool Granted) : ICommand;
    public record RemoverUsuarioCapacidadeCommand(Guid Id) : ICommand;
}
