using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record CriarPessoaGrupoCommand(
        string Descricao
    ) : ICommand;
}
