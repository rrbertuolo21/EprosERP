using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.ESG.Application.Queries
{
    // ESG-GHG
    public record ListarInventariosGeeQuery() : IQuery<CommandResult>;
    public record ListarFatoresEmissaoGeeQuery() : IQuery<CommandResult>;
    // ESG-EHS
    public record ListarRegistrosEhsQuery() : IQuery<CommandResult>;
    public record ListarIncidentesQuery() : IQuery<CommandResult>;
    public record ListarLicencasAmbientaisQuery() : IQuery<CommandResult>;
    // ESG-REL
    public record ListarFrameworksRelQuery() : IQuery<CommandResult>;
    public record ListarItensRelatorioQuery(System.Guid RelatorioId) : IQuery<CommandResult>;
    // ESG-ECO
    public record ListarDevolucoesEcoQuery() : IQuery<CommandResult>;
    public record ListarFluxosCircularesQuery() : IQuery<CommandResult>;
}
