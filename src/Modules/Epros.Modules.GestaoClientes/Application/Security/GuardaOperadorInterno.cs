using System;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Modules.GestaoClientes.Application.Security
{
    /// <summary>
    /// 1.11 (OPERACAO_SUPER_ADMIN) — Defense-in-depth uniforme para operações LANDLORD (governança
    /// de tenants, planos, faturas, gateways). Espelha o guard que o módulo Aplicativo já aplica
    /// (<c>tenant == "system"</c>): a operação da Siser sobre tenants exige um operador interno.
    ///
    /// O <c>AbacFilter</c> (fix #1) já barra o escalonamento no ingresso; este guard fecha o handler
    /// mesmo que o filtro seja contornado (ex.: envio direto de comando/fila), fail-closed.
    /// </summary>
    public static class GuardaOperadorInterno
    {
        public const string TenantSistema = "system";

        /// <summary>True se o contexto corrente é o do operador interno da Siser (tenant "system").</summary>
        public static bool EhOperadorInterno(ITenantProvider tenantProvider)
            => string.Equals(tenantProvider?.GetTenantId(), TenantSistema, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Retorna <c>null</c> se autorizado (operador interno); caso contrário um <see cref="CommandResult"/>
        /// de falha padronizado. Uso: <c>var g = GuardaOperadorInterno.Exigir(_tenantProvider); if (g != null) return g;</c>
        /// </summary>
        public static CommandResult? Exigir(ITenantProvider tenantProvider)
            => EhOperadorInterno(tenantProvider)
                ? null
                : CommandResult.Falha(new[] { "Acesso Proibido: operação restrita ao operador interno da Siser (landlord)." });
    }
}
