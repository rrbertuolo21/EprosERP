using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Modules.Vendas.Domain.Entities;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Modules.Vendas.Application.Commands;
using Epros.Modules.Vendas.Application.Handlers;
using Epros.Modules.Vendas.Infrastructure.Data;

namespace Epros.Tests
{
    public class VendasTests
    {
        [Fact]
        public async Task Deve_Sincronizar_Vendas_Offline_Com_Sucesso_E_Evitar_Duplicidade()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_vendas_sync", "tenant-1", "user-1");

            var handler = new SincronizarVendasCommandHandler(context, tenantProvider, currentUser);

            var vendaId = Guid.NewGuid();
            var syncId = Guid.NewGuid();
            var prodId1 = Guid.NewGuid();
            var prodId2 = Guid.NewGuid();

            var itens = new List<VendaItemSyncDto>
            {
                new VendaItemSyncDto { ProdutoId = prodId1, Quantidade = 2, PrecoUnitario = 50m },
                new VendaItemSyncDto { ProdutoId = prodId2, Quantidade = 1, PrecoUnitario = 100m }
            };

            var vendaDto = new VendaSyncDto
            {
                Id = vendaId,
                SyncId = syncId,
                CaixaId = "caixa-01",
                Total = 200m,
                Status = "Contingencia",
                CriadoEm = DateTime.UtcNow.AddMinutes(-10),
                Itens = itens
            };

            var command = new SincronizarVendasCommand(new List<VendaSyncDto> { vendaDto });

            // Act - Primeira sincronização
            var result1 = await handler.Handle(command, CancellationToken.None);

            // Assert 1
            Assert.True(result1.Sucesso);
            var vendasBanco = await context.Vendas.Include(v => v.Itens).ToListAsync();
            Assert.Single(vendasBanco);
            Assert.Equal(vendaId, vendasBanco[0].Id);
            Assert.Equal(syncId, vendasBanco[0].SyncId);
            Assert.Equal(2, vendasBanco[0].Itens.Count);
            // DTO Status "Contingencia" é mapeado por VendaStatusMapper para EVendaStatus.Transmitido.
            Assert.Equal(EVendaStatus.Transmitido, vendasBanco[0].Status);

            // Act - Segunda sincronização (tentativa duplicada)
            var result2 = await handler.Handle(command, CancellationToken.None);

            // Assert 2
            Assert.True(result2.Sucesso); // Deve completar com sucesso
            Assert.Contains("Puladas/Existentes: 1", result2.Mensagem);

            var vendasBancoFinal = await context.Vendas.ToListAsync();
            Assert.Single(vendasBancoFinal); // Não deve ter adicionado outra venda
        }

        private ContextVendas CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextVendas>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextVendas(options, tenantProvider, currentUser);
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
