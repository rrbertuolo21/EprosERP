using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarEnderecosQuery(Guid? PessoaId = null, int Pagina = 1, int TamanhoPagina = 25) : IQuery<CommandResult>;

    public record ObterEnderecoPorIdQuery(Guid Id) : IQuery<CommandResult>;
}
