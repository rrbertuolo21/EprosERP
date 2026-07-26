using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.DMS.Application.Queries
{
    public record ObterProspectsShowroomQuery() : IQuery<CommandResult>;

    public record ObterOportunidadesConcessionariaQuery() : IQuery<CommandResult>;

    public record ObterTestDrivesQuery() : IQuery<CommandResult>;
}
