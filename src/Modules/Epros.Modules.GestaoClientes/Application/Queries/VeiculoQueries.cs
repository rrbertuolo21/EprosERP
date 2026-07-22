using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Queries
{
    public record ListarVeiculosQuery(Guid? PessoaId = null, int Pagina = 1, int TamanhoPagina = 25) : IQuery<CommandResult>;

    public record ObterVeiculoPorIdQuery(Guid Id) : IQuery<CommandResult>;
}
