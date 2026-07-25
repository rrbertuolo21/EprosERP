using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Epros.Modules.Manutencao.Infrastructure.Data
{
    /// <summary>
    /// Fabrica de design-time usada SOMENTE pelas ferramentas de migracao (dotnet ef).
    /// Permite gerar migrations deste Context de forma isolada, sem depender do startup da API.
    /// Nao e registrada em runtime.
    /// </summary>
    public class ContextManutencaoDesignTimeFactory : IDesignTimeDbContextFactory<ContextManutencao>
    {
        public ContextManutencao CreateDbContext(string[] args)
        {
            var conn = System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Database=epros_design;Username=postgres;Password=postgres";
            var options = new DbContextOptionsBuilder<ContextManutencao>()
                .UseNpgsql(conn)
                .Options;

            return new ContextManutencao(options, new DesignTenantProvider(), new DesignCurrentUser());
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
