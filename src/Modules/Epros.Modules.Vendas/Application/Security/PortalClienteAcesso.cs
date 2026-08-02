using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Vendas.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Vendas.Application.Security
{
    /// <summary>
    /// VEN-PCL / T-02 — Enforcement de isolamento usuário↔cliente do Portal do Cliente.
    ///
    /// Regra de segurança (fecha o vazamento cross-cliente P0): quando o principal autenticado é um
    /// usuário EXTERNO do portal (traz a claim "clienteId"), o clienteId efetivo é SEMPRE o do sujeito
    /// autenticado — nunca o parâmetro do request. Qualquer tentativa de referenciar outro cliente é
    /// negada. Além disso, quando a claim "portalUsuarioId" existe, o usuário externo é carregado e
    /// PodeAcessar()/vínculo são exigidos (§18) — tornando esse código, antes morto, efetivo.
    ///
    /// Quando o principal NÃO traz o vínculo externo, trata-se de operador interno (backoffice), já
    /// autorizado pelo ABAC; o clienteId do request é preservado (o filtro por cliente/tenant continua).
    ///
    /// valida-ambiente: wiring de login externo do portal (SSO/token) que emite as claims
    /// clienteId/portalUsuarioId. A checagem abaixo assume essas claims no principal.
    /// </summary>
    public static class PortalClienteAcesso
    {
        /// <summary>
        /// Resolve o clienteId efetivo a partir do principal. Retorna (erro, clienteId).
        /// Se Erro != null, o chamador deve negar (não confiar no request).
        /// </summary>
        public static async Task<(CommandResult? Erro, Guid? ClienteId)> ResolverAsync(
            ICurrentUser currentUser,
            ContextVendas context,
            string tenantId,
            Guid? clienteIdRequest,
            CancellationToken cancellationToken)
        {
            var clienteClaim = currentUser.GetClienteId();

            // Sem vínculo externo → operador interno (ABAC já autorizou). Preserva o request.
            if (!clienteClaim.HasValue)
                return (null, clienteIdRequest);

            // Vínculo externo presente: o cliente é SEMPRE o do principal (nunca o request).
            if (clienteIdRequest.HasValue && clienteIdRequest.Value != clienteClaim.Value)
                return (CommandResult.Falha("Acesso negado: cliente distinto do vínculo do portal [VEN-PCL/T-02]."), null);

            // §18: valida o usuário externo (ativo + vinculado) quando o principal o identifica.
            var erroUsuario = await ValidarUsuarioExternoAsync(currentUser, context, tenantId, clienteClaim.Value, cancellationToken);
            if (erroUsuario != null) return (erroUsuario, null);

            return (null, clienteClaim.Value);
        }

        /// <summary>
        /// Garante que o recurso referenciado (por seu clienteId) pertence ao cliente do principal externo.
        /// Para operador interno (sem vínculo), não restringe (ABAC + filtro por tenant já valem).
        /// </summary>
        public static async Task<CommandResult?> GarantirClienteDoRecursoAsync(
            ICurrentUser currentUser,
            ContextVendas context,
            string tenantId,
            Guid? clienteIdDoRecurso,
            CancellationToken cancellationToken)
        {
            var clienteClaim = currentUser.GetClienteId();
            if (!clienteClaim.HasValue) return null; // interno

            if (clienteIdDoRecurso != clienteClaim.Value)
                return CommandResult.Falha("Acesso negado: recurso de outro cliente [VEN-PCL/T-02].");

            return await ValidarUsuarioExternoAsync(currentUser, context, tenantId, clienteClaim.Value, cancellationToken);
        }

        /// <summary>
        /// Nega o acesso quando o principal é um usuário externo do portal. Usado em superfícies que são
        /// exclusivamente de backoffice interno (ex.: catálogo de formulários), onde não há vínculo de cliente.
        /// </summary>
        public static CommandResult? SomenteInterno(ICurrentUser currentUser)
        {
            return currentUser.GetClienteId().HasValue
                ? CommandResult.Falha("Acesso negado: recurso restrito a operadores internos [VEN-PCL].")
                : null;
        }

        private static async Task<CommandResult?> ValidarUsuarioExternoAsync(
            ICurrentUser currentUser,
            ContextVendas context,
            string tenantId,
            Guid clienteId,
            CancellationToken cancellationToken)
        {
            var portalUsuarioId = currentUser.GetPortalUsuarioId();
            if (!portalUsuarioId.HasValue) return null; // login externo pode trazer só clienteId

            var usuario = await context.PortalUsuariosCliente.AsNoTracking()
                .FirstOrDefaultAsync(u => u.TenantId == tenantId && u.Id == portalUsuarioId.Value, cancellationToken);

            if (usuario == null || usuario.ClienteId != clienteId)
                return CommandResult.Falha("Acesso negado: usuário do portal não vinculado ao cliente [VEN-PCL §18].");

            // §18: código antes morto — só usuário ativo acessa.
            if (!usuario.PodeAcessar())
                return CommandResult.Falha("Acesso negado: usuário do portal inativo ou bloqueado [VEN-PCL §18].");

            return null;
        }
    }
}
