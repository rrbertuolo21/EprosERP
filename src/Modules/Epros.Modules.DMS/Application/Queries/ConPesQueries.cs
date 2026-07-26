using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.DMS.Application.Queries
{
    public record ObterPecasReposicaoQuery() : IQuery<CommandResult>;

    public record ObterDemandasPecaQuery() : IQuery<CommandResult>;

    public record ObterReservasPecaQuery() : IQuery<CommandResult>;
}
