using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.RH.Application.Queries
{
    public record ObterColaboradoresQuery() : IQuery<CommandResult>;

    public record ObterFolhasPagamentoQuery() : IQuery<CommandResult>;
}
