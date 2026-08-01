using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.GestaoClientes.Domain.Entities;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;

namespace Epros.Tests
{
    public class GeografiaQueriesTests
    {
        private const string TenantId = "tenant-queries-test";
        private const string UsuarioId = "user-queries-test";

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
        public async Task Deve_Obter_Pais_Por_Id_Com_Sucesso()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_obter_pais_por_id");
            var pais = new Pais("Alemanha", "DE", "DEU", "276", "Berlin", "+49", UsuarioId);
            context.Paises.Add(pais);
            await context.SaveChangesAsync();

            var handler = new ObterPaisPorIdQueryHandler(context);
            var query = new ObterPaisPorIdQuery(pais.Id);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Alemanha", result.Nome);
            Assert.Equal("DE", result.CodigoIsoAlpha2);
            Assert.Equal("DEU", result.CodigoIsoAlpha3);
            Assert.True(result.Ativo);
        }

        [Fact]
        public async Task Deve_Retornar_Null_Ao_Obter_Pais_Por_Id_Inexistente()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_obter_pais_inexistente");
            var handler = new ObterPaisPorIdQueryHandler(context);
            var query = new ObterPaisPorIdQuery(Guid.NewGuid());

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Deve_Listar_Municipios_Por_Id_Subdivisao_Com_Sucesso()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_listar_municipios_por_id_sub");

            var pais = new Pais("Brasil", "BR", "BRA", "076", "Brasília", "+55", UsuarioId);
            context.Paises.Add(pais);
            await context.SaveChangesAsync();

            var subdivisao1 = new Subdivisao(pais.Id, "BR-SP", "São Paulo", TipoSubdivisao.Estado, null, UsuarioId);
            var subdivisao2 = new Subdivisao(pais.Id, "BR-RJ", "Rio de Janeiro", TipoSubdivisao.Estado, null, UsuarioId);
            context.Subdivisoes.AddRange(subdivisao1, subdivisao2);
            await context.SaveChangesAsync();

            var mun1 = new Municipio(pais.Id, subdivisao1.Id, "Campinas", 3509502, null, null, UsuarioId);
            var mun2 = new Municipio(pais.Id, subdivisao1.Id, "Sorocaba", 3552205, null, null, UsuarioId);
            var mun3 = new Municipio(pais.Id, subdivisao2.Id, "Niterói", 3303302, null, null, UsuarioId);
            context.Municipios.AddRange(mun1, mun2, mun3);
            await context.SaveChangesAsync();

            var handler = new ListarMunicipiosPorIdUfQueryHandler(context);
            var query = new ListarMunicipiosPorIdUfQuery(subdivisao1.Id);

            // Act
            var result = (await handler.Handle(query, CancellationToken.None)).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(result, m => m.Nome == "Campinas");
            Assert.Contains(result, m => m.Nome == "Sorocaba");
            Assert.DoesNotContain(result, m => m.Nome == "Niterói");
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
