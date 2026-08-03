using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do submódulo Devolução de Compra (CD4 / EF DEVOLUCAO_DE_COMPRA): invariantes de domínio,
    /// checagem de quantidade contra a compra de origem (DEV-002), rascunho sem efeito (DEV-003) e o
    /// fluxo confirmar/cancelar com publicação dos eventos de saída de estoque (D1) e estorno financeiro
    /// idempotente (DEV-006) via Outbox. CQRS InMemory.
    /// </summary>
    public class DevolucaoCompraTests
    {
        private const string TenantId = "tenant-dev-001";
        private const string UserId = "user-dev-001";

        private ContextEstoque CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        /// <summary>Semeia uma compra com um item e retorna (compraId, compraItemId, quantidadeComprada).</summary>
        private static async Task<(Guid compraId, Guid itemId, decimal qtd)> SeedCompraAsync(ContextEstoque ctx, decimal qtdComprada = 10m)
        {
            var compra = new Compra("14200166000187", "Fornecedor Exemplo Ltda", "1001",
                "35200114200166000187550010000000015123456789", 500m, DateTime.UtcNow, TenantId, UserId);
            ctx.Compras.Add(compra);
            var item = new CompraItem(compra.Id, Guid.NewGuid(), qtdComprada, 50m, 0m, 0m, TenantId, UserId);
            ctx.CompraItens.Add(item);
            await ctx.SaveChangesAsync();
            return (compra.Id, item.Id, qtdComprada);
        }

        // ===================== Domínio =====================

        [Fact(DisplayName = "DevolucaoCompra | Sem compra de origem é inválida (DEV-001)")]
        public void Devolucao_SemCompraOrigem_Invalida()
        {
            var d = new DevolucaoCompra(Guid.Empty, Guid.NewGuid(), "DEV-1", DateTime.UtcNow,
                ETipoDevolucaoCompra.Total, null, null, TenantId, UserId);
            Assert.False(d.IsValid);
        }

        [Fact(DisplayName = "DevolucaoCompra | Item com quantidade zero é inválido (DEV-102)")]
        public void Item_QuantidadeZero_Invalido()
        {
            var item = new DevolucaoCompraItem(Guid.NewGuid(), null, Guid.NewGuid(), 0m, 10m, TenantId, UserId);
            Assert.False(item.IsValid);
        }

        [Fact(DisplayName = "DevolucaoCompra | Confirmar exige ao menos um item (DEV-012)")]
        public void Confirmar_SemItens_Falha()
        {
            var d = new DevolucaoCompra(Guid.NewGuid(), Guid.NewGuid(), "DEV-2", DateTime.UtcNow,
                ETipoDevolucaoCompra.Parcial, null, "5202", TenantId, UserId);
            Assert.True(d.IsValid);
            Assert.False(d.Confirmar(UserId));
            Assert.Equal(EStatusDevolucaoCompra.Rascunho, d.Status);
        }

        [Fact(DisplayName = "DevolucaoCompra | Confirmar rascunho com item transiciona para Confirmada e recalcula total")]
        public void Confirmar_ComItem_Transiciona()
        {
            var d = new DevolucaoCompra(Guid.NewGuid(), Guid.NewGuid(), "DEV-3", DateTime.UtcNow,
                ETipoDevolucaoCompra.Parcial, null, "5202", TenantId, UserId);
            d.AdicionarItem(new DevolucaoCompraItem(d.Id, null, Guid.NewGuid(), 2m, 30m, TenantId, UserId));
            Assert.True(d.Confirmar(UserId));
            Assert.Equal(EStatusDevolucaoCompra.Confirmada, d.Status);
            Assert.Equal(60m, d.Total);
            // Não pode confirmar duas vezes (DEV-011).
            Assert.False(d.Confirmar(UserId));
        }

        // ===================== CQRS =====================

        [Fact(DisplayName = "CQRS | Criar sem compra de origem existente falha (DEV-001)")]
        public async Task Criar_SemCompraExistente_Falha()
        {
            var ctx = CreateContext(nameof(Criar_SemCompraExistente_Falha));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);

            var res = await new CriarDevolucaoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarDevolucaoCompraCommand(Guid.NewGuid(), Guid.NewGuid(), "DEV-X", DateTime.UtcNow,
                    ETipoDevolucaoCompra.Total,
                    new List<DevolucaoCompraItemInput> { new(Guid.NewGuid(), 1m, 10m) }),
                CancellationToken.None);

            Assert.False(res.Sucesso);
        }

        [Fact(DisplayName = "CQRS | Quantidade devolvida acima da comprada falha (DEV-002)")]
        public async Task Criar_QuantidadeExcede_Falha()
        {
            var ctx = CreateContext(nameof(Criar_QuantidadeExcede_Falha));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (compraId, itemId, _) = await SeedCompraAsync(ctx, 5m);

            var res = await new CriarDevolucaoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarDevolucaoCompraCommand(compraId, Guid.NewGuid(), "DEV-EXC", DateTime.UtcNow,
                    ETipoDevolucaoCompra.Parcial,
                    new List<DevolucaoCompraItemInput> { new(Guid.NewGuid(), 6m, 50m, itemId) }),
                CancellationToken.None);

            Assert.False(res.Sucesso);
        }

        [Fact(DisplayName = "CQRS | Criar rascunho válido não gera efeito (DEV-003) e computa total")]
        public async Task Criar_Rascunho_SemEfeito()
        {
            var ctx = CreateContext(nameof(Criar_Rascunho_SemEfeito));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (compraId, itemId, _) = await SeedCompraAsync(ctx, 10m);

            var res = await new CriarDevolucaoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarDevolucaoCompraCommand(compraId, Guid.NewGuid(), "DEV-100", DateTime.UtcNow,
                    ETipoDevolucaoCompra.Parcial,
                    new List<DevolucaoCompraItemInput> { new(Guid.NewGuid(), 3m, 50m, itemId) }),
                CancellationToken.None);

            Assert.True(res.Sucesso);
            var dev = await ctx.DevolucoesCompra.Include(d => d.Itens).FirstAsync();
            Assert.Equal(EStatusDevolucaoCompra.Rascunho, dev.Status);
            Assert.Equal(150m, dev.Total);
            // DEV-003: rascunho não publica nenhum evento.
            Assert.False(await ctx.OutboxMessages.AnyAsync());
        }

        [Fact(DisplayName = "CQRS | Confirmar publica saída de estoque (D1) + estorno financeiro (DEV-006)")]
        public async Task Confirmar_PublicaEventos()
        {
            var ctx = CreateContext(nameof(Confirmar_PublicaEventos));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (compraId, itemId, _) = await SeedCompraAsync(ctx, 10m);

            var criar = await new CriarDevolucaoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarDevolucaoCompraCommand(compraId, Guid.NewGuid(), "DEV-200", DateTime.UtcNow,
                    ETipoDevolucaoCompra.Parcial,
                    new List<DevolucaoCompraItemInput> { new(Guid.NewGuid(), 4m, 50m, itemId) },
                    Cfop: "5202"),
                CancellationToken.None);
            var id = (Guid)criar.Dados!.GetType().GetProperty("Id")!.GetValue(criar.Dados)!;

            var conf = await new ConfirmarDevolucaoCompraCommandHandler(ctx, tenant, user)
                .Handle(new ConfirmarDevolucaoCompraCommand(id), CancellationToken.None);
            Assert.True(conf.Sucesso);

            var dev = await ctx.DevolucoesCompra.FirstAsync(x => x.Id == id);
            Assert.Equal(EStatusDevolucaoCompra.Confirmada, dev.Status);

            var eventos = await ctx.OutboxMessages.Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Compras.DevolucaoCompraConfirmada, eventos);
            Assert.Contains(CatalogoEventosIntegracao.Compras.DevolucaoCompraSaidaEstoque, eventos);
            Assert.Contains(CatalogoEventosIntegracao.Compras.DevolucaoCompraEstornoFinanceiro, eventos);
        }

        [Fact(DisplayName = "CQRS | Cancelar devolução confirmada compensa (evento); rascunho não compensa")]
        public async Task Cancelar_Confirmada_Compensa()
        {
            var ctx = CreateContext(nameof(Cancelar_Confirmada_Compensa));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (compraId, itemId, _) = await SeedCompraAsync(ctx, 10m);

            // Rascunho → cancelar: sem evento de compensação (DEV-003).
            var rasc = await new CriarDevolucaoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarDevolucaoCompraCommand(compraId, Guid.NewGuid(), "DEV-300", DateTime.UtcNow,
                    ETipoDevolucaoCompra.Parcial,
                    new List<DevolucaoCompraItemInput> { new(Guid.NewGuid(), 1m, 50m, itemId) }),
                CancellationToken.None);
            var rascId = (Guid)rasc.Dados!.GetType().GetProperty("Id")!.GetValue(rasc.Dados)!;
            await new CancelarDevolucaoCompraCommandHandler(ctx, tenant, user)
                .Handle(new CancelarDevolucaoCompraCommand(rascId), CancellationToken.None);
            Assert.False(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Compras.DevolucaoCompraCancelada));

            // Confirmada → cancelar: publica compensação.
            var conf = await new CriarDevolucaoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarDevolucaoCompraCommand(compraId, Guid.NewGuid(), "DEV-301", DateTime.UtcNow,
                    ETipoDevolucaoCompra.Parcial,
                    new List<DevolucaoCompraItemInput> { new(Guid.NewGuid(), 2m, 50m, itemId) }),
                CancellationToken.None);
            var confId = (Guid)conf.Dados!.GetType().GetProperty("Id")!.GetValue(conf.Dados)!;
            await new ConfirmarDevolucaoCompraCommandHandler(ctx, tenant, user)
                .Handle(new ConfirmarDevolucaoCompraCommand(confId), CancellationToken.None);
            var cancel = await new CancelarDevolucaoCompraCommandHandler(ctx, tenant, user)
                .Handle(new CancelarDevolucaoCompraCommand(confId), CancellationToken.None);

            Assert.True(cancel.Sucesso);
            Assert.True(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Compras.DevolucaoCompraCancelada));
            var dev = await ctx.DevolucoesCompra.FirstAsync(x => x.Id == confId);
            Assert.Equal(EStatusDevolucaoCompra.Cancelada, dev.Status);
        }

        [Fact(DisplayName = "CQRS | Excluir devolução confirmada é bloqueado (DEV-014)")]
        public async Task Excluir_Confirmada_Bloqueado()
        {
            var ctx = CreateContext(nameof(Excluir_Confirmada_Bloqueado));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (compraId, itemId, _) = await SeedCompraAsync(ctx, 10m);

            var criar = await new CriarDevolucaoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarDevolucaoCompraCommand(compraId, Guid.NewGuid(), "DEV-400", DateTime.UtcNow,
                    ETipoDevolucaoCompra.Parcial,
                    new List<DevolucaoCompraItemInput> { new(Guid.NewGuid(), 1m, 50m, itemId) }),
                CancellationToken.None);
            var id = (Guid)criar.Dados!.GetType().GetProperty("Id")!.GetValue(criar.Dados)!;
            await new ConfirmarDevolucaoCompraCommandHandler(ctx, tenant, user)
                .Handle(new ConfirmarDevolucaoCompraCommand(id), CancellationToken.None);

            var excluir = await new ExcluirDevolucaoCompraCommandHandler(ctx, user)
                .Handle(new ExcluirDevolucaoCompraCommand(id), CancellationToken.None);
            Assert.False(excluir.Sucesso);
        }

        [Fact(DisplayName = "Query | ObterPorId retorna a devolução com itens")]
        public async Task Query_ObterPorId_Retorna()
        {
            var ctx = CreateContext(nameof(Query_ObterPorId_Retorna));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (compraId, itemId, _) = await SeedCompraAsync(ctx, 10m);

            var criar = await new CriarDevolucaoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarDevolucaoCompraCommand(compraId, Guid.NewGuid(), "DEV-500", DateTime.UtcNow,
                    ETipoDevolucaoCompra.Parcial,
                    new List<DevolucaoCompraItemInput> { new(Guid.NewGuid(), 2m, 50m, itemId) }),
                CancellationToken.None);
            var id = (Guid)criar.Dados!.GetType().GetProperty("Id")!.GetValue(criar.Dados)!;

            var q = await new ObterDevolucaoCompraPorIdQueryHandler(ctx)
                .Handle(new ObterDevolucaoCompraPorIdQuery(id), CancellationToken.None);
            Assert.True(q.Sucesso);
        }
    }
}
