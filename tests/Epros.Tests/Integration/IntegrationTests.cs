using System;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests.Integration
{
    public class IntegrationTests : IClassFixture<IntegrationTestFixture>
    {
        private readonly IntegrationTestFixture _fixture;

        public IntegrationTests(IntegrationTestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Deve_Garantir_Isolamento_De_Tenant_No_Banco_Real_Postgres()
        {
            // Se o Docker não estiver rodando no host, pula o teste de forma amigável
            if (!_fixture.DockerDisponivel)
            {
                return;
            }

            // Arrange: Inicializa o schema executando as migrations para aplicar RLS no banco
            using (var initContext = _fixture.CreateDbContext("tenant-sistema", "user-sistema", isAdmin: true))
            {
                await initContext.Database.MigrateAsync();
                await initContext.Database.ExecuteSqlRawAsync(@"
                    DO $$ 
                    DECLARE 
                        r RECORD;
                    BEGIN
                        FOR r IN (SELECT tablename, schemaname FROM pg_tables WHERE schemaname IN ('plataforma', 'aplicativo')) 
                        LOOP
                            EXECUTE 'ALTER TABLE ""' || r.schemaname || '"".""' || r.tablename || '"" FORCE ROW LEVEL SECURITY';
                        END LOOP;
                    END $$;
                ");
            }

            // Cadastra plano e cliente do Tenant A
            using (var contextTenantA = _fixture.CreateDbContext("tenant-a", "user-a"))
            {
                var plano = new Plano("Plano A", 199.90m, "tenant-a", "user-a");
                contextTenantA.Planos.Add(plano);
                
                var cliente = new Cliente("Cliente A Ltda", "11111111000111", "contato@a.com", plano.Id, "tenant-a", "user-a");
                contextTenantA.Clientes.Add(cliente);

                await contextTenantA.SaveChangesAsync();
            }

            // Cadastra plano e cliente do Tenant B
            using (var contextTenantB = _fixture.CreateDbContext("tenant-b", "user-b"))
            {
                var plano = new Plano("Plano B", 299.90m, "tenant-b", "user-b");
                contextTenantB.Planos.Add(plano);

                var cliente = new Cliente("Cliente B Ltda", "22222222000122", "contato@b.com", plano.Id, "tenant-b", "user-b");
                contextTenantB.Clientes.Add(cliente);

                await contextTenantB.SaveChangesAsync();
            }

            // Act & Assert: Tenant A tenta ler os dados e só deve ver o cliente A
            using (var contextTenantA = _fixture.CreateDbContext("tenant-a", "user-a"))
            {
                var clientesVisiveis = await contextTenantA.Clientes.ToListAsync();
                
                Assert.Single(clientesVisiveis);
                Assert.Equal("Cliente A Ltda", clientesVisiveis.First().RazaoSocial);
                Assert.Equal("tenant-a", clientesVisiveis.First().TenantId);

                // Comprovação de RLS física no Banco: Mesmo ignorando os filtros de query lógicos do EF Core,
                // a RLS do PostgreSQL deve filtrar fisicamente a query baseada na variável de sessão 'app.current_tenant_id'
                // e retornar apenas o registro do tenant-a.
                var clientesIgnoreFilters = await contextTenantA.Clientes.IgnoreQueryFilters().ToListAsync();
                Assert.Single(clientesIgnoreFilters);
                Assert.Equal("tenant-a", clientesIgnoreFilters.First().TenantId);
            }

            // Tenant B tenta ler os dados e só deve ver o cliente B
            using (var contextTenantB = _fixture.CreateDbContext("tenant-b", "user-b"))
            {
                var clientesVisiveis = await contextTenantB.Clientes.ToListAsync();

                Assert.Single(clientesVisiveis);
                Assert.Equal("Cliente B Ltda", clientesVisiveis.First().RazaoSocial);
                Assert.Equal("tenant-b", clientesVisiveis.First().TenantId);

                // Comprovação de RLS física para Tenant B
                var clientesIgnoreFilters = await contextTenantB.Clientes.IgnoreQueryFilters().ToListAsync();
                Assert.Single(clientesIgnoreFilters);
                Assert.Equal("tenant-b", clientesIgnoreFilters.First().TenantId);
            }
        }

        [Fact]
        public async Task Deve_Garantir_Filtro_De_Soft_Delete_No_Banco_Real_Postgres()
        {
            // Se o Docker não estiver rodando no host, pula o teste de forma amigável
            if (!_fixture.DockerDisponivel)
            {
                return;
            }

            // Arrange: Inicializa o schema executando as migrations
            using (var initContext = _fixture.CreateDbContext("tenant-sistema", "user-sistema", isAdmin: true))
            {
                await initContext.Database.MigrateAsync();
                await initContext.Database.ExecuteSqlRawAsync(@"
                    DO $$ 
                    DECLARE 
                        r RECORD;
                    BEGIN
                        FOR r IN (SELECT tablename, schemaname FROM pg_tables WHERE schemaname IN ('plataforma', 'aplicativo')) 
                        LOOP
                            EXECUTE 'ALTER TABLE ""' || r.schemaname || '"".""' || r.tablename || '"" FORCE ROW LEVEL SECURITY';
                        END LOOP;
                    END $$;
                ");
            }

            Guid clienteId;

            using (var context = _fixture.CreateDbContext("tenant-c", "user-c"))
            {
                var plano = new Plano("Plano C", 99.90m, "tenant-c", "user-c");
                context.Planos.Add(plano);

                var cliente = new Cliente("Cliente C Ltda", "33333333000133", "contato@c.com", plano.Id, "tenant-c", "user-c");
                context.Clientes.Add(cliente);

                await context.SaveChangesAsync();
                clienteId = cliente.Id;
            }

            // Act: Deleta logicamente o cliente C
            using (var context = _fixture.CreateDbContext("tenant-c", "user-c"))
            {
                var cliente = await context.Clientes.FindAsync(clienteId);
                Assert.NotNull(cliente);
                
                cliente.Deletar("user-c");
                await context.SaveChangesAsync();
            }

            // Assert: Cliente C não deve aparecer na consulta padrão
            using (var context = _fixture.CreateDbContext("tenant-c", "user-c"))
            {
                var clientesVisiveis = await context.Clientes.ToListAsync();
                Assert.Empty(clientesVisiveis);

                // Mas deve aparecer se ignorarmos os filtros de query
                // A RLS física do Postgres continuará permitindo porque o tenant-c é o proprietário desta linha
                var clienteIgnorandoFiltros = await context.Clientes
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(c => c.Id == clienteId);

                Assert.NotNull(clienteIgnorandoFiltros);
                Assert.NotNull(clienteIgnorandoFiltros.DeletadoEm);
            }
        }
    }
}
