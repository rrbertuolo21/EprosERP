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
using Epros.Shared.Application.Models;

namespace Epros.Tests
{
    public class PreferenciasAuditoriaTests
    {
        private const string TenantId = "tenant-prefs-test";
        private const string UsuarioId = "user-prefs-test";

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
        public async Task Deve_Atualizar_Preferencias_Nao_Criticas_Sem_Justificativa_Com_Sucesso()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_prefs_no_audit");

            // Setup initial preferences: NegativeCash=true, NegativeStock=true, Mode=CustoMedio
            var pref = new PreferenciaGeral(true, true, true, StockCalculationMode.CustoMedio, false, false, false, false, TenantId, UsuarioId);
            context.PreferenciasGerais.Add(pref);
            await context.SaveChangesAsync();

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UsuarioId);
            var handler = new AtualizarPreferenciasCommandHandler(context, tenantProvider, currentUser);

            // Change non-critical flags: ShowCurrency=false, Discount=true, but KEEP NegativeCash, NegativeStock and Mode unchanged
            var command = new AtualizarPreferenciasCommand(
                ShowCurrency: false,
                NegativeCash: true,
                NegativeStock: true,
                StockCalculationMode: StockCalculationMode.CustoMedio,
                CreditLimit: false,
                Discount: true,
                VatOnPurchase: false,
                VatOnSales: false,
                Justificativa: null
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            var updatedPref = await context.PreferenciasGerais.FirstOrDefaultAsync(p => p.TenantId == TenantId);
            Assert.NotNull(updatedPref);
            Assert.False(updatedPref.ShowCurrency);
            Assert.True(updatedPref.Discount);

            // Check that no audit log was created
            var auditLogs = await context.LogsAuditoriaConfiguracao.ToListAsync();
            Assert.Empty(auditLogs);
        }

        [Fact]
        public async Task Deve_Retornar_Falha_Se_Alterar_Flag_Critica_Sem_Justificativa()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_prefs_fail_audit");

            var pref = new PreferenciaGeral(true, true, true, StockCalculationMode.CustoMedio, false, false, false, false, TenantId, UsuarioId);
            context.PreferenciasGerais.Add(pref);
            await context.SaveChangesAsync();

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UsuarioId);
            var handler = new AtualizarPreferenciasCommandHandler(context, tenantProvider, currentUser);

            // Try to toggle NegativeStock to false without justification
            var command = new AtualizarPreferenciasCommand(
                ShowCurrency: true,
                NegativeCash: true,
                NegativeStock: false, // Critical flag changed!
                StockCalculationMode: StockCalculationMode.CustoMedio,
                CreditLimit: false,
                Discount: false,
                VatOnPurchase: false,
                VatOnSales: false,
                Justificativa: "" // Empty justification
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("Uma justificativa é obrigatória"));

            // Check that preferences were NOT updated
            var dbPref = await context.PreferenciasGerais.FirstOrDefaultAsync(p => p.TenantId == TenantId);
            Assert.NotNull(dbPref);
            Assert.True(dbPref.NegativeStock); // Remains true
        }

        [Fact]
        public async Task Deve_Atualizar_Preferencias_Criticas_E_Gravar_Audit_Log_Se_Justificativa_For_Fornecida()
        {
            // Arrange
            using var context = CreateInMemoryContext("db_prefs_success_audit");

            var pref = new PreferenciaGeral(true, true, true, StockCalculationMode.CustoMedio, false, false, false, false, TenantId, UsuarioId);
            context.PreferenciasGerais.Add(pref);
            await context.SaveChangesAsync();

            var tenantProvider = new TestTenantProvider(TenantId);
            var currentUser = new TestCurrentUser(UsuarioId);
            var handler = new AtualizarPreferenciasCommandHandler(context, tenantProvider, currentUser);

            // Toggle NegativeStock and StockCalculationMode with justification
            var command = new AtualizarPreferenciasCommand(
                ShowCurrency: true,
                NegativeCash: true,
                NegativeStock: false, // Critical flag changed
                StockCalculationMode: StockCalculationMode.FIFO, // Critical flag changed
                CreditLimit: false,
                Discount: false,
                VatOnPurchase: false,
                VatOnSales: false,
                Justificativa: "Nova política de controle rígido de inventário e FIFO."
            );

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            var dbPref = await context.PreferenciasGerais.FirstOrDefaultAsync(p => p.TenantId == TenantId);
            Assert.NotNull(dbPref);
            Assert.False(dbPref.NegativeStock);
            Assert.Equal(StockCalculationMode.FIFO, dbPref.StockCalculationMode);

            // Check audit logs
            var logs = await context.LogsAuditoriaConfiguracao.Where(l => l.TenantId == TenantId).ToListAsync();
            Assert.Equal(2, logs.Count); // One log for NegativeStock, one log for StockCalculationMode

            var logStock = logs.FirstOrDefault(l => l.Campo == "NegativeStock");
            Assert.NotNull(logStock);
            Assert.Equal("True", logStock.ValorAnterior);
            Assert.Equal("False", logStock.ValorNovo);
            Assert.Equal("Nova política de controle rígido de inventário e FIFO.", logStock.Justificativa);
            Assert.Equal(UsuarioId, logStock.UsuarioId);

            var logMode = logs.FirstOrDefault(l => l.Campo == "StockCalculationMode");
            Assert.NotNull(logMode);
            Assert.Equal("CustoMedio", logMode.ValorAnterior);
            Assert.Equal("FIFO", logMode.ValorNovo);
            Assert.Equal("Nova política de controle rígido de inventário e FIFO.", logMode.Justificativa);
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
        #endregion
    }
}
