using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Exceptions;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    public class ModoDemoTests
    {
        private (ServiceProvider Provider, TestDemoTenantProvider TenantProvider, TestCurrentUser CurrentUser) CreateServiceProvider(string databaseName, string tenantId = "tenant-test", bool isDemo = false)
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

            var tenantProvider = new TestDemoTenantProvider(tenantId, isDemo);
            var currentUser = new TestCurrentUser("admin-user");

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);

            return (services.BuildServiceProvider(), tenantProvider, currentUser);
        }

        [Fact]
        public async Task ModoDemo_EntidadesProtegidas_DeveBloquearInsercao()
        {
            // Arrange
            var dbName = "db_demo_bloqueio_insercao";
            var (provider, _, _) = CreateServiceProvider(dbName, "tenant-demo", isDemo: true);
            var contextApp = provider.GetRequiredService<ContextAplicativo>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();

            var usuario = new Usuario("tenant-demo", "Usuario Teste", "teste@epros.com", "senha", UsuarioTipo.Company, "system");
            var cliente = new Cliente("Empresa Demo", "12345678901234", "demo@epros.com", Guid.NewGuid(), "tenant-demo", "system");

            // Act & Assert 1: Inserção de Usuario (Módulo Aplicativo)
            contextApp.Usuarios.Add(usuario);
            await Assert.ThrowsAsync<OperacaoBloqueadaModoDemoException>(() => contextApp.SaveChangesAsync());

            // Limpa o estado para o próximo teste
            contextApp.Entry(usuario).State = EntityState.Detached;

            // Act & Assert 2: Inserção de Cliente (Módulo GestaoClientes)
            contextGestao.Clientes.Add(cliente);
            await Assert.ThrowsAsync<OperacaoBloqueadaModoDemoException>(() => contextGestao.SaveChangesAsync());
        }

        [Fact]
        public async Task ModoDemo_EntidadesProtegidas_DeveBloquearEdicao()
        {
            // Arrange
            var dbName = "db_demo_bloqueio_edicao";
            // 1. Criar dados em modo normal (isDemo: false)
            var (provider, tenantProvider, _) = CreateServiceProvider(dbName, "tenant-demo", isDemo: false);
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var usuario = new Usuario("tenant-demo", "Nome Antigo", "teste@epros.com", "senha", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);
            await contextApp.SaveChangesAsync();

            // 2. Mudar tenant para Modo Demo (isDemo: true)
            tenantProvider.IsDemo = true;

            // Act: Modificar entidade
            usuario.MarcarAlterado("system");

            // Assert: Salvar deve disparar exceção
            await Assert.ThrowsAsync<OperacaoBloqueadaModoDemoException>(() => contextApp.SaveChangesAsync());
        }

        [Fact]
        public async Task ModoDemo_EntidadesProtegidas_DeveBloquearExclusao()
        {
            // Arrange
            var dbName = "db_demo_bloqueio_exclusao";
            // 1. Criar dados em modo normal (isDemo: false)
            var (provider, tenantProvider, _) = CreateServiceProvider(dbName, "tenant-demo", isDemo: false);
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var usuario = new Usuario("tenant-demo", "Usuario Excluir", "teste@epros.com", "senha", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);
            await contextApp.SaveChangesAsync();

            // 2. Mudar tenant para Modo Demo (isDemo: true)
            tenantProvider.IsDemo = true;

            // Act: Deletar logicamente (Soft Delete)
            usuario.Deletar("system");

            // Assert: Salvar deve disparar exceção
            await Assert.ThrowsAsync<OperacaoBloqueadaModoDemoException>(() => contextApp.SaveChangesAsync());
        }

        [Fact]
        public async Task ModoDemo_EntidadesDeSessaoELogin_DevePermitirEscrita()
        {
            // Arrange
            var dbName = "db_demo_permitir_sessoes";
            var (provider, _, _) = CreateServiceProvider(dbName, "tenant-demo", isDemo: true);
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var sessaoUsuario = new SessaoUsuario("tenant-demo", Guid.NewGuid(), "token-demo", "127.0.0.1", "agent", DateTime.UtcNow.AddHours(1), "system");
            var sessaoImpersonacao = new SessaoImpersonacao("tenant-demo", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Suporte", "127.0.0.1", "system");
            var historicoLogin = new HistoricoLogin("tenant-demo", Guid.NewGuid(), "teste@epros.com", "127.0.0.1", "agent", true, "Sucesso", "system");

            // Act: Salvar entidades de login e sessão
            contextApp.SessoesUsuarios.Add(sessaoUsuario);
            contextApp.SessoesImpersonacao.Add(sessaoImpersonacao);
            contextApp.HistoricosLogin.Add(historicoLogin);

            var result = await contextApp.SaveChangesAsync();

            // Assert: Deve salvar com sucesso (3 registros)
            Assert.Equal(3, result);

            // Tenta editar a sessão (deve ser permitido)
            sessaoUsuario.Revogar("system");
            var resultEdit = await contextApp.SaveChangesAsync();
            Assert.Equal(1, resultEdit);
        }

        #region Provedores de Teste
        public class TestDemoTenantProvider : ITenantProvider
        {
            public string TenantId { get; set; }
            public bool IsDemo { get; set; }
            public TestDemoTenantProvider(string tenantId, bool isDemo)
            {
                TenantId = tenantId;
                IsDemo = isDemo;
            }
            public string GetTenantId() => TenantId;
            public bool EhTenantDemo() => IsDemo;
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
