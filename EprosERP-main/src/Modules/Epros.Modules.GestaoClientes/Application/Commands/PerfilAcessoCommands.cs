using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    // Item de acesso (permissão de menu) usado nos comandos de PerfilAcesso.
    public record PerfilAcessoMenuInput(
        Guid MenuId,
        Guid? MenuItemNivel1Id,
        Guid? MenuItemNivel2Id,
        bool Ver,
        bool Editar,
        bool Excluir
    );

    public record CriarPerfilAcessoCommand(
        string Descricao,
        List<PerfilAcessoMenuInput>? Acessos
    ) : ICommand;

    public record AtualizarPerfilAcessoCommand(
        Guid Id,
        string Descricao,
        List<PerfilAcessoMenuInput>? Acessos
    ) : ICommand;

    public record DeletarPerfilAcessoCommand(Guid Id) : ICommand;
}
