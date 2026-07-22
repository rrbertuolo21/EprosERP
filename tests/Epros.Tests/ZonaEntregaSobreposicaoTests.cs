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
    public class ZonaEntregaSobreposicaoTests
    {
        private const string TenantId = "tenant-zona-test";
        private const string UsuarioId = "user-zona-test";

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
        public async Task Deve_Bloquear_Criacao_De_Zona_De_Entrega_Com_Faixa_De_Cep_Sobreposta()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_zonas_sobreposta");

            // Zona existente: CEPs de 01000000 a 09999999
            var zonaExistente = new ZonaEntrega("Zona SP Central", "01000000", "09999999", TenantId, UsuarioId);
            context.ZonasEntrega.Add(zonaExistente);
            await context.SaveChangesAsync();

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UsuarioId);
            var handler = new CriarZonaEntregaCommandHandler(context, tenantProvider, currentUser);

            // Tenta criar zona sobreposta: CEPs de 05000000 a 12000000
            var command = new CriarZonaEntregaCommand("Zona SP Ampliada", "05000000", "12000000");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains("sobrepõe", result.Erros.First());
        }

        [Fact]
        public async Task Deve_Permitir_Criacao_De_Zona_De_Entrega_Com_Faixa_De_Cep_Diferente()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_zonas_sucesso");

            // Zona existente: CEPs de 01000000 a 09999999
            var zonaExistente = new ZonaEntrega("Zona SP Central", "01000000", "09999999", TenantId, UsuarioId);
            context.ZonasEntrega.Add(zonaExistente);
            await context.SaveChangesAsync();

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UsuarioId);
            var handler = new CriarZonaEntregaCommandHandler(context, tenantProvider, currentUser);

            // Cria zona não sobreposta: CEPs de 11000000 a 19999999
            var command = new CriarZonaEntregaCommand("Zona Campinas", "11000000", "19999999");

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            Assert.Single(context.ZonasEntrega.Where(z => z.Nome == "Zona Campinas"));
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
