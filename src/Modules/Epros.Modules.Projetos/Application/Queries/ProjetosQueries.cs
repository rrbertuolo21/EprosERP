using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Projetos.Application.Queries
{
    public record ObterProjetosQuery() : IQuery<CommandResult>;

    public record ObterProjetoPorIdQuery(Guid Id) : IQuery<CommandResult>;
}
