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

    /// <summary>
    /// 1.10 (PERMISSOES_DE_MENU) — menu VISÍVEL do usuário logado, derivado das CAPACIDADES efetivas do RBAC
    /// (fonte única = ICapacidadesEfetivasService, a MESMA do AbacFilter). Um item aparece sse a sua
    /// CapacidadeRequerida está no conjunto efetivo do usuário na empresa corrente (LC-1/LC-2). Empresa
    /// corrente (1.09) e entitlement de plano (1.06) já entram na filtragem.
    /// </summary>
    public record ObterMenuDoUsuarioQuery(
        Guid UsuarioId,
        Guid EmpresaId
    ) : IQuery<CommandResult>;
}
