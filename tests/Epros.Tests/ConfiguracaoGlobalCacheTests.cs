using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Services;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Epros.Tests
{
    public class ConfiguracaoGlobalCacheTests
    {
        private (ContextGestaoClientes Context, TestTenantProvider TenantProvider, TestCurrentUser CurrentUser) CreateContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("user-system");
            var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            return (context, tenantProvider, currentUser);
        }

        private IConfiguration CreateConfig(string redisConnection = "localhost:6379")
        {
            var inMemorySettings = new Dictionary<string, string?> {
                {"ConnectionStrings:RedisConnection", redisConnection}
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
        }

        [Fact]
        public async Task ObterAsync_DeveFazerFallbackParaL1EBanco_SeRedisEstiverOffline()
        {
            // Arrange
            var (context, tenantProvider, _) = CreateContext("db_cache_fallback");

            var config = new ConfiguracaoGlobal("smtp_host", "smtp.test.com", false, "SMTP Test", "system", "user-system");
            context.ConfiguracoesGlobais.Add(config);
            await context.SaveChangesAsync();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var configuration = CreateConfig("invalid_host_force_offline:6379"); // Conexão inválida para forçar o fallback L2
            var cacheService = new ConfiguracaoGlobalCache(memoryCache, configuration, NullLogger<ConfiguracaoGlobalCache>.Instance);

            int factoryCalls = 0;
            Func<Task<ConfiguracaoGlobal?>> factory = async () =>
            {
                factoryCalls++;
                return await context.ConfiguracoesGlobais.FirstOrDefaultAsync(c => c.Chave == "smtp_host");
            };

            // Act 1: Primeira leitura (deve bater no banco)
            var result1 = await cacheService.ObterAsync("smtp_host", factory);

            // Assert 1: Deve ler com sucesso e chamar a factory
            Assert.NotNull(result1);
            Assert.Equal("smtp.test.com", result1!.Valor);
            Assert.Equal(1, factoryCalls);

            // Act 2: Segunda leitura (deve ler do L1 InMemory, sem chamar o banco)
            var result2 = await cacheService.ObterAsync("smtp_host", factory);

            // Assert 2: Deve ler com sucesso do cache e NÃO incrementar chamadas à factory
            Assert.NotNull(result2);
            Assert.Equal("smtp.test.com", result2!.Valor);
            Assert.Equal(1, factoryCalls);
        }

        [Fact]
        public async Task InvalidarAsync_DeveLimparCacheL1_EForçarNovaConsultaAoBanco()
        {
            // Arrange
            var (context, tenantProvider, _) = CreateContext("db_cache_invalidation");

            var config = new ConfiguracaoGlobal("smtp_host", "smtp.test.com", false, "SMTP Test", "system", "user-system");
            context.ConfiguracoesGlobais.Add(config);
            await context.SaveChangesAsync();

            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var configuration = CreateConfig("invalid_host_force_offline:6379");
            var cacheService = new ConfiguracaoGlobalCache(memoryCache, configuration, NullLogger<ConfiguracaoGlobalCache>.Instance);

            int factoryCalls = 0;
            Func<Task<ConfiguracaoGlobal?>> factory = async () =>
            {
                factoryCalls++;
                return await context.ConfiguracoesGlobais.FirstOrDefaultAsync(c => c.Chave == "smtp_host");
            };

            // Act 1: Lê pela primeira vez (grava no cache L1)
            await cacheService.ObterAsync("smtp_host", factory);
            Assert.Equal(1, factoryCalls);

            // Act 2: Invalida o cache
            await cacheService.InvalidarAsync("smtp_host");

            // Act 3: Lê novamente (deve ir ao banco novamente, pois o cache foi invalidado)
            var result = await cacheService.ObterAsync("smtp_host", factory);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("smtp.test.com", result!.Valor);
            Assert.Equal(2, factoryCalls); // Deve ter chamado a factory novamente
        }

        [Fact]
        public async Task Handlers_Integrados_Com_Cache_DevemFuncionarCorretamente()
        {
            // Arrange
            var (context, tenantProvider, currentUser) = CreateContext("db_cache_handlers_integrated");
            var memoryCache = new MemoryCache(new MemoryCacheOptions());
            var configuration = CreateConfig("invalid_host_force_offline:6379");
            var cacheService = new ConfiguracaoGlobalCache(memoryCache, configuration, NullLogger<ConfiguracaoGlobalCache>.Instance);

            var definirHandler = new DefinirConfiguracaoGlobalCommandHandler(context, tenantProvider, currentUser, cacheService, new FakeSegredoCofreService());
            var obterHandler = new ObterConfiguracaoGlobalQueryHandler(context, tenantProvider, cacheService, new FakeSegredoCofreService());

            // 1. Cadastra uma nova configuração global
            var command = new DefinirConfiguracaoGlobalCommand("smtp_port", "465", false, "SMTP Port SSL");
            var resultDefinir = await definirHandler.Handle(command, CancellationToken.None);
            Assert.True(resultDefinir.Sucesso);

            // 2. Consulta a configuração cadastrada (deve bater no banco e popular o cache L1)
            var query = new ObterConfiguracaoGlobalQuery("smtp_port");
            var resultObter1 = await obterHandler.Handle(query, CancellationToken.None);
            Assert.True(resultObter1.Sucesso);
            var config1 = resultObter1.Dados as ConfiguracaoGlobal;
            Assert.NotNull(config1);
            Assert.Equal("465", config1!.Valor);

            // 3. Altera o valor da configuração (deve invalidar o cache L1)
            var commandAlterar = new DefinirConfiguracaoGlobalCommand("smtp_port", "587", false, "SMTP Port TLS");
            var resultAlterar = await definirHandler.Handle(commandAlterar, CancellationToken.None);
            Assert.True(resultAlterar.Sucesso);

            // 4. Consulta novamente (deve obter o valor atualizado do banco)
            var resultObter2 = await obterHandler.Handle(query, CancellationToken.None);
            Assert.True(resultObter2.Sucesso);
            var config2 = resultObter2.Dados as ConfiguracaoGlobal;
            Assert.NotNull(config2);
            Assert.Equal("587", config2!.Valor);
        }

        private class FakeSegredoCofreService : ISegredoCofreService
        {
            public Task<string> CriptografarAsync(string valor) => Task.FromResult(valor);
            public Task<string> DescriptografarAsync(string ciphertext) => Task.FromResult(ciphertext);
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
            public bool EhTenantDemo() => false;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
