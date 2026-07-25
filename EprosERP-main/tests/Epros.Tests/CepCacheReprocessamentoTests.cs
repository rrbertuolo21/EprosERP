using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    public class CepCacheReprocessamentoTests
    {
        private const string TenantId = "tenant-cep-test";
        private const string UsuarioId = "user-cep-test";

        private ContextGestaoClientes CreateInMemoryContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UsuarioId);

            return new ContextGestaoClientes(options, tenantProvider, currentUser);
        }

        [Fact]
        public async Task Deve_Registrar_Cep_Manualmente_Com_Justificativa_Com_Sucesso()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_cep_manual_sucesso");

            // Seed Pais e Municipio
            var pais = new Pais("Brasil", "BR", "BRA", "076", "Brasília", "+55", "system");
            context.Paises.Add(pais);
            await context.SaveChangesAsync();

            var subdivisao = new Subdivisao(pais.Id, "BR-SP", "São Paulo", TipoSubdivisao.Estado, null, "system");
            context.Subdivisoes.Add(subdivisao);
            await context.SaveChangesAsync();

            var municipio = new Municipio(pais.Id, subdivisao.Id, "Campinas", 3509502, null, null, "system");
            context.Municipios.Add(municipio);
            await context.SaveChangesAsync();

            var currentUser = new TestCurrentUser(UsuarioId);
            var handler = new RegistrarCepManualCommandHandler(context, currentUser);

            var command = new RegistrarCepManualCommand(
                Cep: "13083-851",
                Logradouro: "Rua do Registro Manual",
                Bairro: "Bairro Teste",
                MunicipioId: municipio.Id,
                Uf: "SP",
                Justificativa: "CEP novo não localizado na base nacional do Correios"
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            // Verificar se o cache foi gravado corretamente
            var cache = await context.CodigosPostaisCache
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.CodigoPostal == "13083851");

            Assert.NotNull(cache);
            Assert.Equal("Rua do Registro Manual", cache.Logradouro);
            Assert.Equal("Bairro Teste", cache.Bairro);
            Assert.Equal("Manual", cache.Provedor);
            Assert.False(cache.Falhou);
            Assert.Equal("CEP novo não localizado na base nacional do Correios", cache.MotivoFalha);
        }

        [Fact]
        public async Task Nao_Deve_Registrar_Cep_Manual_Se_Municipio_Estiver_Inativo()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_cep_manual_inativo");

            // Seed Pais e Municipio inativo
            var pais = new Pais("Brasil", "BR", "BRA", "076", "Brasília", "+55", "system");
            context.Paises.Add(pais);
            await context.SaveChangesAsync();

            var subdivisao = new Subdivisao(pais.Id, "BR-SP", "São Paulo", TipoSubdivisao.Estado, null, "system");
            context.Subdivisoes.Add(subdivisao);
            await context.SaveChangesAsync();

            var municipio = new Municipio(pais.Id, subdivisao.Id, "Cidade Antiga Extinta", 3550308, null, null, "system");
            municipio.Inativar("system");
            context.Municipios.Add(municipio);
            await context.SaveChangesAsync();

            var currentUser = new TestCurrentUser(UsuarioId);
            var handler = new RegistrarCepManualCommandHandler(context, currentUser);

            var command = new RegistrarCepManualCommand(
                Cep: "01001-000",
                Logradouro: "Rua Direta",
                Bairro: "Centro",
                MunicipioId: municipio.Id,
                Uf: "SP",
                Justificativa: "Nova rua"
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains("inativo", result.Erros.First());
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
    }
}
