using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarPessoaGruposQuery() : IQuery<CommandResult>;

    public record ObterPessoaGrupoPorIdQuery(Guid Id) : IQuery<CommandResult>;
}
