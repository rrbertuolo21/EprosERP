using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.DMS.Application.Queries
{
    public record ObterJornadasFinQuery() : IQuery<CommandResult>;

    public record ObterSimulacoesFinQuery() : IQuery<CommandResult>;

    public record ObterContratosFinQuery() : IQuery<CommandResult>;
}
