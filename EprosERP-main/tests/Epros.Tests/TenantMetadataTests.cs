using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Epros.API.Middlewares;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Tests
{
    /// <summary>IHostEnvironment de teste para exercitar o InquilinoSaaSMiddleware em ambientes específicos.</summary>
    internal sealed class TestHostEnvironment : Microsoft.Extensions.Hosting.IHostEnvironment
    {
        public TestHostEnvironment(string environmentName) => EnvironmentName = environmentName;
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "Epros.Tests";
        public string ContentRootPath { get; set; } = System.AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }

    public class TenantMetadataTests
    {
        private (ServiceProvider Provider, TestTenantProvider TenantProvider, TestCurrentUser CurrentUser) CreateServiceProvider(string databaseName, string tenantId = "tenant-1")
        {
            var services = new ServiceCollection();

            var optAplicativo = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var optGestaoClientes = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser("system-test");

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);
            services.AddMemoryCache();

            return (services.BuildServiceProvider(), tenantProvider, currentUser);
        }

        [Fact]
        public async Task CriarClienteCommand_DeveSalvarMetadadosCorretamente()
        {
            // Arrange
            var dbName = "db_test_criar_cliente_metadata_" + Guid.NewGuid();
            var (provider, tenantProvider, currentUser) = CreateServiceProvider(dbName, "system-tenant");
            var context = provider.GetRequiredService<ContextGestaoClientes>();

            var planoId = Guid.NewGuid();
            var plano = new Plano("Plano Premium", 299.90m, null, 20, 10, "Todos", "system-tenant", "system-test");
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(plano, planoId);
            context.Planos.Add(plano);
            await context.SaveChangesAsync();

            var handler = new CriarClienteCommandHandler(context, tenantProvider, currentUser);
            var command = new CriarClienteCommand(
                RazaoSocial: "Empresa de Teste Ltda",
                Cnpj: "12345678000199",
                Email: "contato@empresa.com",
                PlanoId: planoId,
                Telefone: "(44) 99999-9999",
                NomeContato: "João Silva",
                IsDemo: true,
                TokenAcesso: "token-secreto-123"
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var clienteSalvo = await context.Clientes.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Cnpj == "12345678000199");
            Assert.NotNull(clienteSalvo);
            Assert.Equal("(44) 99999-9999", clienteSalvo.Telefone);
            Assert.Equal("João Silva", clienteSalvo.NomeContato);
            Assert.True(clienteSalvo.IsDemo);
            Assert.Equal("token-secreto-123", clienteSalvo.TokenAcesso);
        }

        [Fact]
        public async Task ObterContextoSessaoQuery_DevemRetornarBlockSeInadimplenteMaisDe15Dias()
        {
            // Arrange
            var dbName = "db_test_onboarding_block_" + Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider(dbName, "tenant-test");
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var plano = new Plano("Plano Teste", 100m, null, 10, 5, null, "tenant-test", "system");
            contextGestao.Planos.Add(plano);

            var cliente = new Cliente("Razao Social", "12345678000195", "cliente@teste.com", plano.Id, null, null, 10, "Active", "tenant-test", "system");
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            // Adiciona fatura atrasada há mais de 15 dias
            var faturaAtrasada = new Fatura(cliente.Id, 100.00m, DateTime.UtcNow.AddDays(-20), "tenant-test", "system");
            typeof(Fatura).GetProperty("Status")!.SetValue(faturaAtrasada, FaturaStatus.Atrasada);
            contextGestao.Faturas.Add(faturaAtrasada);
            await contextGestao.SaveChangesAsync();

            var queryHandler = new ObterContextoSessaoQueryHandler(contextApp, contextGestao);
            var query = new ObterContextoSessaoQuery("tenant-test", Guid.NewGuid());

            // Act
            var result = await queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Block); // Bloqueado devido à fatura atrasada > 15 dias (REG-017 / REG-023)
        }

        [Fact]
        public async Task InquilinoSaaSMiddleware_DeveResolverIsDemoDoBancoDeDados()
        {
            // Arrange
            var dbName = "db_test_middleware_" + Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider(dbName, "tenant-demo");
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var memoryCache = provider.GetRequiredService<IMemoryCache>();

            // Adicionar Cliente com IsDemo = true no banco
            var plano = new Plano("Plano Demo", 0m, null, 100, 100, null, "tenant-demo", "system");
            contextGestao.Planos.Add(plano);

            var cliente = new Cliente("Empresa Demo", "12345678000195", "demo@teste.com", plano.Id, null, null, 10, "Active", "tenant-demo", "system", isDemo: true);
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            // Setup Middleware
            HttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["X-Tenant-Id"] = "tenant-demo";

            var middleware = new InquilinoSaaSMiddleware(
                next: (innerContext) => Task.CompletedTask,
                logger: NullLogger<InquilinoSaaSMiddleware>.Instance,
                environment: new TestHostEnvironment("Development")
            );

            // Act
            await middleware.InvokeAsync(httpContext, contextGestao, memoryCache);

            // Assert
            Assert.True(httpContext.Items.TryGetValue("IsDemo", out var isDemoObj));
            Assert.True((bool)isDemoObj!);
            Assert.Equal("tenant-demo", httpContext.Items["TenantId"]);
        }

        #region Helpers
        public class TestTenantProvider : ITenantProvider
        {
            public string TenantId { get; set; }
            public TestTenantProvider(string tenantId) => TenantId = tenantId;
            public string GetTenantId() => TenantId;
        }

        public class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
        #endregion
    }
}
