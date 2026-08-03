using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Aplicativo.Application.Handlers
{
    /// <summary>
    /// REG-016 — restauração GOVERNADA de soft-delete das entidades-chave do control-plane
    /// (Cliente, Plano, Cupom — no <see cref="ContextGestaoClientes"/> — e Usuario — no
    /// <see cref="ContextAplicativo"/>). Usa <c>IgnoreQueryFilters</c> para enxergar a linha deletada,
    /// mas RE-APLICA o filtro de tenant à mão (o IgnoreQueryFilters derruba tenant + soft-delete juntos),
    /// preservando o isolamento (entidades <see cref="IGlobalEntity"/> não filtram por tenant).
    /// Idempotente: se a linha já está ativa, é no-op; se não existe (ou é de outro tenant), falha.
    /// A auditoria de quem restaurou fica em AlteradoPor/AlteradoEm via <c>Restaurar</c>.
    /// </summary>
    public class RestaurarEntidadeCommandHandler : ICommandHandler<RestaurarEntidadeCommand>
    {
        private readonly ContextGestaoClientes _ctxGestao;
        private readonly ContextAplicativo _ctxApp;
        private readonly ITenantProvider _tenantProvider;
        private readonly ICurrentUser _currentUser;

        public RestaurarEntidadeCommandHandler(
            ContextGestaoClientes ctxGestao,
            ContextAplicativo ctxApp,
            ITenantProvider tenantProvider,
            ICurrentUser currentUser)
        {
            _ctxGestao = ctxGestao;
            _ctxApp = ctxApp;
            _tenantProvider = tenantProvider;
            _currentUser = currentUser;
        }

        public Task<CommandResult> Handle(RestaurarEntidadeCommand request, CancellationToken cancellationToken)
        {
            var tenantId = _tenantProvider.GetTenantId();
            var restauradoPor = _currentUser.GetUserId() ?? "operador-interno";

            return request.Tipo switch
            {
                EntidadeRestauravel.Cliente => RestaurarAsync<Cliente>(_ctxGestao, request.Id, tenantId, restauradoPor, cancellationToken),
                EntidadeRestauravel.Plano => RestaurarAsync<Plano>(_ctxGestao, request.Id, tenantId, restauradoPor, cancellationToken),
                EntidadeRestauravel.Cupom => RestaurarAsync<Cupom>(_ctxGestao, request.Id, tenantId, restauradoPor, cancellationToken),
                EntidadeRestauravel.Usuario => RestaurarAsync<Usuario>(_ctxApp, request.Id, tenantId, restauradoPor, cancellationToken),
                _ => Task.FromResult(CommandResult.Falha("Tipo de entidade não suportado para restauração."))
            };
        }

        private static async Task<CommandResult> RestaurarAsync<T>(
            DbContext ctx, Guid id, string tenantId, string restauradoPor, CancellationToken ct)
            where T : EntidadeSaaSBase
        {
            var query = ctx.Set<T>().IgnoreQueryFilters().Where(e => e.Id == id);

            // Respeita o tenant: entidades não-globais só podem ser restauradas dentro do próprio tenant.
            if (!typeof(IGlobalEntity).IsAssignableFrom(typeof(T)))
                query = query.Where(e => e.TenantId == tenantId);

            var entidade = await query.FirstOrDefaultAsync(ct);
            if (entidade == null)
                return CommandResult.Falha($"{typeof(T).Name} não encontrado(a) neste tenant.");

            // Idempotência: só restaura o que está soft-deleted; já-ativo é no-op de sucesso.
            if (entidade.EstaAtivo())
                return CommandResult.Ok($"{typeof(T).Name} já estava ativo(a); nada a restaurar.",
                    new { entidade.Id, Restaurada = false });

            entidade.Restaurar(restauradoPor);
            await ctx.SaveChangesAsync(ct);

            return CommandResult.Ok($"{typeof(T).Name} restaurado(a) com sucesso.",
                new { entidade.Id, Restaurada = true });
        }
    }
}
