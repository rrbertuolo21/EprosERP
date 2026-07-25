using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Epros.Modules.Vendas.Infrastructure.Data
{
    /// <summary>
    /// Fábrica de design-time para gerar/aplicar migrations do ContextVendas isoladamente (build apenas do
    /// módulo, sem a solução inteira) — necessário no MODO PORTE paralelo. Não é usada em runtime; a app real
    /// registra o contexto por DI com os provedores reais de tenant/usuário.
    /// </summary>
    public class ContextVendasDesignTimeFactory : IDesignTimeDbContextFactory<ContextVendas>
    {
        public ContextVendas CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>()
                .UseNpgsql("Host=localhost;Database=epros_design;Username=postgres;Password=postgres")
                .Options;

            return new ContextVendas(options, new DesignTenantProvider(), new DesignCurrentUser());
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
