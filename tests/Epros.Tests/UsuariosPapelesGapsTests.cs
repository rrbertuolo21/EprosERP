using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.Aplicativo.Infrastructure.Jobs;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Application.Events;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    public class UsuariosPapelesGapsTests
    {
        private (ServiceProvider Provider, TestTenantProvider TenantProvider, TestCurrentUser CurrentUser) CreateServiceProvider(string databaseName, string tenantId = "tenant-test", string userId = "admin-user")
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
            var currentUser = new TestCurrentUser(userId);
            var httpContextAccessor = new HttpContextAccessor();

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);
            services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
            services.AddSingleton<IValidadorLimitesSaaS, TestLimitesValidador>();
            services.AddSingleton<INotificacaoService>(new TestNotificacaoService());
            services.AddSingleton<IPasswordHasher, Epros.Infrastructure.Services.Pbkdf2PasswordHasher>();
            services.AddSingleton<Epros.Shared.Security.IEprosTokenService>(
                new Epros.Shared.Security.EprosTokenService("epros-test-signing-key-0123456789-abcdef"));

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AutenticarUsuarioCommandHandler).Assembly); // Módulo Aplicativo
                cfg.RegisterServicesFromAssembly(typeof(UsuarioSyncEventHandler).Assembly);       // Módulo GestaoClientes
            });

            return (services.BuildServiceProvider(), tenantProvider, currentUser);
        }

        [Fact]
        public async Task FluxoPontaAPonta_CriacaoUsuario_OutboxSincronizacao_DeveFuncionarComSucesso()
        {
            // Arrange
            var dbName = "db_gaps_criar_usuario";
            var (provider, _, _) = CreateServiceProvider(dbName, "tenant-epros");
            var contextApp = provider.GetRequiredService<ContextAplicativo>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var mediator = provider.GetRequiredService<IMediator>();
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();

            var command = new CriarUsuarioCommand(
                Nome: "Colaborador Vendedor",
                Email: "vendedor@epros.com",
                Senha: "SenhaForte@123",
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput>
                {
                    new UsuarioEmpresaInput(Guid.NewGuid(), Guid.NewGuid(), false, "Vendedor", "Comercial", 15m)
                }
            );

            // Act 1: Criar Usuário (Deve registrar no aplicativo e enfileirar outbox, mas ainda não criar perfil em GestaoClientes)
            var resultCriar = await mediator.Send(command);
            Assert.True(resultCriar.Sucesso);

            var usuarioId = contextApp.Usuarios.First(u => u.Email == "vendedor@epros.com").Id;

            // Perfil não deve existir ainda no ContextGestaoClientes
            var perfilExisteAntes = await contextGestao.PerfisUsuarios
                .AnyAsync(p => p.UserId == usuarioId.ToString());
            Assert.False(perfilExisteAntes);

            // Outbox deve conter 1 mensagem pendente
            var outboxPendentes = await contextApp.OutboxMessages
                .Where(m => m.EventType == "UsuarioCriado" && m.ProcessadoEm == null)
                .ToListAsync();
            Assert.Single(outboxPendentes);

            // Act 2: Executar o Outbox Job para processar a sincronização assíncrona
            var outboxJob = new AplicativoOutboxProcessorJob(contextApp, contextGestao, mediator, httpContextAccessor, provider.GetRequiredService<INotificacaoService>());
            await outboxJob.Execute(null!);

            // Assert 2: Outbox processado e PerfilUsuario criado no ContextGestaoClientes
            var outboxProcessado = await contextApp.OutboxMessages
                .AnyAsync(m => m.EventType == "UsuarioCriado" && m.ProcessadoEm == null);
            Assert.False(outboxProcessado);

            var perfilUsuarioDb = await contextGestao.PerfisUsuarios
                .FirstOrDefaultAsync(p => p.UserId == usuarioId.ToString());
            Assert.NotNull(perfilUsuarioDb);
            Assert.Equal("Colaborador Vendedor", perfilUsuarioDb!.Nome);
            Assert.Equal("Vendedor", perfilUsuarioDb.Cargo);
            Assert.Equal("Comercial", perfilUsuarioDb.Departamento);
            Assert.Equal(15m, perfilUsuarioDb.LimiteDesconto);
            Assert.True(perfilUsuarioDb.Ativo);
        }

        [Fact]
        public async Task FluxoPontaAPonta_AtualizacaoUsuario_E_Delecao_DeveSincronizarEventual_ComSucesso()
        {
            // Arrange
            var dbName = "db_gaps_atualizar_usuario";
            var (provider, _, _) = CreateServiceProvider(dbName, "tenant-epros");
            var contextApp = provider.GetRequiredService<ContextAplicativo>();
            var contextGestao = provider.GetRequiredService<ContextGestaoClientes>();
            var mediator = provider.GetRequiredService<IMediator>();
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();

            // Criação inicial
            var usuario = new Usuario("tenant-epros", "Nome Antigo", "antigo@epros.com", "senha", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);

            var empresaId = Guid.NewGuid();
            var perfilId = Guid.NewGuid();
            var vinculo = new UsuarioEmpresa("tenant-epros", usuario.Id, empresaId, perfilId, false, "system");
            contextApp.UsuariosEmpresas.Add(vinculo);

            var perfilOriginal = new Epros.Modules.GestaoClientes.Domain.Entities.PerfilColaborador(perfilId, usuario.Id.ToString(), "Nome Antigo", "antigo@epros.com", "Analista", "TI", 5m, "tenant-epros", "system");
            contextGestao.PerfisUsuarios.Add(perfilOriginal);

            await contextApp.SaveChangesAsync();
            await contextGestao.SaveChangesAsync();

            // Act 1: Atualizar Usuário
            var commandAtualizar = new AtualizarUsuarioCommand(
                UsuarioId: usuario.Id,
                Nome: "Nome Novo",
                Telefone: "11999999999",
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput>
                {
                    new UsuarioEmpresaInput(empresaId, Guid.NewGuid(), false, "Supervisor", "Vendas", 10m)
                }
            );

            var resultAtualizar = await mediator.Send(commandAtualizar);
            Assert.True(resultAtualizar.Sucesso);

            // Roda o Job para sincronizar
            var outboxJob = new AplicativoOutboxProcessorJob(contextApp, contextGestao, mediator, httpContextAccessor, provider.GetRequiredService<INotificacaoService>());
            await outboxJob.Execute(null!);

            // Assert 1: Dados no GestaoClientes devem ter sido sincronizados
            var perfilAtualizadoDb = await contextGestao.PerfisUsuarios.FirstAsync(p => p.Id == perfilId);
            Assert.Equal("Nome Novo", perfilAtualizadoDb.Nome);
            Assert.Equal("Supervisor", perfilAtualizadoDb.Cargo);
            Assert.Equal("Vendas", perfilAtualizadoDb.Departamento);
            Assert.Equal(10m, perfilAtualizadoDb.LimiteDesconto);

            // Act 2: Deletar Usuário (Exclusão Lógica + Inativação de Perfil de Vendas)
            // Para deletar, precisamos garantir que tem outro admin na empresa do vínculo para passar na trava
            var outroAdmin = new UsuarioEmpresa("tenant-epros", Guid.NewGuid(), empresaId, null, true, "system");
            contextApp.UsuariosEmpresas.Add(outroAdmin);
            await contextApp.SaveChangesAsync();

            var commandDeletar = new DeletarUsuarioCommand(usuario.Id);
            var resultDeletar = await mediator.Send(commandDeletar);
            Assert.True(resultDeletar.Sucesso);

            // Roda o Job para sincronizar a exclusão
            await outboxJob.Execute(null!);

            // Assert 2: PerfilUsuario deve ter sido inativado
            var perfilDeletadoDb = await contextGestao.PerfisUsuarios.FirstAsync(p => p.Id == perfilId);
            Assert.False(perfilDeletadoDb.Ativo);
        }

        [Fact]
        public async Task IniciarImpersonacao_DeveEnfileirarOutbox_E_EventHandlerDeveLogarComSucesso()
        {
            // Arrange
            var dbName = "db_gaps_impersonacao";
            var (provider, _, _) = CreateServiceProvider(dbName, "system", Guid.NewGuid().ToString());
            var contextApp = provider.GetRequiredService<ContextAplicativo>();
            var mediator = provider.GetRequiredService<IMediator>();
            var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();

            var usuarioAlvo = new Usuario("tenant-alvo", "Usuário Alvo", "alvo@epros.com", "senha", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuarioAlvo);
            
            var vinculo = new UsuarioEmpresa("tenant-alvo", usuarioAlvo.Id, Guid.NewGuid(), null, false, "system"); // não-admin (admin é bloqueado pela salvaguarda)
            contextApp.UsuariosEmpresas.Add(vinculo);
            await contextApp.SaveChangesAsync();

            var command = new IniciarImpersonacaoCommand(usuarioAlvo.Id, vinculo.EmpresaId, "Necessidade de Suporte do Cliente");

            // Act: Iniciar impersonação
            var result = await mediator.Send(command);
            Assert.True(result.Sucesso);

            // Verifica que gerou outbox de impersonação
            var outboxMessage = await contextApp.OutboxMessages
                .FirstOrDefaultAsync(m => m.EventType == "ImpersonacaoIniciada" && m.ProcessadoEm == null);
            Assert.NotNull(outboxMessage);

            // Roda o Job para publicar no MediatR e disparar o alerta de segurança simulado
            var outboxJob = new AplicativoOutboxProcessorJob(contextApp, provider.GetRequiredService<ContextGestaoClientes>(), mediator, httpContextAccessor, provider.GetRequiredService<INotificacaoService>());
            await outboxJob.Execute(null!);

            // Verifica se a mensagem foi processada
            var messageProcessada = await contextApp.OutboxMessages.FirstAsync(m => m.Id == outboxMessage!.Id);
            Assert.NotNull(messageProcessada.ProcessadoEm);
        }

        [Fact]
        public async Task ExpiracaoSessoesJob_DeveForcarTimeout_E_FazerHardDelete_ConformeLGPD()
        {
            // Arrange
            var dbName = "db_gaps_job_lgpd";
            var (provider, _, _) = CreateServiceProvider(dbName, "tenant-epros");
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            // Sessão 1: Impersonação Ativa mas criada há 3 horas (deve expirar/sofrer timeout)
            var sessaoAtivaAntiga = new SessaoImpersonacao("system", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Suporte", "127.0.0.1", "system");
            contextApp.SessoesImpersonacao.Add(sessaoAtivaAntiga);
            await contextApp.SaveChangesAsync();

            // Usamos reflexão para setar InicioEm retroativo e forçar o estado modificado no InMemory
            typeof(SessaoImpersonacao).GetProperty(nameof(SessaoImpersonacao.InicioEm))!
                .SetValue(sessaoAtivaAntiga, DateTime.UtcNow.AddHours(-3));
            contextApp.Entry(sessaoAtivaAntiga).State = EntityState.Modified;
            await contextApp.SaveChangesAsync();

            // Sessão 2: Impersonação Encerrada há 95 dias (deve sofrer Hard Delete)
            var sessaoEncerradaLgpd = new SessaoImpersonacao("system", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Suporte", "127.0.0.1", "system");
            contextApp.SessoesImpersonacao.Add(sessaoEncerradaLgpd);
            await contextApp.SaveChangesAsync();

            sessaoEncerradaLgpd.Encerrar("system");
            typeof(SessaoImpersonacao).GetProperty(nameof(SessaoImpersonacao.FimEm))!
                .SetValue(sessaoEncerradaLgpd, DateTime.UtcNow.AddDays(-95));
            contextApp.Entry(sessaoEncerradaLgpd).State = EntityState.Modified;
            await contextApp.SaveChangesAsync();

            // Sessão 3: Sessão de Usuário expirada há 100 dias (deve sofrer Hard Delete)
            var sessaoUsuarioLgpd = new SessaoUsuario("tenant-epros", Guid.NewGuid(), "token-lgpd", "127.0.0.1", "agent", DateTime.UtcNow.AddDays(-100), "system");
            contextApp.SessoesUsuarios.Add(sessaoUsuarioLgpd);
            await contextApp.SaveChangesAsync();

            // Sessão 4: Sessão de Usuário recente e válida (deve permanecer)
            var sessaoUsuarioValida = new SessaoUsuario("tenant-epros", Guid.NewGuid(), "token-valido", "127.0.0.1", "agent", DateTime.UtcNow.AddHours(1), "system");
            contextApp.SessoesUsuarios.Add(sessaoUsuarioValida);
            await contextApp.SaveChangesAsync();

            // Depuração antes de rodar o Job
            var sessoesAntes = await contextApp.SessoesUsuarios.IgnoreQueryFilters().ToListAsync();
            foreach (var s in sessoesAntes)
            {
                Console.WriteLine($"[TEST DEBUG] Sessao: {s.Id}, Expiracao: {s.Expiracao}, Tenant: {s.TenantId}, DeletadoEm: {s.DeletadoEm}");
            }

            // Act: Executar o Job
            var job = new ExpiracaoSessoesJob(contextApp);
            await job.Execute(null!);

            // Assert
            // 1. A sessão ativa antiga deve ter sido encerrada (FimEm não nulo)
            var sessaoTimeout = await contextApp.SessoesImpersonacao.FirstAsync(s => s.Id == sessaoAtivaAntiga.Id);
            Assert.NotNull(sessaoTimeout.FimEm);

            // 2. A sessão encerrada há >90 dias deve ter sofrido Hard Delete físico do banco
            var sessaoEncerradaExiste = await contextApp.SessoesImpersonacao
                .IgnoreQueryFilters()
                .AnyAsync(s => s.Id == sessaoEncerradaLgpd.Id);
            Assert.False(sessaoEncerradaExiste);

            // 3. A sessão de usuário antiga deve ter sofrido Hard Delete
            var sessaoUsuarioAntigaExiste = await contextApp.SessoesUsuarios
                .IgnoreQueryFilters()
                .AnyAsync(s => s.Id == sessaoUsuarioLgpd.Id);
            Assert.False(sessaoUsuarioAntigaExiste);

            // 4. A sessão de usuário válida e recente deve permanecer intacta
            var sessaoUsuarioValidaExiste = await contextApp.SessoesUsuarios
                .AnyAsync(s => s.Id == sessaoUsuarioValida.Id);
            Assert.True(sessaoUsuarioValidaExiste);
        }

        #region Provedores de Teste
        public class TestLimitesValidador : IValidadorLimitesSaaS
        {
            public Task<bool> PossuiFolgaUsuariosAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult(true);
            public Task<bool> PossuiFolgaEmpresasAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult(true);
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteUsuariosAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((false, string.Empty));
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteEmpresasAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((false, string.Empty));
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteClientesAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((false, string.Empty));
            public Task<(bool Excedido, string Mensagem)> ValidarLimitePermissoesAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((false, string.Empty));
        }

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

        public class TestNotificacaoService : INotificacaoService
        {
            public Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml) => Task.CompletedTask;
            public Task EnviarSmsAsync(string telefone, string mensagem) => Task.CompletedTask;
            public Task EnviarWhatsAppAsync(string telefone, string mensagem) => Task.CompletedTask;
        }
        #endregion
    }
}
