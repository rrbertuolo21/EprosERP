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
    public class CaixaTests
    {
        [Fact]
        public async Task Deve_Abrir_Caixa_Com_Sucesso_E_Impedir_Duplicidade_Para_Mesmo_Operador()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("operador-1");
            var context = CreateInMemoryContext("db_caixa_abrir", "tenant-1", "operador-1");

            var handler = new AbrirCaixaCommandHandler(context, tenantProvider, currentUser);
            var command = new AbrirCaixaCommand("operador-1", 150.00m);

            // Act - Abre o primeiro caixa
            var result1 = await handler.Handle(command, CancellationToken.None);

            // Assert 1
            Assert.True(result1.Sucesso);
            var caixaSalvo = await context.Caixas.FirstOrDefaultAsync();
            Assert.NotNull(caixaSalvo);
            Assert.Equal("operador-1", caixaSalvo.OperadorId);
            Assert.Equal(150.00m, caixaSalvo.SaldoAbertura);
            Assert.Equal(ECaixaStatus.Aberto, caixaSalvo.Status);

            // Act - Tenta abrir o segundo caixa sem fechar o primeiro
            var result2 = await handler.Handle(command, CancellationToken.None);

            // Assert 2
            Assert.False(result2.Sucesso);
            Assert.Contains("já possui uma sessão de caixa aberta", result2.Erros.First());
        }

        [Fact]
        public async Task Deve_Fechar_Caixa_Auditando_Diferenca_De_Saldo_Corretamente()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("operador-1");
            var context = CreateInMemoryContext("db_caixa_fechar", "tenant-1", "operador-1");

            // Insere caixa aberto
            var caixa = new Caixa(Guid.NewGuid(), Guid.NewGuid(), "operador-1", 100.00m, "tenant-1", "operador-1", DateTime.UtcNow);
            context.Caixas.Add(caixa);

            // Insere movimentações (1 Suprimento de R$ 50, 1 Sangria de R$ 30)
            var sup = new CaixaMovimento(Guid.NewGuid(), Guid.NewGuid(), caixa.Id, "Suprimento", 50.00m, "Troco", "tenant-1", "operador-1", DateTime.UtcNow);
            var san = new CaixaMovimento(Guid.NewGuid(), Guid.NewGuid(), caixa.Id, "Sangria", 30.00m, "Retirada", "tenant-1", "operador-1", DateTime.UtcNow);
            context.CaixaMovimentos.AddRange(sup, san);

            // Insere vendas em dinheiro vinculadas (Total R$ 120.00)
            var venda = new Venda(Guid.NewGuid(), Guid.NewGuid(), caixa.Id.ToString(), 120.00m, EVendaStatus.Transmitido, "tenant-1", "operador-1", DateTime.UtcNow);
            context.Vendas.Add(venda);

            await context.SaveChangesAsync();

            var handler = new FecharCaixaCommandHandler(context, tenantProvider, currentUser);

            // Act - Saldo Calculado: 100 (Abertura) + 50 (Suprimento) - 30 (Sangria) + 120 (Venda) = 240.00
            // Fechamento informado: R$ 245.00 (Diferença de sobra de R$ 5.00)
            var command = new FecharCaixaCommand(caixa.Id, 245.00m);
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);
            var caixaFechado = await context.Caixas.FirstOrDefaultAsync(c => c.Id == caixa.Id);
            Assert.NotNull(caixaFechado);
            Assert.Equal(ECaixaStatus.Fechado, caixaFechado.Status);
            Assert.Equal(245.00m, caixaFechado.SaldoFechamento);
            Assert.Equal(5.00m, caixaFechado.DiferencaFechamento);
            Assert.NotNull(caixaFechado.FechadoEm);
        }

        [Fact]
        public async Task Deve_Sincronizar_Lote_Caixas_Offline_E_Movimentos_Idempotente()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("operador-1");
            var context = CreateInMemoryContext("db_caixas_sync_lote", "tenant-1", "operador-1");

            var handler = new SincronizarCaixasCommandHandler(context, tenantProvider, currentUser);

            var caixaId = Guid.NewGuid();
            var syncId = Guid.NewGuid();
            var movId = Guid.NewGuid();

            var movimentos = new List<CaixaMovimentoSyncDto>
            {
                new CaixaMovimentoSyncDto { Id = movId, SyncId = movId, Tipo = "Suprimento", Valor = 50.00m, CriadoEm = DateTime.UtcNow }
            };

            var caixaDto = new CaixaSyncDto
            {
                Id = caixaId,
                SyncId = syncId,
                OperadorId = "operador-1",
                SaldoAbertura = 100.00m,
                SaldoFechamento = 150.00m,
                Status = "Fechado",
                CriadoEm = DateTime.UtcNow.AddHours(-1),
                FechadoEm = DateTime.UtcNow,
                Movimentos = movimentos
            };

            var command = new SincronizarCaixasCommand(new List<CaixaSyncDto> { caixaDto });

            // Act - Primeira Execução
            var result1 = await handler.Handle(command, CancellationToken.None);

            // Assert 1
            Assert.True(result1.Sucesso);
            var caixaSalvo = await context.Caixas.FirstOrDefaultAsync(c => c.Id == caixaId);
            Assert.NotNull(caixaSalvo);
            Assert.Equal(ECaixaStatus.Fechado, caixaSalvo.Status);

            var movSalvo = await context.CaixaMovimentos.FirstOrDefaultAsync(m => m.CaixaId == caixaId);
            Assert.NotNull(movSalvo);
            Assert.Equal(50.00m, movSalvo.Valor);

            // Act - Segunda Execução (Idempotência)
            var result2 = await handler.Handle(command, CancellationToken.None);

            // Assert 2
            Assert.True(result2.Sucesso);
            var totalCaixas = await context.Caixas.CountAsync();
            Assert.Equal(1, totalCaixas); // Não deve duplicar
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
