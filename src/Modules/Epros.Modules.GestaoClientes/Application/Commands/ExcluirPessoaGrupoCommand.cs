using System;
using Epros.Shared.Application.Contracts;

namespace Epros.Modules.GestaoClientes.Application.Commands
{
    public record ExcluirPessoaGrupoCommand(Guid Id) : ICommand;
}
