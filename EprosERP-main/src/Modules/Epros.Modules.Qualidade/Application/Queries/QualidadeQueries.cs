using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Qualidade.Application.Queries
{
    public record ObterInspecoesPendentesQuery() : IQuery<CommandResult>;

    public record ObterNaoConformidadesAtivasQuery() : IQuery<CommandResult>;
}
