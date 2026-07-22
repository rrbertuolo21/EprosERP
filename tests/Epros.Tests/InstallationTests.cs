using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Tests
{
    public class InstallationTests
    {
        private (ServiceProvider Provider, TestTenantProvider TenantProvider, TestCurrentUser CurrentUser) CreateServiceProvider(string databaseName, string tenantId = "system")
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
            var currentUser = new TestCurrentUser("system-installer");

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);

            return (services.BuildServiceProvider(), tenantProvider, currentUser);
        }

        [Fact]
        public async Task ObterInstalacaoStateQuery_DeveRetornarIsCompletedFalse_QuandoBancoNaoInstalado()
        {
            // Arrange
            var dbName = "db_test_install_state_new_" + Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider(dbName);
            var context = provider.GetRequiredService<ContextAplicativo>();

            var queryHandler = new ObterInstalacaoStateQueryHandler(context);
            var query = new ObterInstalacaoStateQuery();

            // Act
            var result = await queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.IsCompleted);
            Assert.False(result.DatabaseInitialized);
            Assert.False(result.AdminCreated);
            Assert.False(result.SystemSettingsSeeded);
        }

        [Fact]
        public async Task VerificarRequisitosQuery_DeveExecutarSemErros()
        {
            // Arrange
            var dbName = "db_test_install_reqs_" + Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider(dbName);
            var context = provider.GetRequiredService<ContextAplicativo>();

            var queryHandler = new VerificarRequisitosQueryHandler(context);
            var query = new VerificarRequisitosQuery();

            // Act
            var result = await queryHandler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Pass);
            Assert.NotEmpty(result.Itens);
            Assert.Contains(result.Itens, i => i.Nome == "Conexão com Banco de Dados" && i.Status);
            Assert.Contains(result.Itens, i => i.Nome == "Diretório Temporário" && i.Status);
        }

        [Fact]
        public async Task ExecutarInstalacaoCommand_DeveInstalarComSucessoE_BloquearReexecucao()
        {
            // Arrange
            var dbName = "db_test_install_flow_" + Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider(dbName);
            var context = provider.GetRequiredService<ContextAplicativo>();

            var handler = new ExecutarInstalacaoCommandHandler(context, provider, new Epros.Infrastructure.Services.Pbkdf2PasswordHasher());
            var command = new ExecutarInstalacaoCommand(
                AdminNome: "Super Admin",
                AdminEmail: "admin@epros.com.br",
                AdminSenha: "SenhaSuperSegura123"
            );

            // Act 1: Executa Instalação Inicial
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert 1: Deve instalar com sucesso
            Assert.True(result.Sucesso);

            var state = await context.InstalacaoStates.IgnoreQueryFilters().FirstOrDefaultAsync();
            Assert.NotNull(state);
            Assert.True(state.IsCompleted);
            Assert.True(state.DatabaseInitialized);
            Assert.True(state.AdminCreated);
            Assert.True(state.SystemSettingsSeeded);

            // Deve ter semeado administrador e configurações globais
            var adminUser = await context.UsuariosInternos.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Email == "admin@epros.com.br");
            Assert.NotNull(adminUser);
            Assert.True(adminUser.PrimaryAdmin);

            var setting = await context.SystemSettings.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Chave == "empresa_siser");
            Assert.NotNull(setting);
            Assert.Equal("Siser Holding S.A.", setting.Valor);

            // Act 2: Tenta reexecutar a instalação
            var reResult = await handler.Handle(command, CancellationToken.None);

            // Assert 2: Deve ser bloqueado (REG-016)
            Assert.False(reResult.Sucesso);
            Assert.Contains("Reexecução bloqueada", reResult.Mensagem);
        }

        [Fact]
        public async Task ExecutarAtualizacaoCommand_DeveGravarUpdateLogEConsultarHistorico()
        {
            // Arrange
            var dbName = "db_test_upgrade_flow_" + Guid.NewGuid();
            var (provider, _, currentUser) = CreateServiceProvider(dbName);
            var context = provider.GetRequiredService<ContextAplicativo>();

            var handler = new ExecutarAtualizacaoCommandHandler(context, provider, currentUser);
            var command = new ExecutarAtualizacaoCommand("v2026.06.18.1");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            // Verifica se o log foi gravado no banco de dados (REG-019)
            var logsQueryHandler = new ListarUpdateLogsQueryHandler(context);
            var logs = (await logsQueryHandler.Handle(new ListarUpdateLogsQuery(), CancellationToken.None)).ToList();

            Assert.NotEmpty(logs);
            Assert.Equal(1, logs.Count);
            Assert.Equal("v2026.06.18.1", logs[0].VersaoAlvo);
            Assert.True(logs[0].Sucesso);
            Assert.Equal("system-installer", logs[0].ExecutadoPor);
            Assert.Contains("Atualizações", logs[0].Log);
        }

        #region Provedores de Teste
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
            public string? GetUserName() => "Super Admin Installer";
            public string? GetUserEmail() => "super@siser.com";
        }
        #endregion
    }
}
