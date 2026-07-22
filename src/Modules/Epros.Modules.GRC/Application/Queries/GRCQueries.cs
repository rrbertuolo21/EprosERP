using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GRC.Application.Queries
{
    public record ObterRiscosQuery() : IQuery<CommandResult>;

    public record ObterDenunciasQuery() : IQuery<CommandResult>;

    public record ObterIncidentesQuery() : IQuery<CommandResult>;
}
