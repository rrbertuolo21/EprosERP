using System;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace Epros.Modules.Estoque.Application.Security
{
    /// <summary>
    /// EST-PFO / PFO-002 — Enforcement de isolamento por fornecedor do Portal do Fornecedor.
    ///
    /// Regra de segurança (fecha o vazamento cross-fornecedor P0 / VAL-PFO-004/005): quando o principal
    /// autenticado é um usuário EXTERNO do portal (traz a claim "fornecedorId"), o fornecedorId efetivo é
    /// SEMPRE o do sujeito autenticado — nunca o parâmetro do request. Qualquer tentativa de agir/consultar
    /// como outro fornecedor é negada. Exige ainda acesso ATIVO (§15.2 pfo_usuario_fornecedor).
    ///
    /// Quando o principal NÃO traz o vínculo externo, trata-se de operador interno (comprador/backoffice),
    /// já autorizado pelo ABAC — o request é preservado (publicar cotação, convidar, listar convites).
    ///
    /// valida-ambiente: wiring de login externo do portal (SSO/token) que emite a claim fornecedorId.
    /// </summary>
    public static class PortalFornecedorAcesso
    {
        /// <summary>
        /// Resolve o fornecedorId efetivo a partir do principal. Retorna (erro, fornecedorId).
        /// Se Erro != null, o chamador deve negar (não confiar no request).
        /// </summary>
        public static async Task<(CommandResult? Erro, Guid FornecedorId)> ResolverAsync(
            ICurrentUser currentUser,
            ContextEstoque context,
            string tenantId,
            Guid fornecedorIdRequest,
            CancellationToken cancellationToken)
        {
            var claim = currentUser.GetFornecedorId();

            // Sem vínculo externo → operador interno (ABAC já autorizou). Preserva o request.
            if (!claim.HasValue)
                return (null, fornecedorIdRequest);

            // Vínculo externo presente: o fornecedor é SEMPRE o do principal (nunca o request).
            if (fornecedorIdRequest != Guid.Empty && fornecedorIdRequest != claim.Value)
                return (CommandResult.Falha("Acesso negado: fornecedor distinto do vínculo do portal [PFO-002/VAL-PFO-004]."), Guid.Empty);

            // §15.2: exige acesso ativo do fornecedor no portal.
            var acessoAtivo = await context.PfoUsuariosFornecedor.AsNoTracking()
                .AnyAsync(u => u.TenantId == tenantId && u.FornecedorId == claim.Value && u.Ativo && u.DeletadoEm == null, cancellationToken);
            if (!acessoAtivo)
                return (CommandResult.Falha("Acesso negado: fornecedor sem acesso ativo no portal [PFO-002]."), Guid.Empty);

            return (null, claim.Value);
        }
    }
}
