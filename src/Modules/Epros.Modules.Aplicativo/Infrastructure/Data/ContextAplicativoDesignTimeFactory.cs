using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Epros.Modules.Aplicativo.Infrastructure.Data
{
    /// <summary>
    /// Fabrica de design-time usada SOMENTE pelas ferramentas de migracao (dotnet ef) e utilitarios
    /// de geracao de schema. Permite instanciar o ContextAplicativo de forma isolada, sem depender do
    /// startup da API. Nao e registrada em runtime.
    ///
    /// A AUSENCIA desta fabrica foi a raiz do drift do modulo: entidades (wf_*, upl_*, super-admin)
    /// entraram no modelo/snapshot mas suas migrations CreateTable nunca foram geradas de forma isolada.
    /// </summary>
    public class ContextAplicativoDesignTimeFactory : IDesignTimeDbContextFactory<ContextAplicativo>
    {
        public ContextAplicativo CreateDbContext(string[] args)
        {
            var conn = System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Database=epros_design;Username=postgres;Password=postgres";
            var options = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseNpgsql(conn)
                .Options;

            return new ContextAplicativo(options, new DesignTenantProvider(), new DesignCurrentUser());
        }

        private sealed class DesignTenantProvider : ITenantProvider
        {
            public string GetTenantId() => "design-time";
        }

        private sealed class DesignCurrentUser : ICurrentUser
        {
            public string? GetUserId() => "design-time";
            public string? GetUserName() => "design-time";
            public string? GetUserEmail() => "design-time@epros.local";
        }
    }
}
