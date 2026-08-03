using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.ESG.Application.Queries
{
    // ESG-DIV (Diversidade e Responsabilidade Social)
    public record ListarProgramasDivQuery() : IQuery<CommandResult>;
    public record ListarIndicadoresDivQuery() : IQuery<CommandResult>;
    public record ListarMedicoesDivQuery(Guid IndicadorId) : IQuery<CommandResult>;
}
