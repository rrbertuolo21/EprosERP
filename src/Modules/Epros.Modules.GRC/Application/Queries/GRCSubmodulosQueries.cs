using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GRC.Application.Queries
{
    // GRC-POL
    public record ObterPoliticasQuery() : IQuery<CommandResult>;
    public record ObterAceitesPoliticaQuery(Guid PoliticaId) : IQuery<CommandResult>;

    // GRC-REG
    public record ObterRegistrosRegulatoriosQuery() : IQuery<CommandResult>;
    public record ObterCertificadosDigitaisQuery() : IQuery<CommandResult>;

    // GRC-RIS
    public record ObterAvaliacoesRiscoQuery(Guid RiscoId) : IQuery<CommandResult>;

    // GRC-CIA
    public record ObterPlanosAuditoriaQuery() : IQuery<CommandResult>;
    public record ObterAchadosQuery() : IQuery<CommandResult>;

    // GRC-SOD
    public record ObterRegrasSoDQuery() : IQuery<CommandResult>;
    public record ObterViolacoesSoDQuery() : IQuery<CommandResult>;
}
