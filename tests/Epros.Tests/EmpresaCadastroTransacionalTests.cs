using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Domain.ValueObjects;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;

namespace Epros.Tests
{
    public class EmpresaCadastroTransacionalTests
    {
        private const string TenantId = "tenant-transacao-test";
        private const string Usuario = "user-transacao-test";

        private ContextGestaoClientes CreateInMemoryContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(Usuario);

            return new ContextGestaoClientes(options, tenantProvider, currentUser);
        }

        [Fact]
        public async Task Deve_Inicializar_Primeira_Empresa_Com_Parametros_E_Configuracoes_Com_Sucesso()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_empresa_transacao_success");

            // Seed a plano system-wide to be selected by the handler
            var planoMaster = new Plano("Plano Teste Master", 150m, "system", "system");
            context.Planos.Add(planoMaster);
            await context.SaveChangesAsync();

            var limitValidator = new FakeValidadorLimitesSaaS(false, "");
            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(Usuario);

            var handler = new CriarEmpresaCommandHandler(context, tenantProvider, currentUser, limitValidator);

            var enderecoDto = new EmpresaEnderecoDto("Rua das Oliveiras", "123", "Sala 2", "Centro", "13010-000", "Campinas", "SP");
            var command = new CriarEmpresaCommand(
                "Empresa Transacional S/A",
                "Transacional",
                "12345678000195", // Valid CNPJ
                "123.456.789",
                "987.654",
                "Suframa123",
                "6201501",
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null, null, null, null, null, null, null,
                "https://api.vendas.com",
                "mp-token-123",
                "https://logo.com/logo.png",
                enderecoDto,
                "DD-MM-YYYY",
                Guid.NewGuid(),
                Guid.NewGuid()
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            // Verify all 4 entities are created
            var empresa = await context.Empresas.FirstOrDefaultAsync(e => e.TenantId == TenantId);
            Assert.NotNull(empresa);
            Assert.Equal("Empresa Transacional S/A", empresa.RazaoSocial);

            var upgradePlano = await context.UpgradesPlanos.FirstOrDefaultAsync(u => u.TenantId == TenantId);
            Assert.NotNull(upgradePlano);
            Assert.True(upgradePlano.IsActive);
            Assert.Equal(planoMaster.Id, upgradePlano.PlanoId);

            var exercicio = await context.ExerciciosFinanceiros.FirstOrDefaultAsync(ex => ex.TenantId == TenantId);
            Assert.NotNull(exercicio);
            Assert.Equal("Aberto", exercicio.Status);
            Assert.True(exercicio.ToDate > exercicio.FromDate);

            var preferencias = await context.PreferenciasGerais.FirstOrDefaultAsync(p => p.TenantId == TenantId);
            Assert.NotNull(preferencias);
            Assert.True(preferencias.NegativeCash);
            Assert.True(preferencias.NegativeStock);
            Assert.Equal(StockCalculationMode.CustoMedio, preferencias.StockCalculationMode);
        }

        [Fact]
        public async Task Deve_Retornar_Falha_Se_Exceder_Limite_De_Empresas()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_empresa_transacao_limit");
            var limitValidator = new FakeValidadorLimitesSaaS(true, "Limite atingido");
            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(Usuario);

            var handler = new CriarEmpresaCommandHandler(context, tenantProvider, currentUser, limitValidator);

            var enderecoDto = new EmpresaEnderecoDto("Rua das Oliveiras", "123", null, "Centro", "13010-000", "Campinas", "SP");
            var command = new CriarEmpresaCommand(
                "Empresa Falha Limite S/A",
                "Limite",
                "12345678000195",
                null, null, null, null,
                RegimeTributario.SimplesNacional,
                RegimeApuracao.Cumulativo,
                null, null, null, null, null, null, null, null, null, null,
                enderecoDto
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Limite de empresas excedido") || e.Contains("Limite atingido"));

            var empresa = await context.Empresas.FirstOrDefaultAsync(e => e.TenantId == TenantId);
            Assert.Null(empresa);
        }

        #region Helpers
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

        private class FakeValidadorLimitesSaaS : IValidadorLimitesSaaS
        {
            private readonly bool _excedeu;
            private readonly string _mensagem;

            public FakeValidadorLimitesSaaS(bool excedeu, string mensagem)
            {
                _excedeu = excedeu;
                _mensagem = mensagem;
            }

            public Task<bool> PossuiFolgaUsuariosAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult(!_excedeu);
            public Task<bool> PossuiFolgaEmpresasAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult(!_excedeu);
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteUsuariosAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((_excedeu, _mensagem));
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteEmpresasAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((_excedeu, _mensagem));
            public Task<(bool Excedido, string Mensagem)> ValidarLimiteClientesAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((_excedeu, _mensagem));
            public Task<(bool Excedido, string Mensagem)> ValidarLimitePermissoesAsync(string tenantId, CancellationToken cancellationToken = default) => Task.FromResult((_excedeu, _mensagem));
        }
        #endregion
    }
}
