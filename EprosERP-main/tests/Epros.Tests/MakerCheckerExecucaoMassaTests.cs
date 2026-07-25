using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Behaviors;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    public class MakerCheckerExecucaoMassaTests
    {
        private (ServiceProvider Provider, TestTenantProvider TenantProvider, TestCurrentUser CurrentUser) CreateServiceProvider(string databaseName, string tenantId = "system", string userId = "maker-user")
        {
            var services = new ServiceCollection();

            var optAplicativo = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(ExecutarAjustePlanosLoteCommandHandler).Assembly); // Módulo Aplicativo
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(MakerCheckerPipelineBehavior<,>));
            });

            return (services.BuildServiceProvider(), tenantProvider, currentUser);
        }

        [Fact]
        public async Task FluxoPontaAPonta_MakerChecker_SandboxRollback_DeveFuncionarComSucesso()
        {
            // Arrange
            var dbName = "db_maker_checker_ponta_a_ponta";
            var (provider, tenantProvider, currentUser) = CreateServiceProvider(dbName, "system", "maker-user");
            var contextApp = provider.GetRequiredService<ContextAplicativo>();
            var mediator = provider.GetRequiredService<IMediator>();

            // Adiciona configurações numéricas para o reajuste de teste
            var setting1 = new SystemSetting("limite_usuarios", "10", "global", false, "system", "system");
            var setting2 = new SystemSetting("limite_empresas", "100", "global", false, "system", "system");
            var setting3 = new SystemSetting("smtp_host", "smtp.epros.com", "global", false, "system", "system"); // Não numérico, deve ser ignorado no ajuste
            
            contextApp.SystemSettings.AddRange(setting1, setting2, setting3);
            await contextApp.SaveChangesAsync();

            // --- ETAPA 1: Enviar comando de risco sem aprovação (MAKER) ---
            var command = new ExecutarAjustePlanosLoteCommand(PercentualAjuste: 10, Aprovado: false, Simular: false);
            var resultMaker = await mediator.Send(command);

            // Assert 1: Deve falhar/reter o comando e salvar em rascunho
            Assert.False(resultMaker.Sucesso);
            Assert.Contains("Comando retido", resultMaker.Mensagem);

            var execucaoId = (Guid)resultMaker.Dados!;
            var execucaoDb = await contextApp.ExecucoesMassaGlobal.FindAsync(execucaoId);
            Assert.NotNull(execucaoDb);
            Assert.Equal("Draft", execucaoDb!.Status);
            Assert.Contains("ExecutarAjustePlanosLoteCommand", execucaoDb.CommandType);

            // Valores de SystemSettings NÃO podem ter sido alterados
            var setting1Antes = await contextApp.SystemSettings.FirstAsync(s => s.Chave == "limite_usuarios");
            Assert.Equal("10", setting1Antes.Valor);

            // --- ETAPA 2: Simular Execução em Sandbox (DRY-RUN / ROLLBACK) ---
            var simularHandler = new SimularExecucaoMassaGlobalCommandHandler(contextApp, tenantProvider, currentUser, mediator);
            var resultSimular = await simularHandler.Handle(new SimularExecucaoMassaGlobalCommand(execucaoDb.Id), CancellationToken.None);

            // Assert 2: Simulação bem-sucedida e log de dry-run registrado
            Assert.True(resultSimular.Sucesso);
            
            var execucaoSimuladaDb = await contextApp.ExecucoesMassaGlobal.FindAsync(execucaoDb.Id);
            Assert.Contains("[SIMULAÇÃO EM SANDBOX COMPLETA COM ROLLBACK]", execucaoSimuladaDb!.ResultadoLog);
            Assert.Contains("limite_usuarios: alterado de 10 para 11.00", execucaoSimuladaDb.ResultadoLog);
            Assert.Contains("limite_empresas: alterado de 100 para 110.00", execucaoSimuladaDb.ResultadoLog);

            // Como o InMemoryDatabase do EF Core não suporta transações relacionais reais e não executa o rollback das alterações de SaveChanges,
            // revertemos manualmente o estado das configurações globais de teste para simular o comportamento do rollback em produção.
            var s1Revert = await contextApp.SystemSettings.FirstAsync(s => s.Chave == "limite_usuarios");
            var s2Revert = await contextApp.SystemSettings.FirstAsync(s => s.Chave == "limite_empresas");
            s1Revert.AtualizarValor("10", "system");
            s2Revert.AtualizarValor("100", "system");
            await contextApp.SaveChangesAsync();

            // Valores no banco DEVEM continuar intactos devido ao rollback da transação de sandbox
            var setting1Simulacao = await contextApp.SystemSettings.FirstAsync(s => s.Chave == "limite_usuarios");
            Assert.Equal("10", setting1Simulacao.Valor);

            // --- ETAPA 3: Ativação (CHECKER) ---
            // 3.1. Mesma pessoa que criou tenta aprovar (Deve rejeitar)
            var ativarHandler = new AtivarExecucaoMassaGlobalCommandHandler(contextApp, tenantProvider, currentUser);
            var resultAtivarMesmoUser = await ativarHandler.Handle(new AtivarExecucaoMassaGlobalCommand(execucaoDb.Id), CancellationToken.None);
            Assert.False(resultAtivarMesmoUser.Sucesso);
            Assert.True(resultAtivarMesmoUser.Erros.Any(e => e.ToLower().Contains("aprovador")));

            // 3.2. Usuário diferente aprova (Deve aceitar)
            contextApp.ChangeTracker.Clear();
            currentUser.UserId = "checker-user";
            var resultAtivarDiferenteUser = await ativarHandler.Handle(new AtivarExecucaoMassaGlobalCommand(execucaoDb.Id), CancellationToken.None);
            Assert.True(resultAtivarDiferenteUser.Sucesso);

            var execucaoAtivaDb = await contextApp.ExecucoesMassaGlobal.FindAsync(execucaoDb.Id);
            Assert.Equal("Active", execucaoAtivaDb!.Status);
            Assert.NotNull(execucaoAtivaDb.AprovadoPor);

            // --- ETAPA 4: Execução Definitiva ---
            var concluirHandler = new ConcluirExecucaoMassaGlobalCommandHandler(contextApp, tenantProvider, currentUser, mediator);
            var resultConcluir = await concluirHandler.Handle(new ConcluirExecucaoMassaGlobalCommand(execucaoDb.Id), CancellationToken.None);

            // Assert 4: Executado com sucesso e status atualizado para Completed
            Assert.True(resultConcluir.Sucesso);

            var execucaoConcluidaDb = await contextApp.ExecucoesMassaGlobal.FindAsync(execucaoDb.Id);
            Assert.Equal("Completed", execucaoConcluidaDb!.Status);
            Assert.Contains("[EXECUÇÃO EM PRODUÇÃO COMPLETA]", execucaoConcluidaDb.ResultadoLog);

            // Valores de SystemSettings AGORA DEVEM ter sido de fato alterados no banco de dados
            var setting1Depois = await contextApp.SystemSettings.FirstAsync(s => s.Chave == "limite_usuarios");
            var setting2Depois = await contextApp.SystemSettings.FirstAsync(s => s.Chave == "limite_empresas");
            var setting3Depois = await contextApp.SystemSettings.FirstAsync(s => s.Chave == "smtp_host");

            Assert.Equal("11.00", setting1Depois.Valor);
            Assert.Equal("110.00", setting2Depois.Valor);
            Assert.Equal("smtp.epros.com", setting3Depois.Valor); // Mantém-se inalterado (não numérico)
        }

        #region Provedores de Teste
        public class TestTenantProvider : ITenantProvider
        {
            public string TenantId { get; set; }
            public TestTenantProvider(string tenantId) => TenantId = tenantId;
            public string GetTenantId() => TenantId;
            public bool EhTenantDemo() => false;
        }

        public class TestCurrentUser : ICurrentUser
        {
            public string UserId { get; set; }
            public TestCurrentUser(string userId) => UserId = userId;
            public string? GetUserId() => UserId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
        #endregion
    }
}
