using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Epros.Infrastructure.Services;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Application.Contracts;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Epros.Tests
{
    public class CriptografiaSegredosTests
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VaultEncryptionService> _logger = new TestLogger<VaultEncryptionService>();

        public CriptografiaSegredosTests()
        {
            var configData = new Dictionary<string, string?>
            {
                {"Cofre:VaultUrl", "http://localhost:8200"},
                {"Cofre:VaultToken", "epros-dev-token"},
                {"Cofre:ChaveNome", "epros-kek"},
                {"Cofre:KekLocal", "ChaveMestraLocalDe32BytesTeste!!"}
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        [Fact]
        public async Task Deve_Criptografar_E_Descriptografar_Localmente_Com_AES_256_GCM()
        {
            // Organizar - Forçamos o modo offline usando uma URL inexistente para o Vault
            var configDataOffline = new Dictionary<string, string?>
            {
                {"Cofre:VaultUrl", "http://localhost:9999"}, // Porta errada para simular offline
                {"Cofre:VaultToken", "token"},
                {"Cofre:ChaveNome", "chave"},
                {"Cofre:KekLocal", "MinhaChaveMestraParaTesteLocal123!"}
            };
            var configOffline = new ConfigurationBuilder()
                .AddInMemoryCollection(configDataOffline)
                .Build();

            using var httpClient = new HttpClient();
            var service = new VaultEncryptionService(httpClient, configOffline, _logger);
            var valorOriginal = "stripe-secret-api-key-2026";

            // Agir
            var ciphertext = await service.CriptografarAsync(valorOriginal);
            var valorDescriptografado = await service.DescriptografarAsync(ciphertext);

            // Assertiva
            Assert.NotNull(ciphertext);
            Assert.StartsWith("local:v1:", ciphertext);
            Assert.NotEqual(valorOriginal, ciphertext);
            Assert.Equal(valorOriginal, valorDescriptografado);
        }

        [Fact]
        public async Task Deve_Ativar_Resiliencia_Silenciosa_Quando_Vault_Offline()
        {
            // Organizar - Aponta para host inexistente para forçar SocketException/HttpRequestException
            var configDataInvalida = new Dictionary<string, string?>
            {
                {"Cofre:VaultUrl", "http://host-inexistente-local-test:8200"},
                {"Cofre:VaultToken", "dev"},
                {"Cofre:ChaveNome", "test-kek"},
                {"Cofre:KekLocal", "ChaveMestraLocalDe32BytesTeste!!"}
            };
            var configInvalida = new ConfigurationBuilder()
                .AddInMemoryCollection(configDataInvalida)
                .Build();

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromMilliseconds(500) };
            var service = new VaultEncryptionService(httpClient, configInvalida, _logger);

            // Agir & Assertiva (Não deve estourar erro ao inicializar e deve criptografar localmente)
            var exception = await Record.ExceptionAsync(async () =>
            {
                var cipher = await service.CriptografarAsync("teste-resiliencia");
                Assert.StartsWith("local:v1:", cipher);
            });

            Assert.Null(exception);
        }

        [Fact]
        public async Task Deve_Salvar_E_Obter_Configuracao_Criptografada_No_Fluxo_Cqrs_Ponta_A_Ponta()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_config_segredos_integration")
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("user-system");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            // Instancia o cofre service em modo local (URL inválida)
            var configDataOffline = new Dictionary<string, string?>
            {
                {"Cofre:VaultUrl", "http://localhost:9999"},
                {"Cofre:VaultToken", "dev"},
                {"Cofre:ChaveNome", "test-kek"},
                {"Cofre:KekLocal", "MinhaChaveMestraParaTesteLocal123!"}
            };
            var configOffline = new ConfigurationBuilder().AddInMemoryCollection(configDataOffline).Build();
            using var httpClient = new HttpClient();
            var cofreService = new VaultEncryptionService(httpClient, configOffline, _logger);

            // CQRS Handlers
            var definerHandler = new DefinirConfiguracaoGlobalCommandHandler(context, tenantProvider, currentUser, new FakeConfiguracaoGlobalCache(), cofreService);
            var obterHandler = new ObterConfiguracaoGlobalQueryHandler(context, tenantProvider, new FakeConfiguracaoGlobalCache(), cofreService);

            var valorOriginalSegredo = "asaas_api_secret_key_abcdef123";
            var commandSegredo = new DefinirConfiguracaoGlobalCommand("asaas_key", valorOriginalSegredo, true, "Chave API Asaas");

            // Agir - Salvar Segredo
            var resultDefinir = await definerHandler.Handle(commandSegredo, CancellationToken.None);
            Assert.True(resultDefinir.Sucesso);

            // Assertiva 1 - Verificar se está criptografada no banco físico (memória)
            var configNoBanco = await context.ConfiguracoesGlobais.FirstOrDefaultAsync(c => c.Chave == "asaas_key");
            Assert.NotNull(configNoBanco);
            Assert.True(configNoBanco!.EhSegredo);
            Assert.StartsWith("local:v1:", configNoBanco.Valor);
            Assert.NotEqual(valorOriginalSegredo, configNoBanco.Valor);

            // Agir 2 - Obter Segredo
            var query = new ObterConfiguracaoGlobalQuery("asaas_key");
            var resultObter = await obterHandler.Handle(query, CancellationToken.None);
            Assert.True(resultObter.Sucesso);

            // Assertiva 2 - Verificar se descriptografou de forma transparente
            var configRecuperada = resultObter.Dados as ConfiguracaoGlobal;
            Assert.NotNull(configRecuperada);
            Assert.Equal(valorOriginalSegredo, configRecuperada!.Valor);
        }

        [Fact]
        public async Task Deve_Salvar_Configuracao_Comum_Em_Texto_Claro()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_config_comum_integration")
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("user-system");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            using var httpClient = new HttpClient();
            var cofreService = new VaultEncryptionService(httpClient, _configuration, _logger);

            var definerHandler = new DefinirConfiguracaoGlobalCommandHandler(context, tenantProvider, currentUser, new FakeConfiguracaoGlobalCache(), cofreService);
            var obterHandler = new ObterConfiguracaoGlobalQueryHandler(context, tenantProvider, new FakeConfiguracaoGlobalCache(), cofreService);

            var valorOriginalComum = "mail.epros.com.br";
            var commandComum = new DefinirConfiguracaoGlobalCommand("smtp_host", valorOriginalComum, false, "Host SMTP");

            // Agir
            var resultDefinir = await definerHandler.Handle(commandComum, CancellationToken.None);
            Assert.True(resultDefinir.Sucesso);

            // Assertiva - Deve salvar em texto plano sem prefixo
            var configNoBanco = await context.ConfiguracoesGlobais.FirstOrDefaultAsync(c => c.Chave == "smtp_host");
            Assert.NotNull(configNoBanco);
            Assert.False(configNoBanco!.EhSegredo);
            Assert.Equal(valorOriginalComum, configNoBanco.Valor);

            // Obter e validar
            var query = new ObterConfiguracaoGlobalQuery("smtp_host");
            var resultObter = await obterHandler.Handle(query, CancellationToken.None);
            Assert.True(resultObter.Sucesso);
            var configRecuperada = resultObter.Dados as ConfiguracaoGlobal;
            Assert.NotNull(configRecuperada);
            Assert.Equal(valorOriginalComum, configRecuperada!.Valor);
        }

        // Classes de Teste Auxiliares
        private class TestLogger<T> : ILogger<T>
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
            }
        }

        private class FakeConfiguracaoGlobalCache : IConfiguracaoGlobalCache
        {
            public Task<Epros.Modules.GestaoClientes.Application.Dtos.ConfiguracaoGlobalCacheDto?> ObterAsync(string chave, Func<Task<ConfiguracaoGlobal?>> factory) => 
                factory().ContinueWith(t => t.Result == null ? null : new Epros.Modules.GestaoClientes.Application.Dtos.ConfiguracaoGlobalCacheDto(t.Result.Id, t.Result.Chave, t.Result.Valor, t.Result.EhSegredo, t.Result.Descricao, t.Result.TenantId));

            public Task InvalidarAsync(string chave) => Task.CompletedTask;
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
