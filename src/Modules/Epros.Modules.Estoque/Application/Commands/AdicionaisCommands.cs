using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.Estoque.Application.Commands
{
    public record CriarAdicionaisCommand(string Descricao, decimal ValorPreco) : ICommand;

    public record AtualizarAdicionaisCommand(Guid Id, string Descricao, decimal ValorPreco) : ICommand;

    public record DeletarAdicionaisCommand(Guid Id) : ICommand;
}
