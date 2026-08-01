using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Jobs;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class SuperAdminCqrsTests
    {
        private static readonly IPasswordHasher _hasher = new Epros.Infrastructure.Services.Pbkdf2PasswordHasher();

        private ContextAplicativo CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextAplicativo(options, tenantProvider, currentUser);
        }

        #region Testes de Segurança (Tenant "system")

        [Fact]
        public async Task Handlers_Devem_Rejeitar_Execucao_Se_Tenant_Nao_For_System()
        {
            // Arrange
            var context = CreateInMemoryContext("db_sec_fail", "tenant-regular", "operador-1");
            var tenantProvider = new TestTenantProvider("tenant-regular");
            var currentUser = new TestCurrentUser("operador-1");

            var handler = new CriarUsuarioInternoCommandHandler(context, tenantProvider, currentUser, _hasher);
            var command = new CriarUsuarioInternoCommand("Nome", "email@siser.com", "senhaSegura123", "America/Sao_Paulo", false);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Acesso Proibido"));
        }

        [Fact]
        public async Task Queries_Devem_Lancar_Excecao_Se_Tenant_Nao_For_System()
        {
            // Arrange
            var context = CreateInMemoryContext("db_sec_query_fail", "tenant-regular", "operador-1");
            var tenantProvider = new TestTenantProvider("tenant-regular");

            var queryHandler = new ListarUsuariosInternosQueryHandler(context, tenantProvider);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await queryHandler.Handle(new ListarUsuariosInternosQuery(), CancellationToken.None);
            });
        }

        #endregion

        #region Testes de UsuarioInterno

        [Fact]
        public async Task Deve_Criar_UsuarioInterno_Com_Sucesso_Via_Handler()
        {
            // Arrange
            var context = CreateInMemoryContext("db_user_create_success", "system", "admin-root");
            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("admin-root");

            var handler = new CriarUsuarioInternoCommandHandler(context, tenantProvider, currentUser, _hasher);
            var command = new CriarUsuarioInternoCommand("Operador Siser A", "op.a@siser.com", "senhaSegura123", "America/Sao_Paulo", false);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var userSalvo = await context.UsuariosInternos.FirstOrDefaultAsync(u => u.Email == "op.a@siser.com");
            Assert.NotNull(userSalvo);
            Assert.Equal("Operador Siser A", userSalvo.Nome);
        }

        [Fact]
        public async Task Nao_Deve_Criar_UsuarioInterno_Com_Email_Duplicado()
        {
            // Arrange
            var context = CreateInMemoryContext("db_user_dup_fail", "system", "admin-root");
            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("admin-root");

            // Insere um pré-existente
            var existente = new UsuarioInterno("Existente", "dup@siser.com", "senhaSegura123", Guid.NewGuid(), "u-1", "America/Sao_Paulo", false, "system", "admin-root");
            context.UsuariosInternos.Add(existente);
            await context.SaveChangesAsync();

            var handler = new CriarUsuarioInternoCommandHandler(context, tenantProvider, currentUser, _hasher);
            var command = new CriarUsuarioInternoCommand("Operador Novo", "dup@siser.com", "senhaSegura123", "America/Sao_Paulo", false);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Já existe"));
        }

        [Fact]
        public async Task Deve_Alterar_Senha_E_Promover_Para_AdminPrincipal_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_user_mutate", "system", "admin-root");
            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("admin-root");

            var usuario = new UsuarioInterno("Operador", "op@siser.com", "senhaSegura123", Guid.NewGuid(), "u-2", "America/Sao_Paulo", false, "system", "admin-root");
            context.UsuariosInternos.Add(usuario);
            await context.SaveChangesAsync();

            // Act: Alterar Senha
            var handlerSenha = new AlterarSenhaUsuarioInternoCommandHandler(context, tenantProvider, currentUser, _hasher);
            var resSenha = await handlerSenha.Handle(new AlterarSenhaUsuarioInternoCommand(usuario.Id, "novaSenhaSegura123"), CancellationToken.None);
            Assert.True(resSenha.Sucesso);

            // Act: Tornar Admin Principal
            var handlerAdmin = new TornarAdminPrincipalCommandHandler(context, tenantProvider, currentUser);
            var resAdmin = await handlerAdmin.Handle(new TornarAdminPrincipalCommand(usuario.Id), CancellationToken.None);
            Assert.True(resAdmin.Sucesso);

            // Assert
            var userAtualizado = await context.UsuariosInternos.FindAsync(usuario.Id);
            Assert.NotNull(userAtualizado);
            Assert.NotEqual("novaSenhaSegura123", userAtualizado.Senha); // hasheada, nunca em texto puro
            Assert.True(_hasher.Verify("novaSenhaSegura123", userAtualizado.Senha));
            Assert.True(userAtualizado.PrimaryAdmin);
        }

        #endregion

        #region Testes de SystemSetting

        [Fact]
        public async Task Deve_Definir_E_Atualizar_SystemSetting_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_setting_flow", "system", "admin-root");
            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("admin-root");

            var handler = new DefinirSystemSettingCommandHandler(context, tenantProvider, currentUser);

            // Act: Criar configuração
            var cmdCriar = new DefinirSystemSettingCommand("smtp.port", "587", "global", false);
            var resCriar = await handler.Handle(cmdCriar, CancellationToken.None);
            Assert.True(resCriar.Sucesso);

            var settingCriado = await context.SystemSettings.FirstOrDefaultAsync(s => s.Chave == "smtp.port" && s.Escopo == "global");
            Assert.NotNull(settingCriado);
            Assert.Equal("587", settingCriado.Valor);

            // Act: Atualizar valor
            var cmdAtualizar = new DefinirSystemSettingCommand("smtp.port", "465", "global", false);
            var resAtualizar = await handler.Handle(cmdAtualizar, CancellationToken.None);
            Assert.True(resAtualizar.Sucesso);

            var settingAtualizado = await context.SystemSettings.FirstOrDefaultAsync(s => s.Chave == "smtp.port" && s.Escopo == "global");
            Assert.NotNull(settingAtualizado);
            Assert.Equal("465", settingAtualizado.Valor);
        }

        #endregion

        #region Testes de ExecucaoMassaGlobal

        [Fact]
        public async Task Deve_Executar_Fluxo_Maker_Checker_Na_Execucao_Massa_Global()
        {
            // Arrange
            var dbName = "db_mass_flow_" + Guid.NewGuid().ToString();
            var tenantProvider = new TestTenantProvider("system");

            // 1. Maker cria a execução em rascunho
            Guid execId;
            using (var context = CreateInMemoryContext(dbName, "system", "operador-maker"))
            {
                var currentUserMaker = new TestCurrentUser("operador-maker");
                var handlerCriar = new CriarExecucaoMassaGlobalCommandHandler(context, tenantProvider, currentUserMaker);
                var cmdCriar = new CriarExecucaoMassaGlobalCommand("Lote de suspensão", "{}");
                var resCriar = await handlerCriar.Handle(cmdCriar, CancellationToken.None);
                Assert.True(resCriar.Sucesso);

                execId = (Guid)resCriar.Dados!.GetType().GetProperty("ExecucaoMassaGlobalId")!.GetValue(resCriar.Dados)!;
            }

            // 2. Tenta aprovar usando o mesmo Maker (deve falhar - Checker tem que ser outro)
            using (var context = CreateInMemoryContext(dbName, "system", "operador-maker"))
            {
                var currentUserMaker = new TestCurrentUser("operador-maker");
                var handlerAtivar = new AtivarExecucaoMassaGlobalCommandHandler(context, tenantProvider, currentUserMaker);
                var cmdAtivarMesmo = new AtivarExecucaoMassaGlobalCommand(execId);
                var resAtivarMesmo = await handlerAtivar.Handle(cmdAtivarMesmo, CancellationToken.None);
                Assert.False(resAtivarMesmo.Sucesso);
            }

            // 3. Checker aprova/ativa com sucesso
            using (var context = CreateInMemoryContext(dbName, "system", "operador-checker"))
            {
                var currentUserChecker = new TestCurrentUser("operador-checker");
                var handlerAtivarChecker = new AtivarExecucaoMassaGlobalCommandHandler(context, tenantProvider, currentUserChecker);
                var resAtivarChecker = await handlerAtivarChecker.Handle(new AtivarExecucaoMassaGlobalCommand(execId), CancellationToken.None);
                Assert.True(resAtivarChecker.Sucesso);
            }

            // 4. Concluir execução
            using (var context = CreateInMemoryContext(dbName, "system", "operador-checker"))
            {
                var currentUserChecker = new TestCurrentUser("operador-checker");
                var handlerConcluir = new ConcluirExecucaoMassaGlobalCommandHandler(context, tenantProvider, currentUserChecker, null!);
                var resConcluir = await handlerConcluir.Handle(new ConcluirExecucaoMassaGlobalCommand(execId), CancellationToken.None);
                Assert.True(resConcluir.Sucesso);

                // Assert final
                var execSalva = await context.ExecucoesMassaGlobal.FindAsync(execId);
                Assert.NotNull(execSalva);
                Assert.Equal("Completed", execSalva.Status);
            }
        }

        #endregion

        #region Testes de CustomPage e Newsletter

        [Fact]
        public async Task Deve_Gerenciar_CustomPage_E_Newsletter_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_cms_flow", "system", "admin-root");
            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("admin-root");

            // CMS CustomPage CRUD
            var handlerPage = new CriarCustomPageCommandHandler(context, tenantProvider, currentUser);
            var resPage = await handlerPage.Handle(new CriarCustomPageCommand("politica-privacidade", "<h1>Privacidade</h1>"), CancellationToken.None);
            Assert.True(resPage.Sucesso);
            var pageId = (Guid)resPage.Dados!.GetType().GetProperty("CustomPageId")!.GetValue(resPage.Dados)!;

            var handlerPublish = new PublicarCustomPageCommandHandler(context, tenantProvider, currentUser);
            var resPublish = await handlerPublish.Handle(new PublicarCustomPageCommand(pageId), CancellationToken.None);
            Assert.True(resPublish.Sucesso);

            var pageSalva = await context.CustomPages.FindAsync(pageId);
            Assert.Equal("Publicada", pageSalva!.Status);

            // Newsletter Subscriber Flow
            var handlerNews = new InscreverNewsletterCommandHandler(context, tenantProvider, currentUser);
            var resNews = await handlerNews.Handle(new InscreverNewsletterCommand("usuario@teste.com"), CancellationToken.None);
            Assert.True(resNews.Sucesso);
            var subId = (Guid)resNews.Dados!.GetType().GetProperty("NewsletterSubscriberId")!.GetValue(resNews.Dados)!;

            var handlerCancel = new CancelarNewsletterCommandHandler(context, tenantProvider, currentUser);
            var resCancel = await handlerCancel.Handle(new CancelarNewsletterCommand(subId), CancellationToken.None);
            Assert.True(resCancel.Sucesso);

            var subSalvo = await context.NewsletterSubscribers.FindAsync(subId);
            Assert.False(subSalvo!.Ativo); // Opt-out LGPD
        }

        #endregion

        #region Testes de Queries

        [Fact]
        public async Task Deve_Executar_Queries_De_Leitura_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_queries_flow", "system", "admin-root");
            var tenantProvider = new TestTenantProvider("system");

            var optionsGestao = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase("db_queries_flow_gestao")
                .Options;
            using var contextGestao = new ContextGestaoClientes(optionsGestao, tenantProvider, new TestCurrentUser("admin-root"));

            // Insere dados para ler no módulo Aplicativo
            var user = new UsuarioInterno("User A", "user.a@siser.com", "senhaSegura123", Guid.NewGuid(), "u-a", "America/Sao_Paulo", false, "system", "admin-root");
            var setting = new SystemSetting("smtp.port", "587", "global", false, "system", "admin-root");
            context.UsuariosInternos.Add(user);
            context.SystemSettings.Add(setting);
            await context.SaveChangesAsync();

            // Plano e assinatura ativa
            var planoId = Guid.NewGuid();
            var plano = new Plano("Plano Teste", 100.00m, "tenant-corp", "admin-root");
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(plano, planoId);
            contextGestao.Planos.Add(plano);

            // Insere dados para ler no módulo Gestão de Clientes
            var cliente = new Cliente("Inquilino Corp", "12.345.678/0001-90", "tenant@corp.com", planoId, "tenant-corp", "admin-root");
            var clienteId = Guid.NewGuid();
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(cliente, clienteId);
            contextGestao.Clientes.Add(cliente);

            var assinaturaAtiva = new AssinaturaCliente(
                clienteId: clienteId,
                planoId: planoId,
                status: AssinaturaStatus.Aprovada,
                dataInicio: DateTime.UtcNow,
                dataFim: null,
                trialAte: null,
                metodoPagamento: "Pix",
                transacaoId: "tx_123",
                detalhesPacoteJson: "{}",
                tenantId: "tenant-corp",
                criadoPor: "admin-root"
            );
            contextGestao.AssinaturasClientes.Add(assinaturaAtiva);

            // Contrato com valor recorrente para MRR
            var contrato = new Contrato(clienteId, 10, DateTime.UtcNow, null, "tenant-corp", "admin-root");
            contrato.DefinirValorRecorrenteManual(100.00m, "admin-root");
            contextGestao.Contratos.Add(contrato);

            // Fatura paga
            var fatura = new Fatura(clienteId, 100.00m, DateTime.UtcNow, "tenant-corp", "admin-root");
            fatura.Baixar("admin-root");
            contextGestao.Faturas.Add(fatura);

            // Assinatura cancelada nos últimos 30 dias (para teste de Churn)
            var assinaturaCancelada = new AssinaturaCliente(
                clienteId: clienteId,
                planoId: planoId,
                status: AssinaturaStatus.Aprovada,
                dataInicio: DateTime.UtcNow.AddDays(-60),
                dataFim: DateTime.UtcNow.AddDays(-30),
                trialAte: null,
                metodoPagamento: "Pix",
                transacaoId: "tx_124",
                detalhesPacoteJson: "{}",
                tenantId: "tenant-corp",
                criadoPor: "admin-root"
            );
            assinaturaCancelada.Expirar("admin-root");
            contextGestao.AssinaturasClientes.Add(assinaturaCancelada);

            await contextGestao.SaveChangesAsync();

            // Query Listar Usuarios
            var queryUsers = new ListarUsuariosInternosQueryHandler(context, tenantProvider);
            var listUsers = await queryUsers.Handle(new ListarUsuariosInternosQuery(), CancellationToken.None);
            Assert.NotEmpty(listUsers);

            // Query Listar Settings
            var querySettings = new ListarSystemSettingsQueryHandler(context, tenantProvider);
            var listSettings = await querySettings.Handle(new ListarSystemSettingsQuery(), CancellationToken.None);
            Assert.NotEmpty(listSettings);

            // Query Listar Clientes (Novo)
            var queryClientes = new ListarClientesQueryHandler(contextGestao, tenantProvider);
            var listClientes = await queryClientes.Handle(new ListarClientesQuery(), CancellationToken.None);
            Assert.NotEmpty(listClientes);
            var clienteDto = listClientes.First();
            Assert.Equal("Inquilino Corp", clienteDto.RazaoSocial);
            Assert.Equal("Plano Teste", clienteDto.PlanoNome);

            // Query Dashboard
            var queryDash = new ObterDashboardGlobalQueryHandler(context, contextGestao, tenantProvider);
            var dashDto = await queryDash.Handle(new ObterDashboardGlobalQuery(), CancellationToken.None);

            Assert.NotNull(dashDto);
            Assert.Equal(1, dashDto.TotalTenants);
            Assert.Equal(1, dashDto.TotalAssinaturasAtivas);
            Assert.Equal(100.00m, dashDto.ReceitaEstimadaMRR);
            Assert.Equal(100.00m, dashDto.ReceitaTotal);
            // 1 active, 1 cancelled in 30 days -> Churn Rate = (1 / (1 + 1)) * 100 = 50.00%
            Assert.Equal(50.00m, dashDto.ChurnRate);
        }

        #endregion

        #region Testes de ComunicacaoSuperAdmin (REG-020)

        [Fact]
        public async Task EnviarComunicacaoSuperAdmin_Deve_Enfileirar_No_Outbox_E_Processar_Via_Job()
        {
            // Arrange
            var dbName = "db_comunicacao_success";
            var context = CreateInMemoryContext(dbName, "system", "admin-root");
            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("admin-root");

            var optionsGestao = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(dbName)
                .Options;
            using var contextGestao = new ContextGestaoClientes(optionsGestao, tenantProvider, currentUser);

            // Add client in GestaoClientes
            var cliente = new Cliente(
                razaoSocial: "Empresa Alfa",
                cnpj: "12.345.678/0001-90",
                email: "alfa@cliente.com",
                planoId: Guid.NewGuid(),
                revendaId: null,
                vendedorId: null,
                diaVencimento: 10,
                statusSaaS: "Ativo",
                tenantId: "tenant-alfa",
                criadoPor: "admin-root",
                telefone: "11999999999"
            );
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            var handler = new EnviarComunicacaoSuperAdminCommandHandler(context, tenantProvider, currentUser);
            var command = new EnviarComunicacaoSuperAdminCommand(
                BusinessIds: new List<string> { "tenant-alfa" },
                Subject: "Assunto Teste",
                Message: "Olá {Nome}, esta é uma mensagem sobre {Assunto}",
                Canais: new List<string> { "Email", "SMS", "WhatsApp" }
            );

            // Act 1: Send communication
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert 1: Created successfully with Pendente status and Outbox queued
            Assert.True(result.Sucesso);
            var statusResult = result.Dados;
            Assert.NotNull(statusResult);
            var statusType = statusResult.GetType();
            var comunicacaoIdProp = statusType.GetProperty("ComunicacaoId");
            Assert.NotNull(comunicacaoIdProp);
            var comunicacaoId = (Guid)comunicacaoIdProp.GetValue(statusResult)!;

            var comunicacao = await context.ComunicacoesSuperAdmin.FirstOrDefaultAsync(c => c.Id == comunicacaoId);
            Assert.NotNull(comunicacao);
            Assert.Equal("Pendente", comunicacao.Status);
            Assert.Equal("tenant-alfa", comunicacao.BusinessIds.First());
            Assert.Equal(3, comunicacao.Canais.Count);

            var outboxMessage = await context.OutboxMessages.FirstOrDefaultAsync(o => o.EventType == "ComunicacaoSuperAdminCriada");
            Assert.NotNull(outboxMessage);
            Assert.Null(outboxMessage.ProcessadoEm);

            // Act 2: Execute Job with SpyNotificacaoService
            var spyNotif = new SpyNotificacaoService();
            var mockHttpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
            var outboxProcessor = new AplicativoOutboxProcessorJob(context, contextGestao, null!, mockHttpContextAccessor, spyNotif);

            await outboxProcessor.Execute(null!);

            // Assert 2: Status updated to Sucesso and channels called
            var comunicacaoProcessada = await context.ComunicacoesSuperAdmin.FirstOrDefaultAsync(c => c.Id == comunicacaoId);
            Assert.Equal("Sucesso", comunicacaoProcessada!.Status);
            Assert.Equal(1, spyNotif.EmailCalls);
            Assert.Equal(1, spyNotif.SmsCalls);
            Assert.Equal(1, spyNotif.WhatsappCalls);
            Assert.Equal("alfa@cliente.com", spyNotif.LastEmail);

            var outboxMessageProcessada = await context.OutboxMessages.FirstOrDefaultAsync(o => o.Id == outboxMessage.Id);
            Assert.NotNull(outboxMessageProcessada!.ProcessadoEm);
        }

        [Fact]
        public async Task EnviarComunicacaoSuperAdmin_Deve_Incrementar_Tentativas_E_Falhar_Apos_5_Erros()
        {
            // Arrange
            var dbName = "db_comunicacao_retry";
            var context = CreateInMemoryContext(dbName, "system", "admin-root");
            var tenantProvider = new TestTenantProvider("system");
            var currentUser = new TestCurrentUser("admin-root");

            var optionsGestao = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(dbName)
                .Options;
            using var contextGestao = new ContextGestaoClientes(optionsGestao, tenantProvider, currentUser);

            // Add client in GestaoClientes
            var cliente = new Cliente(
                razaoSocial: "Empresa Beta",
                cnpj: "12.345.678/0001-91",
                email: "beta@cliente.com",
                planoId: Guid.NewGuid(),
                revendaId: null,
                vendedorId: null,
                diaVencimento: 10,
                statusSaaS: "Ativo",
                tenantId: "tenant-beta",
                criadoPor: "admin-root",
                telefone: "11999999999"
            );
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            var handler = new EnviarComunicacaoSuperAdminCommandHandler(context, tenantProvider, currentUser);
            var command = new EnviarComunicacaoSuperAdminCommand(
                BusinessIds: new List<string> { "tenant-beta" },
                Subject: "Assunto Falha",
                Message: "Mensagem",
                Canais: new List<string> { "Email" }
            );

            var result = await handler.Handle(command, CancellationToken.None);
            var statusResult = result.Dados;
            Assert.NotNull(statusResult);
            var statusType = statusResult.GetType();
            var comunicacaoIdProp = statusType.GetProperty("ComunicacaoId");
            Assert.NotNull(comunicacaoIdProp);
            var comunicacaoId = (Guid)comunicacaoIdProp.GetValue(statusResult)!;

            var spyNotif = new SpyNotificacaoService { ShouldThrow = true };
            var mockHttpContextAccessor = new Microsoft.AspNetCore.Http.HttpContextAccessor();
            var outboxProcessor = new AplicativoOutboxProcessorJob(context, contextGestao, null!, mockHttpContextAccessor, spyNotif);

            // Act & Assert: Run job 5 times to exhaust attempts
            for (int i = 1; i <= 5; i++)
            {
                await outboxProcessor.Execute(null!);

                var outboxMessage = await context.OutboxMessages.AsNoTracking().FirstOrDefaultAsync(o => o.EventType == "ComunicacaoSuperAdminCriada");
                Assert.Equal(i, outboxMessage!.Tentativas);
                Assert.Null(outboxMessage.ProcessadoEm);

                var comunicacaoCheck = await context.ComunicacoesSuperAdmin.AsNoTracking().FirstOrDefaultAsync(c => c.Id == comunicacaoId);
                if (i < 5)
                {
                    Assert.Equal("Pendente", comunicacaoCheck!.Status);
                }
                else
                {
                    Assert.Equal("Falha", comunicacaoCheck!.Status);
                }
            }
        }

        #endregion

        #region Classes Auxiliares de Teste

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Super Admin Tester";
            public string? GetUserEmail() => "super@siser.com";
        }

        public class SpyNotificacaoService : INotificacaoService
        {
            public int EmailCalls { get; private set; }
            public int SmsCalls { get; private set; }
            public int WhatsappCalls { get; private set; }
            public string LastEmail { get; private set; } = string.Empty;
            public bool ShouldThrow { get; set; }

            public Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
            {
                EmailCalls++;
                LastEmail = destinatario;
                if (ShouldThrow) throw new Exception("Simulated service error");
                return Task.CompletedTask;
            }

            public Task EnviarSmsAsync(string telefone, string mensagem)
            {
                SmsCalls++;
                if (ShouldThrow) throw new Exception("Simulated service error");
                return Task.CompletedTask;
            }

            public Task EnviarWhatsAppAsync(string telefone, string mensagem)
            {
                WhatsappCalls++;
                if (ShouldThrow) throw new Exception("Simulated service error");
                return Task.CompletedTask;
            }
        }

        #endregion
    }
}
