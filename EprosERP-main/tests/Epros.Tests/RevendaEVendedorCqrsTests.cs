using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.GestaoClientes.Application.Commands;
using Epros.Modules.GestaoClientes.Application.Handlers;
using Epros.Modules.GestaoClientes.Application.Queries;
using Epros.Modules.GestaoClientes.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    public class RevendaEVendedorCqrsTests
    {
        private ContextGestaoClientes CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextGestaoClientes>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextGestaoClientes(options, tenantProvider, currentUser);
        }

        [Fact]
        public async Task Deve_Criar_E_Atualizar_Revenda_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_revenda_test", "tenant-1", "user-1");
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");

            var createHandler = new CriarRevendaCommandHandler(context, tenantProvider, currentUser);
            var updateHandler = new AtualizarRevendaCommandHandler(context, currentUser);

            // Act - Criar
            var createCmd = new CriarRevendaCommand("Revenda Teste TI", 12.5m, true);
            var createResult = await createHandler.Handle(createCmd, CancellationToken.None);

            // Assert - Criar
            Assert.True(createResult.Sucesso);
            var revendaId = (Guid)createResult.Dados.GetType().GetProperty("RevendaId")!.GetValue(createResult.Dados)!;
            var revendaSalva = await context.Revendas.FindAsync(revendaId);
            Assert.NotNull(revendaSalva);
            Assert.Equal("Revenda Teste TI", revendaSalva.Nome);
            Assert.Equal(12.5m, revendaSalva.PercentualComissao);

            // Act - Atualizar
            var updateCmd = new AtualizarRevendaCommand(revendaId, "Revenda Teste TI Editada", 15.0m, true);
            var updateResult = await updateHandler.Handle(updateCmd, CancellationToken.None);

            // Assert - Atualizar
            Assert.True(updateResult.Sucesso);
            context.Entry(revendaSalva).State = EntityState.Detached; // Desanexa para buscar novamente do banco em memória
            var revendaEditada = await context.Revendas.FindAsync(revendaId);
            Assert.Equal("Revenda Teste TI Editada", revendaEditada!.Nome);
            Assert.Equal(15.0m, revendaEditada.PercentualComissao);
        }

        [Fact]
        public async Task Deve_Criar_E_Atualizar_Vendedor_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_vendedor_test", "tenant-1", "user-1");
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");

            // Criar revenda primeiro
            var revendaHandler = new CriarRevendaCommandHandler(context, tenantProvider, currentUser);
            var revResult = await revendaHandler.Handle(new CriarRevendaCommand("Revenda Alfa", 10m), CancellationToken.None);
            var revendaId = (Guid)revResult.Dados.GetType().GetProperty("RevendaId")!.GetValue(revResult.Dados)!;

            var createHandler = new CriarVendedorCommandHandler(context, tenantProvider, currentUser);
            var updateHandler = new AtualizarVendedorCommandHandler(context, currentUser);

            // Act - Criar
            var createCmd = new CriarVendedorCommand("Vendedor Roberto", "roberto@teste.com", "11988887777", 5m, revendaId);
            var createResult = await createHandler.Handle(createCmd, CancellationToken.None);

            // Assert - Criar
            Assert.True(createResult.Sucesso);
            var vendedorId = (Guid)createResult.Dados.GetType().GetProperty("VendedorId")!.GetValue(createResult.Dados)!;
            var vendedorSalvo = await context.Vendedores.FindAsync(vendedorId);
            Assert.NotNull(vendedorSalvo);
            Assert.Equal("Vendedor Roberto", vendedorSalvo.Nome);
            Assert.Equal("roberto@teste.com", vendedorSalvo.Email);
            Assert.Equal(revendaId, vendedorSalvo.RevendaId);

            // Act - Atualizar
            var updateCmd = new AtualizarVendedorCommand(vendedorId, "Vendedor Roberto Editado", "roberto.editado@teste.com", "11977776666", 6m, null, true);
            var updateResult = await updateHandler.Handle(updateCmd, CancellationToken.None);

            // Assert - Atualizar
            Assert.True(updateResult.Sucesso);
            context.Entry(vendedorSalvo).State = EntityState.Detached;
            var vendedorEditado = await context.Vendedores.FindAsync(vendedorId);
            Assert.Equal("Vendedor Roberto Editado", vendedorEditado!.Nome);
            Assert.Equal("roberto.editado@teste.com", vendedorEditado.Email);
            Assert.Null(vendedorEditado.RevendaId);
        }
    }
}
