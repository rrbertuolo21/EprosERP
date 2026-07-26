using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Epros.Infrastructure.Data
{
    /// <summary>
    /// Habilita leitura cross-tenant em endpoints públicos de autenticação (login, registro, recuperação).
    /// Requer política RLS <c>auth_cross_tenant_select</c> nas tabelas afetadas.
    /// </summary>
    public static class AuthRlsBypass
    {
        public const string SessionKey = "app.allow_cross_tenant_auth";

        public static Task EnableAsync(DbContext context, CancellationToken cancellationToken = default)
            => context.Database.ExecuteSqlRawAsync(
                $"SELECT set_config('{SessionKey}', 'true', true);",
                cancellationToken);

        public static Task DisableAsync(DbContext context, CancellationToken cancellationToken = default)
            => context.Database.ExecuteSqlRawAsync(
                $"SELECT set_config('{SessionKey}', 'false', true);",
                cancellationToken);
    }
}
