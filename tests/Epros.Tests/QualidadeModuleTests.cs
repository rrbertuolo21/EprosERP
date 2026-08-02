using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Qualidade.Application.Commands;
using Epros.Modules.Qualidade.Application.EventHandlers;
using Epros.Modules.Qualidade.Application.Handlers;
using Epros.Modules.Qualidade.Application.Queries;
using Epros.Modules.Qualidade.Domain.Entities;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Models;
using Epros.Shared.Domain.Events;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    public class QualidadeModuleTests
    {
        [Fact]
        public async Task Deve_Criar_Inspecoes_Pendentes_Quando_Compra_For_Lancada()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextQualidade>()
                .UseInMemoryDatabase("db_qualidade_compra_lancada")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextQualidade(options, tenantProvider, currentUser);

            var handler = new CompraLancadaQualidadeHandler(context);

            var items = new List<CompraLancadaItemNotification>
            {
                new("PROD-001", "Produto 1", 10.0m, 15.0m),
                new("PROD-002", "Produto 2", 5.0m, 30.0m)
            };

            var notification = new CompraLancadaEventNotification(
                CompraId: Guid.NewGuid(),
                FornecedorId: Guid.NewGuid(),
                ValorTotal: 300.0m,
                DataVencimento: DateTime.UtcNow.AddDays(30),
                NumeroNota: "12345",
                TenantId: "tenant-1",
                UserId: "user-1",
                Itens: items
            );

            // Agir
            await handler.Handle(notification, CancellationToken.None);

            // Assertiva
            var inspecoes = await context.InspecoesLote.ToListAsync();
            Assert.Equal(2, inspecoes.Count);

            var inspecao1 = inspecoes.FirstOrDefault(i => i.Sku == "PROD-001");
            Assert.NotNull(inspecao1);
            Assert.Equal("Pendente", inspecao1!.Status);
            Assert.Equal(10.0m, inspecao1.QuantidadeLote);

            var inspecao2 = inspecoes.FirstOrDefault(i => i.Sku == "PROD-002");
            Assert.NotNull(inspecao2);
            Assert.Equal("Pendente", inspecao2!.Status);
            Assert.Equal(5.0m, inspecao2.QuantidadeLote);
        }

        [Fact]
        public async Task Deve_Aprovar_Inspecao_Sem_Criar_Nao_Conformidade()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextQualidade>()
                .UseInMemoryDatabase("db_qualidade_inspecao_aprovada")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextQualidade(options, tenantProvider, currentUser);

            var inspecao = new InspecaoLote(Guid.NewGuid(), "PROD-001", "Produto 1", 10.0m, "tenant-1", "user-1");
            context.InspecoesLote.Add(inspecao);
            await context.SaveChangesAsync();

            var handler = new RegistrarResultadoInspecaoCommandHandler(context, tenantProvider, currentUser);
            var command = new RegistrarResultadoInspecaoCommand(inspecao.Id, "Aprovado", "Inspetor João", "Lote OK");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var inspecaoAtualizada = await context.InspecoesLote.FindAsync(inspecao.Id);
            Assert.Equal("Aprovado", inspecaoAtualizada!.Status);
            Assert.Equal("Inspetor João", inspecaoAtualizada.Responsavel);
            Assert.Equal("Lote OK", inspecaoAtualizada.Observacoes);

            var ncrCriada = await context.NaoConformidades.AnyAsync(nc => nc.InspecaoLoteId == inspecao.Id);
            Assert.False(ncrCriada);

            var outboxMessage = await context.OutboxMessages.AnyAsync();
            Assert.False(outboxMessage);
        }

        [Fact]
        public async Task Deve_Reprovar_Inspecao_E_Criar_Nao_Conformidade_E_Outbox_Event()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextQualidade>()
                .UseInMemoryDatabase("db_qualidade_inspecao_reprovada")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextQualidade(options, tenantProvider, currentUser);

            var inspecao = new InspecaoLote(Guid.NewGuid(), "PROD-001", "Produto 1", 10.0m, "tenant-1", "user-1");
            context.InspecoesLote.Add(inspecao);
            await context.SaveChangesAsync();

            var handler = new RegistrarResultadoInspecaoCommandHandler(context, tenantProvider, currentUser);
            var command = new RegistrarResultadoInspecaoCommand(inspecao.Id, "Reprovado", "Inspetor João", "Lote com avarias na embalagem");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var inspecaoAtualizada = await context.InspecoesLote.FindAsync(inspecao.Id);
            Assert.Equal("Reprovado", inspecaoAtualizada!.Status);

            var ncr = await context.NaoConformidades.FirstOrDefaultAsync(nc => nc.InspecaoLoteId == inspecao.Id);
            Assert.NotNull(ncr);
            Assert.Equal("Aberta", ncr!.Status);
            Assert.Equal("PROD-001", ncr.Sku);
            Assert.Contains("Lote Reprovado", ncr.Titulo);

            var outboxMessage = await context.OutboxMessages.FirstOrDefaultAsync();
            Assert.NotNull(outboxMessage);
            Assert.Equal("InspecaoReprovada", outboxMessage!.EventType);
            Assert.Equal("tenant-1", outboxMessage.TenantId);
        }

        [Fact]
        public async Task Deve_Tratar_E_Resolver_Nao_Conformidade()
        {
            // Organizar
            var options = new DbContextOptionsBuilder<ContextQualidade>()
                .UseInMemoryDatabase("db_qualidade_ncr_tratar")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var context = new ContextQualidade(options, tenantProvider, currentUser);

            var ncr = new NaoConformidade(Guid.NewGuid(), "PROD-001", "Lote Reprovado", "Defeito", "tenant-1", "user-1");
            context.NaoConformidades.Add(ncr);
            await context.SaveChangesAsync();

            var handler = new TratarNaoConformidadeCommandHandler(context, currentUser);
            var command = new TratarNaoConformidadeCommand(ncr.Id, "Falha de temperatura no transporte", "Devolução ao fornecedor", "Gerente Maria");

            // Agir
            var result = await handler.Handle(command, CancellationToken.None);

            // Assertiva
            Assert.True(result.Sucesso);

            var ncrAtualizada = await context.NaoConformidades.FindAsync(ncr.Id);
            Assert.Equal("Resolvida", ncrAtualizada!.Status);
            Assert.Equal("Falha de temperatura no transporte", ncrAtualizada.CausaRaiz);
            Assert.Equal("Devolução ao fornecedor", ncrAtualizada.PlanoAcao);
            Assert.Equal("Gerente Maria", ncrAtualizada.ResolvidoPor);
            Assert.NotNull(ncrAtualizada.ResolvidoEm);
        }

        [Fact]
        public async Task Deve_Baixar_Estoque_Quando_Inspecao_For_Reprovada()
        {
            // Organizar
            var optionsEstoque = new DbContextOptionsBuilder<ContextEstoque>()
                .UseInMemoryDatabase("db_estoque_inspecao_reprovada")
                .Options;

            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            using var contextEstoque = new ContextEstoque(optionsEstoque, tenantProvider, currentUser);

            var produto = new Produto("PROD-001", "Produto Teste", 100.0m, "tenant-1", "user-1");
            contextEstoque.Produtos.Add(produto);
            await contextEstoque.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(contextEstoque, "tenant-1", "user-1", produto.Id, 50.0m, 50.0m);

            var handler = new InspecaoReprovadaEstoqueHandler(contextEstoque);
            var notification = new InspecaoReprovadaEventNotification(
                InspecaoLoteId: Guid.NewGuid(),
                TenantId: "tenant-1",
                Sku: "PROD-001",
                NomeProduto: "Produto Teste",
                QuantidadeLote: 10.0m,
                DataInspecao: DateTime.UtcNow,
                Responsavel: "Inspetor João"
            );

            // Agir
            await handler.Handle(notification, CancellationToken.None);

            // Assertiva
            var produtoAtualizado = await contextEstoque.Produtos.FindAsync(produto.Id);
            Assert.Equal(40.0m, produtoAtualizado!.SaldoEstoque); // Baixou 10 do saldo original de 50
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
