using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record AtualizarPessoaGrupoCommand(
        Guid Id,
        string Descricao
    ) : ICommand;
}
