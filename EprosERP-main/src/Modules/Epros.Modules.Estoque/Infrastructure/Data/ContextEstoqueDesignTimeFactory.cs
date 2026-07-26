using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Epros.Modules.Estoque.Infrastructure.Data
{
    /// <summary>
    /// Fábrica de design-time para gerar/aplicar migrations do ContextEstoque isoladamente (build apenas do
    /// módulo, sem a solução inteira) — necessário no MODO PORTE paralelo. Não é usada em runtime; a app real
    /// registra o contexto por DI com os provedores reais de tenant/usuário.
    /// </summary>
    public class ContextEstoqueDesignTimeFactory : IDesignTimeDbContextFactory<ContextEstoque>
    {
        public ContextEstoque CreateDbContext(string[] args)
        {
            var conn = System.Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Host=localhost;Database=epros_design;Username=postgres;Password=postgres";
            var options = new DbContextOptionsBuilder<ContextEstoque>()
                .UseNpgsql(conn)
                .Options;

            return new ContextEstoque(options, new DesignTenantProvider(), new DesignCurrentUser());
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
