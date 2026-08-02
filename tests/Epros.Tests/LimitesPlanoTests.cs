using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Application.Services;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;

namespace Epros.Tests
{
    public class LimitesPlanoTests
    {
        private static readonly IPasswordHasher _hasher = new Epros.Infrastructure.Services.Pbkdf2PasswordHasher();

        #region Testes de Limite de Usuários

        [Fact]
        public async Task Deve_Criar_Usuario_Com_Sucesso_Se_Dentro_Do_Limite_De_Usuarios()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-usuarios-dentro";
            var userId = "user-criador";

            var (contextApp, contextGestao) = CreateInMemoryContexts(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Setup: Cliente e Plano contratado com limite de 2 usuários
            var plano = new Plano(
                nome: "Plano Especial",
                preco: 199.90m,
                grupoPlanoId: null,
                limiteUsuarios: 2, // Limite de 2
                limiteEmpresas: 1,
                recursosInclusos: null,
                tenantId: tenantId,
                criadoPor: userId
            );
            contextGestao.Planos.Add(plano);

            var cliente = new Cliente("Empresa Cliente", "00.000.000/0001-00", "cliente@limites.com", plano.Id, tenantId, userId);
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            // Adiciona o primeiro usuário no aplicativo
            var usuario1 = new Usuario(tenantId, "Usuario Um", "user1@epros.com", "senha123", UsuarioTipo.Company, userId);
            contextApp.Usuarios.Add(usuario1);
            await contextApp.SaveChangesAsync();

            var validadorLimites = new ValidadorLimitesSaaS(contextApp, contextGestao);
            var handler = new CriarUsuarioCommandHandler(contextApp, tenantProvider, currentUser, validadorLimites, _hasher);

            // Act: Tenta criar o segundo usuário (dentro do limite de 2)
            var command = new CriarUsuarioCommand(
                Nome: "Usuario Dois",
                Email: "user2@epros.com",
                Senha: "senha123",
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput> { new UsuarioEmpresaInput(Guid.NewGuid(), Guid.NewGuid(), false, "Analista", "TI", 0) }
            );
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var totalUsuariosApp = await contextApp.Usuarios.CountAsync(u => u.TenantId == tenantId && u.Status == UsuarioStatus.Active);
            Assert.Equal(2, totalUsuariosApp);
        }

        [Fact]
        public async Task Deve_Bloquear_Criacao_De_Usuario_Se_Limite_Atingido()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-usuarios-excedido";
            var userId = "user-criador";

            var (contextApp, contextGestao) = CreateInMemoryContexts(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Setup: Cliente e Plano com limite de 1 usuário
            var plano = new Plano(
                nome: "Plano Light",
                preco: 99.90m,
                grupoPlanoId: null,
                limiteUsuarios: 1, // Limite de 1 usuário ativo
                limiteEmpresas: 1,
                recursosInclusos: null,
                tenantId: tenantId,
                criadoPor: userId
            );
            contextGestao.Planos.Add(plano);

            var cliente = new Cliente("Empresa Cliente", "00.000.000/0001-00", "cliente@limites.com", plano.Id, tenantId, userId);
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            // Adiciona o primeiro usuário ativo
            var usuario1 = new Usuario(tenantId, "Usuario Um", "user1@epros.com", "senha123", UsuarioTipo.Company, userId);
            contextApp.Usuarios.Add(usuario1);
            await contextApp.SaveChangesAsync();

            var validadorLimites = new ValidadorLimitesSaaS(contextApp, contextGestao);
            var handler = new CriarUsuarioCommandHandler(contextApp, tenantProvider, currentUser, validadorLimites, _hasher);

            // Act: Tenta criar o segundo usuário (excede o limite de 1)
            var command = new CriarUsuarioCommand(
                Nome: "Usuario Dois",
                Email: "user2@epros.com",
                Senha: "senha123",
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput> { new UsuarioEmpresaInput(Guid.NewGuid(), Guid.NewGuid(), false, "Cargo", "Depto", 0) }
            );
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("limite de usuários ativos contratados em seu plano foi atingido"));
        }

        [Fact]
        public async Task Deve_Bloquear_Ativacao_De_Usuario_Se_Limite_De_Usuarios_Atingido()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-ativar-usuarios";
            var userId = "user-criador";

            var (contextApp, contextGestao) = CreateInMemoryContexts(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Setup: Cliente e Plano com limite de 1 usuário ativo
            var plano = new Plano("Plano Light", 99.90m, null, 1, 1, null, tenantId, userId);
            contextGestao.Planos.Add(plano);

            var cliente = new Cliente("Empresa Cliente", "00.000.000/0001-00", "cliente@limites.com", plano.Id, tenantId, userId);
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            // Adiciona o primeiro usuário que já está ativo
            var usuarioAtivo = new Usuario(tenantId, "Usuario Ativo", "ativo@epros.com", "senha123", UsuarioTipo.Company, userId);
            contextApp.Usuarios.Add(usuarioAtivo);

            // Adiciona um segundo usuário como inativo/disabled
            var usuarioInativo = new Usuario(tenantId, "Usuario Inativo", "inativo@epros.com", "senha123", UsuarioTipo.Company, userId);
            usuarioInativo.Bloquear(userId); // Disabled
            contextApp.Usuarios.Add(usuarioInativo);

            await contextApp.SaveChangesAsync();

            var validadorLimites = new ValidadorLimitesSaaS(contextApp, contextGestao);
            var handler = new AtualizarUsuarioCommandHandler(contextApp, tenantProvider, currentUser, validadorLimites);

            // Act: Tenta reativar o usuário inativo (o que faria ultrapassar o limite de 1 ativo)
            var command = new AtualizarUsuarioCommand(
                UsuarioId: usuarioInativo.Id,
                Nome: "Usuario Inativo",
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active, // Reativação
                Empresas: new List<UsuarioEmpresaInput> { new UsuarioEmpresaInput(Guid.NewGuid(), Guid.NewGuid(), false, "Cargo", "Depto", 0) }
            );
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("limite de usuários ativos contratados em seu plano foi atingido"));
        }

        #endregion

        #region Testes de Limite de Empresas

        [Fact]
        public async Task Deve_Criar_Empresa_Com_Sucesso_Se_Dentro_Do_Limite_De_Empresas()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-empresas-dentro";
            var userId = "user-criador";

            var (contextApp, contextGestao) = CreateInMemoryContexts(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Setup: Cliente e Plano com limite de 2 empresas
            var plano = new Plano(
                nome: "Plano Pro",
                preco: 299.90m,
                grupoPlanoId: null,
                limiteUsuarios: 5,
                limiteEmpresas: 2, // Limite de 2 empresas
                recursosInclusos: null,
                tenantId: tenantId,
                criadoPor: userId
            );
            contextGestao.Planos.Add(plano);

            var cliente = new Cliente("Empresa Cliente", "00.000.000/0001-00", "cliente@limites.com", plano.Id, tenantId, userId);
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            // Adiciona a primeira empresa
            var empresa1 = new Empresa(
                "Razao Social 1",
                "Fantasia 1",
                "11.111.111/0001-11",
                "IE",
                "IM",
                null,
                null,
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco("L", "N", null, "B", "C", "C", "E"),
                tenantId,
                userId
            );
            contextGestao.Empresas.Add(empresa1);
            await contextGestao.SaveChangesAsync();

            var validadorLimites = new ValidadorLimitesSaaS(contextApp, contextGestao);
            var handler = new CriarEmpresaCommandHandler(contextGestao, tenantProvider, currentUser, validadorLimites, new IdentityCofrePixLp());

            // Act: Tenta criar a segunda empresa
            var command = new CriarEmpresaCommand(
                RazaoSocial: "Razao Social 2",
                NomeFantasia: "Fantasia 2",
                Cnpj: "12345678000195", // Cnpj válido
                InscricaoEstadual: "IE",
                InscricaoMunicipal: null,
                InscricaoSuframa: null,
                Cnae: null,
                RegimeTributario: RegimeTributario.SimplesNacional,
                RegimeApuracao: RegimeApuracao.Cumulativo,
                PessoaGrupoId: null,
                ProdutoGrupoId: null,
                PlanoContasFinanceiroId: null,
                TributarioGrupoId: null,
                NcmTributacaoId: null,
                CertificadoDigitalId: null,
                EmpresaParametrosDfeId: null,
                LinkWebApiAppVendas: null,
                TokenMercadoPagoPix: null,
                Logo: null,
                Endereco: new EmpresaEnderecoDto("Logradouro", "123", null, "Bairro", "12345-678", "Cidade", "SP")
            );
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var totalEmpresas = await contextGestao.Empresas.CountAsync(e => e.TenantId == tenantId);
            Assert.Equal(2, totalEmpresas);
        }

        [Fact]
        public async Task Deve_Bloquear_Criacao_De_Empresa_Se_Limite_Atingido()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-empresas-excedido";
            var userId = "user-criador";

            var (contextApp, contextGestao) = CreateInMemoryContexts(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Setup: Cliente e Plano com limite de 1 empresa
            var plano = new Plano(
                nome: "Plano Start",
                preco: 149.90m,
                grupoPlanoId: null,
                limiteUsuarios: 5,
                limiteEmpresas: 1, // Limite de 1 empresa
                recursosInclusos: null,
                tenantId: tenantId,
                criadoPor: userId
            );
            contextGestao.Planos.Add(plano);

            var cliente = new Cliente("Empresa Cliente", "00.000.000/0001-00", "cliente@limites.com", plano.Id, tenantId, userId);
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            // Adiciona a primeira empresa
            var empresa1 = new Empresa(
                "Razao Social 1",
                "Fantasia 1",
                "11.111.111/0001-11",
                "IE",
                "IM",
                null,
                null,
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                new Epros.Modules.GestaoClientes.Domain.ValueObjects.Endereco("L", "N", null, "B", "C", "C", "E"),
                tenantId,
                userId
            );
            contextGestao.Empresas.Add(empresa1);
            await contextGestao.SaveChangesAsync();

            var validadorLimites = new ValidadorLimitesSaaS(contextApp, contextGestao);
            var handler = new CriarEmpresaCommandHandler(contextGestao, tenantProvider, currentUser, validadorLimites, new IdentityCofrePixLp());

            // Act: Tenta criar a segunda empresa
            var command = new CriarEmpresaCommand(
                RazaoSocial: "Razao Social 2",
                NomeFantasia: "Fantasia 2",
                Cnpj: "12345678000195", // Cnpj válido
                InscricaoEstadual: "IE",
                InscricaoMunicipal: null,
                InscricaoSuframa: null,
                Cnae: null,
                RegimeTributario: RegimeTributario.SimplesNacional,
                RegimeApuracao: RegimeApuracao.Cumulativo,
                PessoaGrupoId: null,
                ProdutoGrupoId: null,
                PlanoContasFinanceiroId: null,
                TributarioGrupoId: null,
                NcmTributacaoId: null,
                CertificadoDigitalId: null,
                EmpresaParametrosDfeId: null,
                LinkWebApiAppVendas: null,
                TokenMercadoPagoPix: null,
                Logo: null,
                Endereco: new EmpresaEnderecoDto("Logradouro", "123", null, "Bairro", "12345-678", "Cidade", "SP")
            );
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("limite de empresas cadastradas contratadas em seu plano foi atingido"));
        }

        #endregion

        #region Testes de Ilimitado (Recurso <= 0)

        [Fact]
        public async Task Deve_Criar_Usuario_Livremente_Se_Plano_For_Ilimitado()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var tenantId = "tenant-ilimitado";
            var userId = "user-criador";

            var (contextApp, contextGestao) = CreateInMemoryContexts(dbName, tenantId, userId);
            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            // Setup: Cliente e Plano com limiteUsuarios = 0 (Ilimitado)
            var plano = new Plano(
                nome: "Plano Custom Enterprise",
                preco: 1999.90m,
                grupoPlanoId: null,
                limiteUsuarios: 0, // 0 = Ilimitado
                limiteEmpresas: 0,  // 0 = Ilimitado
                recursosInclusos: null,
                tenantId: tenantId,
                criadoPor: userId
            );
            contextGestao.Planos.Add(plano);

            var cliente = new Cliente("Empresa Cliente", "00.000.000/0001-00", "cliente@limites.com", plano.Id, tenantId, userId);
            contextGestao.Clientes.Add(cliente);
            await contextGestao.SaveChangesAsync();

            // Adiciona 3 usuários ativos
            for (int i = 1; i <= 3; i++)
            {
                var user = new Usuario(tenantId, $"Usuario {i}", $"user{i}@epros.com", "senha123", UsuarioTipo.Company, userId);
                contextApp.Usuarios.Add(user);
            }
            await contextApp.SaveChangesAsync();

            var validadorLimites = new ValidadorLimitesSaaS(contextApp, contextGestao);
            var handler = new CriarUsuarioCommandHandler(contextApp, tenantProvider, currentUser, validadorLimites, _hasher);

            // Act: Cria o quarto usuário
            var command = new CriarUsuarioCommand(
                Nome: "Usuario Quatro",
                Email: "user4@epros.com",
                Senha: "senha123",
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput> { new UsuarioEmpresaInput(Guid.NewGuid(), Guid.NewGuid(), false, "TI", "TI", 0) }
            );
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert: Deve permitir com sucesso já que o limite é zero (ilimitado)
            Assert.True(result.Sucesso);
        }

        #endregion

        #region Helpers

        private (ContextAplicativo ContextApp, ContextGestaoClientes ContextGestao) CreateInMemoryContexts(string dbName, string tenantId, string userId)
        {
            var optionsApp = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var optionsGestao = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(dbName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return (
                new ContextAplicativo(optionsApp, tenantProvider, currentUser),
                new ContextGestaoClientes(optionsGestao, tenantProvider, currentUser)
            );
        }

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
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }

        private sealed class IdentityCofrePixLp : ISegredoCofreService
        {
            public Task<string> CriptografarAsync(string valor) => Task.FromResult("enc:" + valor);
            public Task<string> DescriptografarAsync(string ciphertext) => Task.FromResult(ciphertext);
        }

        #endregion
    }
}
