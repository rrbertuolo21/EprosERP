using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Contracts;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class ConfiguracaoGlobalTests
    {
        [Fact]
        public void Deve_Criar_Configuracao_Global_Valida()
        {
            var config = new ConfiguracaoGlobal("smtp_host", "smtp.gmail.com", false, "Servidor de e-mail de saída", "system", "user-test");
            Assert.True(config.IsValid);
            Assert.Equal("smtp_host", config.Chave);
            Assert.Equal("smtp.gmail.com", config.Valor);
            Assert.False(config.EhSegredo);
            Assert.Equal("Servidor de e-mail de saída", config.Descricao);
        }

        [Fact]
        public void Deve_Invalidar_Configuracao_Com_Dados_Invalidos()
        {
            var config = new ConfiguracaoGlobal("", "", false, "Descricao", "system", "user-test");
            Assert.False(config.IsValid);
            Assert.Contains(config.Notifications, n => n.Key == "Chave");
            Assert.Contains(config.Notifications, n => n.Key == "Valor");
        }

        [Fact]
        public async Task Deve_Executar_Comando_Definir_Configuracao_Com_Sucesso_Para_Tenant_System()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_config_success")
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("user-system");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            var handler = new DefinirConfiguracaoGlobalCommandHandler(context, tenantProvider, currentUser, new FakeConfiguracaoGlobalCache(), new FakeSegredoCofreService());
            var command = new DefinirConfiguracaoGlobalCommand("token_key", "sec-123", true, "Chave JWT");

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.True(result.Sucesso);

            var configSalva = await context.ConfiguracoesGlobais.FirstOrDefaultAsync(c => c.Chave == "token_key");
            Assert.NotNull(configSalva);
            Assert.Equal("sec-123", configSalva!.Valor);
            Assert.True(configSalva.EhSegredo);
        }

        [Fact]
        public async Task Deve_Rejeitar_Comando_Definir_Configuracao_Para_Tenant_Diferente_De_System()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_config_forbidden")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-a");
            var currentUser = new TestCurrentUser("user-a");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            var handler = new DefinirConfiguracaoGlobalCommandHandler(context, tenantProvider, currentUser, new FakeConfiguracaoGlobalCache(), new FakeSegredoCofreService());
            var command = new DefinirConfiguracaoGlobalCommand("token_key", "sec-123", true, "Chave JWT");

            var result = await handler.Handle(command, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Acesso Proibido"));
        }

        [Fact]
        public async Task Deve_Obter_Configuracao_Com_Sucesso_Para_Tenant_System()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_config_get_success")
                .Options;

            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("user-system");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            var config = new ConfiguracaoGlobal("smtp_port", "587", false, "Porta SMTP", "system", "user-system");
            context.ConfiguracoesGlobais.Add(config);
            await context.SaveChangesAsync();

            var handler = new ObterConfiguracaoGlobalQueryHandler(context, tenantProvider, new FakeConfiguracaoGlobalCache(), new FakeSegredoCofreService());
            var query = new ObterConfiguracaoGlobalQuery("smtp_port");

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.True(result.Sucesso);
            var configRecuperada = result.Dados as ConfiguracaoGlobal;
            Assert.NotNull(configRecuperada);
            Assert.Equal("587", configRecuperada!.Valor);
        }

        [Fact]
        public async Task Deve_Rejeitar_Query_Obter_Configuracao_Para_Tenant_Diferente_De_System()
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_config_get_forbidden")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-a");
            var currentUser = new TestCurrentUser("user-a");
            using var context = new ContextGestaoClientes(options, tenantProvider, currentUser);

            var handler = new ObterConfiguracaoGlobalQueryHandler(context, tenantProvider, new FakeConfiguracaoGlobalCache(), new FakeSegredoCofreService());
            var query = new ObterConfiguracaoGlobalQuery("smtp_port");

            var result = await handler.Handle(query, CancellationToken.None);

            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Acesso Proibido"));
        }

        private class FakeConfiguracaoGlobalCache : IConfiguracaoGlobalCache
        {
            public Task<Epros.Modules.GestaoClientes.Application.Dtos.ConfiguracaoGlobalCacheDto?> ObterAsync(string chave, Func<Task<ConfiguracaoGlobal?>> factory) =>
                factory().ContinueWith(t => t.Result == null ? null : new Epros.Modules.GestaoClientes.Application.Dtos.ConfiguracaoGlobalCacheDto(t.Result.Id, t.Result.Chave, t.Result.Valor, t.Result.EhSegredo, t.Result.Descricao, t.Result.TenantId));

            public Task InvalidarAsync(string chave) => Task.CompletedTask;
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
