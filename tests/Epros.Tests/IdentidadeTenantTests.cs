using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Epros.API.Controllers;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Dtos;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Queries;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Epros.Tests
{
    public class IdentidadeTenantTests
    {
        private (ServiceProvider Provider, TestTenantProvider TenantProvider) CreateServiceProvider(string databaseName, string tenantInicial = "system")
        {
            var services = new ServiceCollection();

            // O fluxo de autenticação/RLS usa métodos relacionais (ExecuteSqlRaw, transações,
            // current_setting) que o provider InMemory não suporta. Usa-se um Postgres real e isolado.
            var connectionString = PostgresTestDb.CreateDatabase(databaseName);

            var tenantProvider = new TestTenantProvider(tenantInicial);
            var currentUser = new TestCurrentUser("system-test");

            var optAplicativo = PostgresTestDb.BuildOptions<ContextAplicativo>(connectionString, tenantProvider);
            var optGestaoClientes = PostgresTestDb.BuildOptions<ContextGestaoClientes>(connectionString, tenantProvider);

            services.AddSingleton(new ContextAplicativo(optAplicativo, tenantProvider, currentUser));
            services.AddSingleton(new ContextGestaoClientes(optGestaoClientes, tenantProvider, currentUser));
            services.AddSingleton<ITenantProvider>(tenantProvider);
            services.AddSingleton<ICurrentUser>(currentUser);
            services.AddSingleton<IPasswordHasher, Epros.Infrastructure.Services.Pbkdf2PasswordHasher>();
            services.AddSingleton<Epros.Shared.Security.IEprosTokenService>(
                new Epros.Shared.Security.EprosTokenService("epros-test-signing-key-0123456789-abcdef"));
            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();

            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(AutenticarUsuarioCommandHandler).Assembly);
            });

            return (services.BuildServiceProvider(), tenantProvider);
        }

        private static readonly IPasswordHasher _hasher = new Epros.Infrastructure.Services.Pbkdf2PasswordHasher();

        [Fact]
        public async Task AutenticarUsuario_Com_Email_Inexistente_Deve_Falhar_Com_Mensagem_Generica()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_auth_inexistente", "tenant-test");
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var command = new AutenticarUsuarioCommand("naoexiste@teste.com", "senha123", "127.0.0.1", "Chrome");

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains("E-mail ou senha incorretos.", result.Erros);
        }

        [Fact]
        public async Task AutenticarUsuario_Com_Senha_Incorreta_Deve_Registrar_Tentativa_E_Falhar_Com_Mensagem_Generica()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_auth_senha_errada", "tenant-test");
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            // Cria usuário de teste
            var usuario = new Usuario("tenant-test", "João Silva", "joao@teste.com", _hasher.Hash("senhaCorreta123"), UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);
            await contextApp.SaveChangesAsync();

            var command = new AutenticarUsuarioCommand("joao@teste.com", "senhaIncorreta", "127.0.0.1", "Chrome");

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains("E-mail ou senha incorretos.", result.Erros);

            // Verifica incremento de falhas e registro de auditoria
            var usuarioDb = await contextApp.Usuarios.FirstAsync(u => u.Id == usuario.Id);
            Assert.Equal(1, usuarioDb.AccessFailedCount);

            var audit = await contextApp.HistoricosLogin.AnyAsync(h => h.UsuarioId == usuario.Id && !h.Sucesso);
            Assert.True(audit);
        }

        [Fact]
        public async Task AutenticarUsuario_Excedendo_Tentativas_Deve_Bloquear_Por_Lockout()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_auth_lockout", "tenant-test");
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var usuario = new Usuario("tenant-test", "Maria Souza", "maria@teste.com", _hasher.Hash("senhaMestre"), UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);
            await contextApp.SaveChangesAsync();

            var command = new AutenticarUsuarioCommand("maria@teste.com", "senhaErrada", "127.0.0.1", "Chrome");

            // Act: Tenta 5 vezes com a senha errada
            for (int i = 0; i < 5; i++)
            {
                await mediator.Send(command);
            }

            // Tenta a 6ª vez
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains("Conta temporariamente bloqueada.", result.Erros);

            var usuarioDb = await contextApp.Usuarios.FirstAsync(u => u.Id == usuario.Id);
            Assert.True(usuarioDb.LockoutEnd.HasValue && usuarioDb.LockoutEnd.Value > DateTime.UtcNow);
        }

        [Fact]
        public async Task AutenticarUsuario_Com_Senha_Correta_Deve_Zerar_Falhas_E_Logar_Com_Sucesso()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_auth_sucesso", "tenant-test");
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var usuario = new Usuario("tenant-test", "Carlos Souza", "carlos@teste.com", _hasher.Hash("senhaCerta123"), UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);
            await contextApp.SaveChangesAsync();

            // Simula falha anterior
            var commandFalha = new AutenticarUsuarioCommand("carlos@teste.com", "senhaErrada", "127.0.0.1", "Chrome");
            await mediator.Send(commandFalha);

            var commandSucesso = new AutenticarUsuarioCommand("carlos@teste.com", "senhaCerta123", "127.0.0.1", "Chrome");

            // Act
            var result = await mediator.Send(commandSucesso);

            // Assert
            Assert.True(result.Sucesso);
            var dto = Assert.IsType<AuthResponseDto>(result.Dados);
            Assert.Equal(usuario.Id, dto.UsuarioId);
            Assert.Equal("tenant-test", dto.TenantId);

            // Verifica que limpou contadores
            var usuarioDb = await contextApp.Usuarios.FirstAsync(u => u.Id == usuario.Id);
            Assert.Equal(0, usuarioDb.AccessFailedCount);

            // Verifica se gerou sessão no banco
            var temSessao = await contextApp.SessoesUsuarios.AnyAsync(s => s.UsuarioId == usuario.Id && !s.Revogado);
            Assert.True(temSessao);
        }

        [Fact]
        public async Task SelecionarEmpresa_Com_Empresa_Vinculada_Deve_Retornar_Token_Completo()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_auth_selecao", "tenant-test");
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var tokenService = serviceProvider.GetRequiredService<Epros.Shared.Security.IEprosTokenService>();

            var usuario = new Usuario("tenant-test", "Pedro Reis", "pedro@teste.com", "senhaReis", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);

            var empresaId = Guid.NewGuid();
            var vinculo = new UsuarioEmpresa("tenant-test", usuario.Id, empresaId, Guid.NewGuid(), false, "system");
            contextApp.UsuariosEmpresas.Add(vinculo);

            await contextApp.SaveChangesAsync();

            var command = new SelecionarEmpresaCommand(usuario.Id, empresaId);

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.True(result.Sucesso);
            var dto = Assert.IsType<AuthResponseDto>(result.Dados);

            // Token agora é um JWT assinado: validamos via serviço e conferimos os claims.
            var principal = tokenService.Validar(dto.Token);
            Assert.NotNull(principal);
            Assert.Equal(usuario.Id.ToString(), principal!.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            Assert.Equal("tenant-test", principal.FindFirst("tenantId")?.Value);
            Assert.Equal(empresaId.ToString(), principal.FindFirst("empresaId")?.Value);
        }

        [Fact]
        public async Task SelecionarEmpresa_Com_Empresa_Nao_Vinculada_Deve_Retornar_Falha()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_auth_selecao_invalida", "tenant-test");
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var usuario = new Usuario("tenant-test", "Carla Dias", "carla@teste.com", "senhaDias", UsuarioTipo.Company, "system");
            contextApp.Usuarios.Add(usuario);
            await contextApp.SaveChangesAsync();

            var command = new SelecionarEmpresaCommand(usuario.Id, Guid.NewGuid());

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains("O usuário não possui permissão de acesso à empresa informada.", result.Erros);
        }

        [Fact]
        public async Task ResetarSenha_Com_Token_Valido_Deve_Alterar_Com_Sucesso()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_auth_reset", "tenant-test");
            var contextApp = serviceProvider.GetRequiredService<ContextAplicativo>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var usuario = new Usuario("tenant-test", "Juliana Faria", "juliana@teste.com", _hasher.Hash("senhaFaria"), UsuarioTipo.Company, "system");
            usuario.GerarTokenRecuperacaoSenha("tokenReset123", TimeSpan.FromHours(3));
            contextApp.Usuarios.Add(usuario);
            await contextApp.SaveChangesAsync();

            var command = new ResetarSenhaCommand("juliana@teste.com", "tokenReset123", "novaSenhaForte1", "novaSenhaForte1");

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.True(result.Sucesso);

            var usuarioDb = await contextApp.Usuarios.FirstAsync(u => u.Id == usuario.Id);
            Assert.True(_hasher.Verify("novaSenhaForte1", usuarioDb.PasswordHash));
            Assert.NotEqual("novaSenhaForte1", usuarioDb.PasswordHash); // nunca em texto puro
            Assert.Null(usuarioDb.ForgotPasswordToken);
        }

        [Fact]
        public async Task RegistrarNovoTenant_Com_Cnpj_Duplicado_Deve_Bloquear()
        {
            // Arrange
            var (serviceProvider, _) = CreateServiceProvider("db_auth_tenant_dup", "system");
            var contextGestao = serviceProvider.GetRequiredService<ContextGestaoClientes>();
            var mediator = serviceProvider.GetRequiredService<IMediator>();

            var endereco = new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco("Logradouro", "123", null, "Centro", "11111111", "Cianorte", "PR");
            // CNPJ válido (dígito verificador correto) para exercitar a checagem de DUPLICIDADE — a
            // validação de CNPJ inválido é anterior e barraria um CNPJ com DV errado antes daqui.
            var empresaExistente = new Empresa("Razao Existente", "Fantasia", "12345678000195", null, null, null, null, RegimeTributario.SimplesNacional, RegimeApuracao.Cumulativo, null, null, null, null, null, null, null, null, null, null, endereco, "tenant-ex", "system");
            contextGestao.Empresas.Add(empresaExistente);
            await contextGestao.SaveChangesAsync();

            var command = new RegistrarNovoTenantCommand(
                "Nova Empresa S/A", "12345678000195", "Admin", "admin@nova.com", "senhaForte1",
                CodigoIbgeMunicipio: 4105508, Telefone: "44999990000", TipoTelefone: "Celular");

            // Act
            var result = await mediator.Send(command);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains("Já existe uma empresa cadastrada com este CNPJ.", result.Erros);
        }

        #region Provedores de Teste
        public class TestTenantProvider : ITenantProvider
        {
            public string TenantId { get; set; }
            public TestTenantProvider(string tenantId) => TenantId = tenantId;
            public string GetTenantId() => TenantId;
        }

        private class TestCurrentUser : ICurrentUser
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
