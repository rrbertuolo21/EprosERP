using System;
using System.Collections.Generic;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Aplicativo.Application.Commands
{
    public record AcessoInputDto(
        Guid MenuId,
        Guid? MenuItemNivel1Id,
        Guid? MenuItemNivel2Id,
        bool Ver,
        bool Editar,
        bool Excluir
    );

    public record CriarPerfilAcessoCommand(
        string Descricao,
        List<AcessoInputDto> Acessos
    ) : ICommand;

    public record AtualizarPerfilAcessoCommand(
        Guid Id,
        string Descricao,
        List<AcessoInputDto> Acessos
    ) : ICommand;

    public record DeletarPerfilAcessoCommand(
        Guid Id
    ) : ICommand;
}
