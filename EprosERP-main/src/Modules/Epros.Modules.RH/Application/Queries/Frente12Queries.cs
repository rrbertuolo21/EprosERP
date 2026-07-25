using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.RH.Application.Queries
{
    // RH-FP
    public record ListarRubricasQuery() : IQuery<CommandResult>;
    public record ListarCompetenciasQuery() : IQuery<CommandResult>;

    // RH-PNT
    public record ListarMarcacoesQuery() : IQuery<CommandResult>;
    public record ListarPeriodosApuracaoQuery() : IQuery<CommandResult>;

    // RH-SSO
    public record ListarPppsQuery() : IQuery<CommandResult>;

    // RH-REC
    public record ListarVagasQuery() : IQuery<CommandResult>;
    public record ListarCandidatosQuery() : IQuery<CommandResult>;
}
