using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    public record ListarPerfisAcessoQuery(
        int Pagina = 1,
        int TamanhoPagina = 20
    ) : IQuery<CommandResult>;

    public record ObterPerfilAcessoPorIdQuery(
        Guid Id
    ) : IQuery<CommandResult>;

    public record ObterArvoreCompletaMenusQuery : IQuery<CommandResult>;

    public record ListarMenusQuery(
        int Pagina = 1,
        int TamanhoPagina = 25
    ) : IQuery<CommandResult>;

    public record ObterMenuPorIdQuery(
        Guid Id
    ) : IQuery<CommandResult>;

    public record ObterAcessosSessaoQuery(
        Guid UsuarioId,
        Guid EmpresaId
    ) : IQuery<CommandResult>;
}
