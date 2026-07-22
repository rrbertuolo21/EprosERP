using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.DMS.Application.Queries
{
    public record ObterVendasVeiculosQuery() : IQuery<CommandResult>;

    public record ObterOrdensServicoDmsQuery() : IQuery<CommandResult>;

    public record ObterGarantiasMontadoraQuery() : IQuery<CommandResult>;
}
