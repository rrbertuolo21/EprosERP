using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Infrastructure.Services;
using Epros.Modules.Aplicativo.Application.Commands;
using Epros.Modules.Aplicativo.Application.Handlers;
using Epros.Modules.Aplicativo.Domain.Entities;
using Epros.Modules.Aplicativo.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Cobertura do fix crítico de segurança: senhas nunca são armazenadas nem comparadas em texto puro.
    /// </summary>
    public class PasswordHasherTests
    {
        private readonly IPasswordHasher _hasher = new Pbkdf2PasswordHasher();

        [Fact]
        public void Hash_Nao_Deve_Retornar_A_Senha_Em_Texto_Puro()
        {
            const string senha = "MinhaSenhaSecreta@123";

            var hash = _hasher.Hash(senha);

            Assert.NotNull(hash);
            Assert.NotEqual(senha, hash);
            Assert.DoesNotContain(senha, hash);
            // Formato autocontido esperado: pbkdf2.sha256.<iteracoes>.<salt>.<hash>
            Assert.StartsWith("pbkdf2.sha256.", hash);
            Assert.Equal(5, hash.Split('.').Length);
        }

        [Fact]
        public void Hash_Deve_Usar_Salt_Aleatorio_Gerando_Hashes_Diferentes_Para_Mesma_Senha()
        {
            const string senha = "SenhaRepetida@123";

            var hash1 = _hasher.Hash(senha);
            var hash2 = _hasher.Hash(senha);

            // Salt aleatório => hashes distintos, mesmo para a mesma senha.
            Assert.NotEqual(hash1, hash2);

            // Ainda assim, ambos devem verificar corretamente.
            Assert.True(_hasher.Verify(senha, hash1));
            Assert.True(_hasher.Verify(senha, hash2));
        }

        [Fact]
        public void Verify_Deve_Retornar_True_Para_Senha_Correta()
        {
            const string senha = "SenhaCorreta@123";
            var hash = _hasher.Hash(senha);

            Assert.True(_hasher.Verify(senha, hash));
        }

        [Fact]
        public void Verify_Deve_Retornar_False_Para_Senha_Incorreta()
        {
            var hash = _hasher.Hash("SenhaCorreta@123");

            Assert.False(_hasher.Verify("SenhaErrada@123", hash));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("123")]                       // formato legado/texto puro (bug antigo)
        [InlineData("pbkdf2.sha256.100000")]      // partes insuficientes
        [InlineData("pbkdf2.sha512.100000.abc.def")] // algoritmo não suportado
        [InlineData("pbkdf2.sha256.naoNumero.abc.def")] // iterações inválidas
        [InlineData("pbkdf2.sha256.100000.$$$.$$$")]    // base64 inválido
        [InlineData("bcrypt.sha256.100000.abc.def")]    // prefixo desconhecido
        public void Verify_Deve_Retornar_False_Sem_Lancar_Para_Formato_Invalido_Ou_Legado(string hashArmazenado)
        {
            var ex = Record.Exception(() => _hasher.Verify("qualquerSenha", hashArmazenado));

            Assert.Null(ex); // não lança
            Assert.False(_hasher.Verify("qualquerSenha", hashArmazenado));
        }

        [Fact]
        public void Verify_Deve_Retornar_False_Para_Hash_Nulo()
        {
            Assert.False(_hasher.Verify("qualquerSenha", null!));
        }

        [Fact]
        public async Task CriarUsuario_Deve_Gravar_PasswordHash_Diferente_Da_Senha_Crua()
        {
            // Arrange
            var dbName = "db_password_hash_" + Guid.NewGuid();
            var tenantId = "tenant-hash-test";
            var userId = "criador-teste";

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            var options = new DbContextOptionsBuilder<ContextAplicativo>()
                .UseInMemoryDatabase(dbName)
                .Options;

            using var contextApp = new ContextAplicativo(options, tenantProvider, currentUser);

            var handler = new CriarUsuarioCommandHandler(
                contextApp,
                tenantProvider,
                currentUser,
                new AlwaysAllowLimitesValidador(),
                _hasher);

            const string senhaCrua = "SenhaDoUsuario@123";
            var command = new CriarUsuarioCommand(
                Nome: "Usuário Teste",
                Email: "usuario.hash@teste.com",
                Senha: senhaCrua,
                Telefone: null,
                Tipo: UsuarioTipo.Company,
                Status: UsuarioStatus.Active,
                Empresas: new List<UsuarioEmpresaInput>
                {
                    new UsuarioEmpresaInput(Guid.NewGuid(), null, true, "Administrador", "TI", 0m)
                });

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            var usuarioDb = await contextApp.Usuarios.FirstAsync(u => u.Email == "usuario.hash@teste.com");

            // O campo PasswordHash NUNCA pode conter a senha crua.
            Assert.NotEqual(senhaCrua, usuarioDb.PasswordHash);
            Assert.DoesNotContain(senhaCrua, usuarioDb.PasswordHash);
            Assert.StartsWith("pbkdf2.sha256.", usuarioDb.PasswordHash);

            // E o hash gravado deve verificar corretamente contra a senha crua.
            Assert.True(_hasher.Verify(senhaCrua, usuarioDb.PasswordHash));
            Assert.False(_hasher.Verify("outraSenha", usuarioDb.PasswordHash));
        }

        #region Provedores de Teste

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
            public string? GetUserName() => "Teste";
            public string? GetUserEmail() => "teste@epros.com";
        }

        private class AlwaysAllowLimitesValidador : IValidadorLimitesSaaS
        {
            public Task<bool> PossuiFolgaUsuariosAsync(string tenantId, CancellationToken cancellationToken = default)
                => Task.FromResult(true);

            public Task<bool> PossuiFolgaEmpresasAsync(string tenantId, CancellationToken cancellationToken = default)
                => Task.FromResult(true);

            public Task<(bool Excedido, string Mensagem)> ValidarLimiteUsuariosAsync(string tenantId, CancellationToken cancellationToken = default)
                => Task.FromResult((false, string.Empty));

            public Task<(bool Excedido, string Mensagem)> ValidarLimiteEmpresasAsync(string tenantId, CancellationToken cancellationToken = default)
                => Task.FromResult((false, string.Empty));

            public Task<(bool Excedido, string Mensagem)> ValidarLimiteClientesAsync(string tenantId, CancellationToken cancellationToken = default)
                => Task.FromResult((false, string.Empty));

            public Task<(bool Excedido, string Mensagem)> ValidarLimitePermissoesAsync(string tenantId, CancellationToken cancellationToken = default)
                => Task.FromResult((false, string.Empty));
        }

        #endregion
    }
}
