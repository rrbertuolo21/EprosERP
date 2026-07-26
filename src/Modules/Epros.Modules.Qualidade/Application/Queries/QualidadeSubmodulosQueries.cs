using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Qualidade.Application.Queries
{
    // QLD-NCR
    public record ListarNcrsQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    // QLD-INS
    public record ListarPlanosInspecaoQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    // QLD-ACR
    public record ListarAnalisesAcrQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    // QLD-ADM
    public record ListarRegistrosAdmQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;

    // QLD-ATR
    public record ListarAtributosQuery(string? Status, int Pagina = 1, int TamanhoPagina = 20) : IQuery<CommandResult>;
}
