using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.ESG.Application.Queries
{
    // ESG-TSU (Transporte Sustentavel)
    public record ListarRegistrosTsuQuery() : IQuery<CommandResult>;
    public record ListarCalculosTsuQuery(Guid TrechoId) : IQuery<CommandResult>;
    public record ListarMetasModalTsuQuery(Guid RegistroTsuId) : IQuery<CommandResult>;
}
