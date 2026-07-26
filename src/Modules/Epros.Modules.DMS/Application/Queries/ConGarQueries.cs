using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.DMS.Application.Queries
{
    public record ObterPlanosGarantiaQuery() : IQuery<CommandResult>;

    public record ObterVeiculosGarantiaQuery() : IQuery<CommandResult>;

    public record ObterSolicitacoesGarantiaQuery() : IQuery<CommandResult>;
}
