using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Manutencao.Application.Queries
{
    public record ObterEquipamentosQuery() : IQuery<CommandResult>;

    public record ObterOrdensManutencaoQuery() : IQuery<CommandResult>;
}
