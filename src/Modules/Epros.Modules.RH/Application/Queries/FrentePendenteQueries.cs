using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.RH.Application.Queries
{
    // RH-DEV
    public record ListarPromocoesQuery() : IQuery<CommandResult>;
    public record ListarAdvertenciasQuery() : IQuery<CommandResult>;

    // RH-LMS
    public record ListarTreinamentosQuery() : IQuery<CommandResult>;
    public record ListarCertificacoesQuery() : IQuery<CommandResult>;

    // RH-PLN
    public record ListarTurnosQuery() : IQuery<CommandResult>;
    public record ListarFeriadosQuery() : IQuery<CommandResult>;
    public record ListarHeadcountQuery() : IQuery<CommandResult>;

    // RH-TLT
    public record ListarMetasColaboradorQuery() : IQuery<CommandResult>;
    public record ListarSolicitacoesLicencaQuery() : IQuery<CommandResult>;

    // RH-WFM
    public record ListarWfmColaboradoresQuery() : IQuery<CommandResult>;
}
