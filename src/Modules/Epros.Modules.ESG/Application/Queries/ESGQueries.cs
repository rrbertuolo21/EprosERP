using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.ESG.Application.Queries
{
    public record ObterEmissoesQuery() : IQuery<CommandResult>;

    public record ObterRelatoriosESGQuery() : IQuery<CommandResult>;
}
