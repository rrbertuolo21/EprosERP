using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.DMS.Application.Queries
{
    public record ObterRedeNosQuery() : IQuery<CommandResult>;

    public record ObterContratosRedeQuery() : IQuery<CommandResult>;
}
