using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Application.Handlers;
using Epros.Modules.Manutencao.Application.Queries;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class ManutencaoModuleTests
    {
        [Fact]
        public async Task Deve_Criar_Equipamento()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextManutencao>()
                .UseInMemoryDatabase("db_manutencao_equipamento")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextManutencao(options, tenantProvider, currentUser);

            var handler = new CriarEquipamentoCommandHandler(context, tenantProvider, currentUser);
            var command = new CriarEquipamentoCommand("Injetora Romi 300T", "INJ-300", "Produção Plásticos", DateTime.UtcNow, "Alta");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var eq = await context.Equipamentos.ToListAsync();
            Assert.Single(eq);
            Assert.Equal("INJ-300", eq[0].Codigo);
            Assert.Equal("Ativo", eq[0].Status);
            Assert.Equal("Alta", eq[0].Criticidade);
        }

        [Fact]
        public async Task Deve_Abrir_OS_E_Alterar_Status_Equipamento()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextManutencao>()
                .UseInMemoryDatabase("db_manutencao_abrir_os")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextManutencao(options, tenantProvider, currentUser);

            var eq = new Equipamento("Injetora", "INJ-300", "Produção", DateTime.UtcNow, "Alta", "tenant-1", "user-1");
            context.Equipamentos.Add(eq);
            await context.SaveChangesAsync();

            var handler = new AbrirOrdemManutencaoCommandHandler(context, tenantProvider, currentUser);
            var command = new AbrirOrdemManutencaoCommand(eq.Id, "Corretiva", "Vazamento de óleo hidráulico");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var ordens = await context.OrdensManutencao.ToListAsync();
            Assert.Single(ordens);
            Assert.Equal(eq.Id, ordens[0].EquipamentoId);
            Assert.Equal("Aberta", ordens[0].Status);

            var eqAtualizado = await context.Equipamentos.FindAsync(eq.Id);
            Assert.Equal("EmManutencao", eqAtualizado!.Status);
        }

        [Fact]
        public async Task Deve_Adicionar_Peca_A_OS()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextManutencao>()
                .UseInMemoryDatabase("db_manutencao_peca_os")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextManutencao(options, tenantProvider, currentUser);

            var eq = new Equipamento("Injetora", "INJ-300", "Produção", DateTime.UtcNow, "Alta", "tenant-1", "user-1");
            var OS = new OrdemManutencao(eq.Id, "Corretiva", "Vazamento", "tenant-1", "user-1");
            context.Equipamentos.Add(eq);
            context.OrdensManutencao.Add(OS);
            await context.SaveChangesAsync();

            var handler = new AdicionarPecaOrdemCommandHandler(context, currentUser);
            var produtoPecaId = Guid.NewGuid();
            var command = new AdicionarPecaOrdemCommand(OS.Id, produtoPecaId, 2m);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var osAtualizada = await context.OrdensManutencao.Include(o => o.Pecas).FirstAsync(o => o.Id == OS.Id);
            Assert.Single(osAtualizada.Pecas);
            Assert.Equal(produtoPecaId, osAtualizada.Pecas[0].ProdutoId);
            Assert.Equal(2m, osAtualizada.Pecas[0].Quantidade);
        }

        [Fact]
        public async Task Deve_Concluir_OS_E_Gerar_Outbox_E_Restaurar_Equipamento()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextManutencao>()
                .UseInMemoryDatabase("db_manutencao_concluir_os")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextManutencao(options, tenantProvider, currentUser);

            var eq = new Equipamento("Injetora", "INJ-300", "Produção", DateTime.UtcNow, "Alta", "tenant-1", "user-1");
            eq.AlterarStatus("EmManutencao", "user-1");
            var OS = new OrdemManutencao(eq.Id, "Corretiva", "Vazamento", "tenant-1", "user-1");
            OS.AdicionarPeca(Guid.NewGuid(), 2m, "user-1");
            context.Equipamentos.Add(eq);
            context.OrdensManutencao.Add(OS);
            await context.SaveChangesAsync();

            var handler = new ConcluirOrdemManutencaoCommandHandler(context, tenantProvider, currentUser);
            var command = new ConcluirOrdemManutencaoCommand(OS.Id, "Troca de mangueira hidráulica", 300m);

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var osAtualizada = await context.OrdensManutencao.FindAsync(OS.Id);
            Assert.Equal("Concluida", osAtualizada!.Status);
            Assert.Equal(300m, osAtualizada.CustoMaoObra);

            var eqAtualizado = await context.Equipamentos.FindAsync(eq.Id);
            Assert.Equal("Ativo", eqAtualizado!.Status);

            var outboxMsg = await context.OutboxMessages.FirstOrDefaultAsync();
            Assert.NotNull(outboxMsg);
            Assert.Equal("OrdemManutencaoConcluida", outboxMsg!.EventType);
        }

        [Fact]
        public async Task Deve_Deduzir_Pecas_Do_Estoque_Ao_Processar_OrdemManutencaoConcluida()
        {
            // Organizar
            var optionsEstoque = new DbContextOptionsBuilder<ContextEstoque>()
                .UseInMemoryDatabase("db_estoque_manutencao_integracao")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var contextEstoque = new ContextEstoque(optionsEstoque, tenantProvider, currentUser);
            var produtoPeca = new Produto("MANG-01", "Mangueira Hidráulica", 50m, "tenant-1", "user-1");
            contextEstoque.Produtos.Add(produtoPeca);
            await contextEstoque.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(contextEstoque, "tenant-1", "user-1", produtoPeca.Id, 10m, 25m);

            var handler = new OrdemManutencaoConcluidaEstoqueHandler(contextEstoque);

            var pecas = new List<OrdemManutencaoPecaDto>
            {
                new(produtoPeca.Id, 2m)
            };

            var notification = new OrdemManutencaoConcluidaEventNotification(
                OrdemManutencaoId: Guid.NewGuid(),
                EquipamentoId: Guid.NewGuid(),
                Pecas: pecas,
                TenantId: "tenant-1"
            );

            // Agir
            await handler.Handle(notification, CancellationToken.None);

            // Assertiva
            var produtoAtualizado = await contextEstoque.Produtos.FindAsync(produtoPeca.Id);
            // SaldoEstoque = 10 (inicial) - 2 (consumido) = 8
            Assert.Equal(8m, produtoAtualizado!.SaldoEstoque);

            var movimentos = await contextEstoque.MovimentosEstoque.ToListAsync();
            Assert.Single(movimentos);
            Assert.Equal(produtoPeca.Id, movimentos[0].ProdutoId);
            Assert.Equal(2m, movimentos[0].Quantidade);
            Assert.Equal("Saida", movimentos[0].Tipo);
            Assert.Contains($"Baixa OS {notification.OrdemManutencaoId}", movimentos[0].Historico);
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
            public string? GetUserName() => "test_user";
            public string? GetUserEmail() => "test@epros.com.br";
        }
    }
}
