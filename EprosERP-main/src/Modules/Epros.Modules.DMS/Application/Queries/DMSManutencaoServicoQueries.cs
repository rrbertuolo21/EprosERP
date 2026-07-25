using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.DMS.Application.Queries
{
    public record ObterTiposServicoConcessionariaQuery() : IQuery<CommandResult>;

    public record ObterOperacoesServicoQuery() : IQuery<CommandResult>;

    public record ObterPacotesServicoQuery() : IQuery<CommandResult>;

    public record ObterOrdensServicoManutencaoQuery() : IQuery<CommandResult>;

    public record ObterOrcamentosManutencaoQuery() : IQuery<CommandResult>;
}
