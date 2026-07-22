using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    public record CriarProdutoGrupoCommand(string Descricao) : ICommand;

    public record AtualizarProdutoGrupoCommand(Guid Id, string Descricao) : ICommand;

    public record DeletarProdutoGrupoCommand(Guid Id) : ICommand;
}
