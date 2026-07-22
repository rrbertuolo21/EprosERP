using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.Aplicativo.Application.Queries
{
    /// <summary>
    /// Recupera o estado da sessão de um usuário já autenticado (token válido resolvido no gateway).
    /// Substitui a lógica que vivia inline no <c>AccountController.Session</c> (violação Q5:
    /// controller injetava DbContext e a regra de inadimplência SaaS). Toda a leitura de
    /// usuário/empresas/bloqueio passa a residir neste handler MediatR.
    /// </summary>
    public record RecuperarSessaoQuery(
        string TenantId,
        Guid UsuarioId,
        string Token
    ) : IQuery<CommandResult>;
}
