using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Epros.Modules.Agricultor.Infrastructure.Data
{
    /// <summary>
    /// Fábrica de design-time usada SOMENTE pelas ferramentas de migração (dotnet ef). Não é registrada
    /// em runtime. Espelha ContextImobiliariaDesignTimeFactory.
    /// </summary>
    public class ContextAgricultorDesignTimeFactory : IDesignTimeDbContextFactory<ContextAgricultor>
    {
        public ContextAgricultor CreateDbContext(string[] args)
        {
            var conn = System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Database=epros_design;Username=postgres;Password=postgres";
            var options = new DbContextOptionsBuilder<ContextAgricultor>()
                .UseNpgsql(conn)
                .Options;

            return new ContextAgricultor(options, new DesignTenantProvider(), new DesignCurrentUser());
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
