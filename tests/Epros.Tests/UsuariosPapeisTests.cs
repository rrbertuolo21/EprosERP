using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    public class UsuariosPapeisTests
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

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);
            services.AddSingleton<IValidadorLimitesSaaS, TestLimitesValidador>();
            services.AddSingleton<IPasswordHasher, Epros.Infrastructure.Services.Pbkdf2PasswordHasher>();
            services.AddSingleton<Epros.Shared.Security.IEprosTokenService>(
                new Epros.Shared.Security.EprosTokenService("epros-test-signing-key-0123456789-abcdef"));

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AutenticarUsuarioCommandHandler).Assembly);
            });

            return (services.BuildServiceProvider(), tenantProvider, currentUser);
        }

        [Fact]
        public async Task CriarUsuario_Com_Email_Duplicado_Deve_Retornar_Falha()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_user_dup_email");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var existUser = new Usuario("tenant-test", "Existente", "duplicado@teste.com", "SenhaForte@1", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(existUser);
            await contextApp.SaveChangesAsync();

            var command = new CriarUsuarioCommand(
                Nome: "Novo Usuário",
                Email: "duplicado@teste.com",
                Senha: "SenhaForte@2",
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput>
                {
                    new UsuarioEmpresaInput(Guid.NewGuid(), Guid.NewGuid(), false, "Vendedor", "Comercial", 10.0m)
                }
            );

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Já existe um usuário cadastrado com este e-mail."));
        }

        [Fact]
        public async Task CriarUsuario_Sem_Empresas_Deve_Retornar_Falha()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_user_no_companies");
            var mediator = provider.GetRequiredService<IMediator>();

            var command = new CriarUsuarioCommand(
                Nome: "Sem Empresa",
                Email: "semempresa@teste.com",
                Senha: "SenhaForte@1",
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput>() // Lista vazia
            );

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("possuir ao menos um vínculo"));
        }

        [Fact]
        public async Task CriarUsuario_Comum_Sem_Perfil_Deve_Retornar_Falha()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_user_no_profile");
            var mediator = provider.GetRequiredService<IMediator>();

            var command = new CriarUsuarioCommand(
                Nome: "Comum Sem Perfil",
                Email: "comum@teste.com",
                Senha: "SenhaForte@1",
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput>
                {
                    new UsuarioEmpresaInput(Guid.NewGuid(), null, false, "Vendedor", "Comercial", 0m) // EhAdmin: false, Perfil: null
                }
            );

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("perfil do usuário é obrigatório"));
        }

        [Fact]
        public async Task CriarUsuario_Admin_Sem_Perfil_Deve_Retornar_Sucesso()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_user_admin_no_profile");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var command = new CriarUsuarioCommand(
                Nome: "Admin Sem Perfil",
                Email: "admin@teste.com",
                Senha: "SenhaForte@1",
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput>
                {
                    new UsuarioEmpresaInput(Guid.NewGuid(), null, true, "Administrador", "TI", 100m) // EhAdmin: true, Perfil: null
                }
            );

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.True(result.Sucesso);

            var userDb = await contextApp.Usuarios.FirstOrDefaultAsync(u => u.Email == "admin@teste.com");
            Assert.NotNull(userDb);

            var vinculo = await contextApp.UsuariosEmpresas.FirstOrDefaultAsync(ue => ue.UsuarioId == userDb.Id);
            Assert.NotNull(vinculo);
            Assert.True(vinculo.EhAdmin);
            Assert.Null(vinculo.PerfilAcessoId);
        }

        [Fact]
        public async Task TrocarSenha_Administrativa_Deve_Persistir_Hash_E_Nunca_Texto_Puro()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_user_same_pass");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();
            var hasher = provider.GetRequiredService<IPasswordHasher>();

            var user = new Usuario("tenant-test", "Teste Senha", "senha@teste.com", hasher.Hash("SenhaOriginal"), UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(user);
            await contextApp.SaveChangesAsync();

            var command = new AlterarSenhaAdministrativaCommand(user.Id, "NovaSenha@123");

            // Act
            var result = await mediator.Send(command);

            // Assert: com hash salgado, a troca administrativa é aplicada e a senha é derivada (nunca em texto puro).
            Assert.True(result.Sucesso);

            var userDb = await contextApp.Usuarios.FirstAsync(u => u.Id == user.Id);
            Assert.NotEqual("NovaSenha@123", userDb.PasswordHash);
            Assert.True(hasher.Verify("NovaSenha@123", userDb.PasswordHash));
        }

        [Fact]
        public async Task DeletarUsuario_Deve_Realizar_Exclusao_Logica()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_user_soft_delete");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var user = new Usuario("tenant-test", "Delete Test", "delete@teste.com", "SenhaForte@1", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(user);

            var empId = Guid.NewGuid();
            var vinculo = new UsuarioEmpresa("tenant-test", user.Id, empId, null, true, "system"); // Admin da empresa
            var outroVinculo = new UsuarioEmpresa("tenant-test", Guid.NewGuid(), empId, null, true, "system"); // Outro Admin ativo para evitar a trava

            contextApp.UsuariosEmpresas.AddRange(vinculo, outroVinculo);
            await contextApp.SaveChangesAsync();

            var command = new DeletarUsuarioCommand(user.Id);

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.True(result.Sucesso);

            var userDb = await contextApp.Usuarios.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.NotNull(userDb);
            Assert.NotNull(userDb.DeletadoEm); // Deve ter data de deleção

            var vinculoDb = await contextApp.UsuariosEmpresas.IgnoreQueryFilters().FirstOrDefaultAsync(ue => ue.Id == vinculo.Id);
            Assert.NotNull(vinculoDb);
            Assert.NotNull(vinculoDb.DeletadoEm); // Vínculo também deve ter sofrido soft-delete
        }

        [Fact]
        public async Task DeletarUsuario_Ultimo_Admin_Deve_Retornar_Falha()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_user_delete_last_admin");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var user = new Usuario("tenant-test", "Delete Last Admin Test", "lastadmin@teste.com", "SenhaForte@1", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(user);

            var empId = Guid.NewGuid();
            var vinculo = new UsuarioEmpresa("tenant-test", user.Id, empId, null, true, "system"); // Único admin
            contextApp.UsuariosEmpresas.Add(vinculo);
            await contextApp.SaveChangesAsync();

            var command = new DeletarUsuarioCommand(user.Id);

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("único administrador ativo"));
        }

        [Fact]
        public async Task IniciarImpersonacao_MesmoUsuario_Deve_Retornar_Falha()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider("db_impersonate_self", "system", adminId.ToString());
            var mediator = provider.GetRequiredService<IMediator>();

            var command = new IniciarImpersonacaoCommand(adminId, null, "Impersonar a mim mesmo");

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Não é permitido impersonar a si mesmo"));
        }

        [Fact]
        public async Task IniciarImpersonacao_Valida_Deve_Gerar_Sessao_E_Tokens_Com_Sucesso()
        {
            // Arrange
            var adminId = Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider("db_impersonate_success", "system", adminId.ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var usuarioAlvo = new Usuario("tenant-alvo", "Alvo", "alvo@teste.com", "SenhaForte@1", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuarioAlvo);

            var vinculo = new UsuarioEmpresa("tenant-alvo", usuarioAlvo.Id, Guid.NewGuid(), null, false, "system"); // não-admin (admin é bloqueado pela salvaguarda)
            contextApp.UsuariosEmpresas.Add(vinculo);
            await contextApp.SaveChangesAsync();

            // Act 1: Iniciar Impersonação
            var commandIniciar = new IniciarImpersonacaoCommand(usuarioAlvo.Id, vinculo.EmpresaId, "Suporte Técnico");
            var resultIniciar = await mediator.Send(commandIniciar);

            // Assert 1
            Assert.True(resultIniciar.Sucesso);
            var dados = Assert.IsType<ImpersonacaoResultDto>(resultIniciar.Dados);
            Assert.NotNull(dados.Token);
            Assert.Equal(usuarioAlvo.Id, dados.UsuarioAlvoId);

            var sessao = await contextApp.SessoesImpersonacao.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == dados.SessaoImpersonacaoId);
            Assert.NotNull(sessao);
            Assert.Equal(adminId, sessao.UsuarioOriginalId);
            Assert.Equal(usuarioAlvo.Id, sessao.UsuarioAlvoId);
            Assert.Null(sessao.FimEm);

            // Act 2: Encerrar Impersonação
            var commandEncerrar = new EncerrarImpersonacaoCommand(sessao.Id);
            var resultEncerrar = await mediator.Send(commandEncerrar);

            // Assert 2
            Assert.True(resultEncerrar.Sucesso);
            var sessaoFim = await contextApp.SessoesImpersonacao.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == sessao.Id);
            Assert.NotNull(sessaoFim!.FimEm);
        }

        [Fact]
        public async Task AcessoSuporte_Valido_Deve_Gerar_Sessao_Escopada_Ao_Cliente()
        {
            // Arrange: operador Siser com perfil de suporte, na área Landlord (tenant "system").
            var operadorId = Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider("db_suporte_ok", "system", operadorId.ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var operador = new UsuarioInterno("Suporte Tec", "sup@siser.com", "SenhaForte@1", null, null, null, false, "system", "system", PerfilSuporteSiser.SuporteTecnico);
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(operador, operadorId);
            contextApp.UsuariosInternos.Add(operador);

            var usuarioAlvo = new Usuario("cliente-x", "Alvo Cliente", "alvo@cliente-x.com", "SenhaForte@1", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuarioAlvo);
            var vinculo = new UsuarioEmpresa("cliente-x", usuarioAlvo.Id, Guid.NewGuid(), null, false, "system"); // não-admin
            contextApp.UsuariosEmpresas.Add(vinculo);
            await contextApp.SaveChangesAsync();

            // Act
            var command = new IniciarAcessoSuporteCommand("cliente-x", usuarioAlvo.Id, vinculo.EmpresaId, "Investigar erro no cliente");
            var result = await mediator.Send(command);

            // Assert
            Assert.True(result.Sucesso);
            var dados = Assert.IsType<AcessoSuporteResultDto>(result.Dados);
            Assert.NotNull(dados.Token);
            Assert.Equal("cliente-x", dados.TenantAlvo);
            Assert.Equal("SuporteTecnico", dados.PerfilSuporte);

            var sessao = await contextApp.SessoesImpersonacao.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == dados.SessaoSuporteId);
            Assert.NotNull(sessao);
            Assert.Equal(TipoSessaoAcesso.SuporteTecnico, sessao!.TipoAcesso);
            Assert.Equal("cliente-x", sessao.TenantAlvo);
            Assert.Equal(operadorId, sessao.UsuarioOriginalId);

            // Auditoria via outbox gravada como suporte
            var outbox = await contextApp.OutboxMessages.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.EventType == "AcessoSuporteIniciado");
            Assert.NotNull(outbox);
        }

        [Fact]
        public async Task AcessoSuporte_Alvo_Admin_Deve_Ser_Bloqueado()
        {
            // Arrange
            var operadorId = Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider("db_suporte_admin", "system", operadorId.ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var operador = new UsuarioInterno("Suporte Neg", "neg@siser.com", "SenhaForte@1", null, null, null, false, "system", "system", PerfilSuporteSiser.SuporteNegocio);
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(operador, operadorId);
            contextApp.UsuariosInternos.Add(operador);

            var alvoAdmin = new Usuario("cliente-y", "Admin Cliente", "admin@cliente-y.com", "SenhaForte@1", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(alvoAdmin);
            var vinculoAdmin = new UsuarioEmpresa("cliente-y", alvoAdmin.Id, Guid.NewGuid(), null, true, "system"); // EhAdmin = true
            contextApp.UsuariosEmpresas.Add(vinculoAdmin);
            await contextApp.SaveChangesAsync();

            // Act
            var command = new IniciarAcessoSuporteCommand("cliente-y", alvoAdmin.Id, vinculoAdmin.EmpresaId, "Tentar suporte a admin");
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("ADMIN"));
        }

        [Fact]
        public async Task AcessoSuporte_Sem_Perfil_Suporte_Deve_Ser_Bloqueado()
        {
            // Arrange: operador Siser SEM perfil de suporte (Nenhum) e não admin primário.
            var operadorId = Guid.NewGuid();
            var (provider, _, _) = CreateServiceProvider("db_suporte_sem_perfil", "system", operadorId.ToString());
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var operador = new UsuarioInterno("Comum Siser", "comum@siser.com", "SenhaForte@1", null, null, null, false, "system", "system", PerfilSuporteSiser.Nenhum);
            typeof(Epros.Shared.Domain.Entities.EntidadeSaaSBase).GetField("<Id>k__BackingField", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.SetValue(operador, operadorId);
            contextApp.UsuariosInternos.Add(operador);

            var usuarioAlvo = new Usuario("cliente-z", "Alvo Z", "alvo@cliente-z.com", "SenhaForte@1", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuarioAlvo);
            var vinculo = new UsuarioEmpresa("cliente-z", usuarioAlvo.Id, Guid.NewGuid(), null, false, "system");
            contextApp.UsuariosEmpresas.Add(vinculo);
            await contextApp.SaveChangesAsync();

            // Act
            var command = new IniciarAcessoSuporteCommand("cliente-z", usuarioAlvo.Id, vinculo.EmpresaId, "Sem perfil");
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("perfil de suporte"));
        }

        [Fact]
        public async Task AtualizarPreferencias_Deve_Gravar_Com_Sucesso()
        {
            // Arrange
            var (provider, _, _) = CreateServiceProvider("db_user_preferences");
            var mediator = provider.GetRequiredService<IMediator>();
            var contextApp = provider.GetRequiredService<ContextAplicativo>();

            var user = new Usuario("tenant-test", "User Prefs", "prefs@teste.com", "SenhaForte@1", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(user);
            await contextApp.SaveChangesAsync();

            var command = new AtualizarPreferenciasUsuarioCommand(
                UsuarioId: user.Id,
                Idioma: "pt-BR",
                Tema: "Dark",
                Avatar: "avatar-image-data",
                RecebeNotificacoes: true,
                Timezone: "America/Sao_Paulo",
                FormatoData: "DD/MM/YYYY",
                FormatoHora: "24h",
                FormatoNumero: "1.000,00",
                PreferenciasJson: "{ \"zoom\": 1.2 }"
            );

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.True(result.Sucesso);

            var prefsDb = await contextApp.PreferenciasUsuarios.FirstOrDefaultAsync(p => p.UsuarioId == user.Id);
            Assert.NotNull(prefsDb);
            Assert.Equal("pt-BR", prefsDb.Idioma);
            Assert.Equal("Dark", prefsDb.Tema);
            Assert.Equal("avatar-image-data", prefsDb.Avatar);
            Assert.True(prefsDb.RecebeNotificacoes);
            Assert.Equal("America/Sao_Paulo", prefsDb.Timezone);
            Assert.Equal("DD/MM/YYYY", prefsDb.FormatoData);
        }

        #region Provedores de Teste
        public class TestLimitesValidador : IValidadorLimitesSaaS
        {
            public Task<bool> PossuiFolgaUsuariosAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult(true);
            public Task<bool> PossuiFolgaEmpresasAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult(true);
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteUsuariosAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((false, string.Empty));
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteEmpresasAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((false, string.Empty));
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
            public string? GetUserName() => "Test Admin User";
            public string? GetUserEmail() => "admin@epros.com";
        }
        #endregion
    }
}
