using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Epros.Modules.Manutencao.Application.Commands;
using Epros.Modules.Manutencao.Domain.Entities;
using Epros.Modules.Manutencao.Domain.Enums;
using Epros.Modules.Manutencao.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// MAN-PEC — D22/D23: reservar / baixar (delta, idempotente) / devolver peca; evento de estoque.
    /// </summary>
    public class ManutencaoPecasOperacoesTests
    {
        private const string TenantId = "tenant-man-pec";
        private const string UserId = "user-man-pec";

        private static ContextManutencao NovoContexto(string db)
        {
            var options = new DbContextOptionsBuilder<ContextManutencao>().UseInMemoryDatabase(db).Options;
            return new ContextManutencao(options, new TP(TenantId), new CU(UserId));
        }

        private static Guid SeedItem(string db, decimal quantidade)
        {
            using var seed = NovoContexto(db);
            var reg = new RegistroPecaReposicao("PEC-01", "Registro", Guid.NewGuid(), null, null, TenantId, UserId);
            var item = new ItemPecaReposicao(reg.Id, Guid.NewGuid(), 1, quantidade, null, null, 10m, 0m, ETipoSaidaItem.Venda, TenantId, UserId);
            reg.AdicionarItem(item, UserId);
            seed.RegistrosPecaReposicao.Add(reg);
            seed.ItensPecaReposicao.Add(item);
            seed.SaveChanges();
            return item.Id;
        }

        [Fact(DisplayName = "MAN-PEC | Reservar cria reserva e marca item reservado")]
        public async Task Pec_Reservar_CriaReserva()
        {
            var db = nameof(Pec_Reservar_CriaReserva);
            var itemId = SeedItem(db, 5m);

            using var ctx = NovoContexto(db);
            var handler = new ReservarPecaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new ReservarPecaCommand(itemId, 5m, null, null), CancellationToken.None);
            Assert.True(result.Sucesso);
            Assert.Equal(1, await ctx.ReservasPeca.CountAsync());
            var item = await ctx.ItensPecaReposicao.FindAsync(itemId);
            Assert.Equal(EStatusItemPeca.Reservado, item!.StatusItem);
        }

        [Fact(DisplayName = "MAN-PEC | Baixa move entregue, cria movimento e enfileira evento de estoque")]
        public async Task Pec_Baixar_CriaMovimentoEEvento()
        {
            var db = nameof(Pec_Baixar_CriaMovimentoEEvento);
            var itemId = SeedItem(db, 5m);

            using var ctx = NovoContexto(db);
            var handler = new BaixarPecaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new BaixarPecaCommand(itemId, 3m, null, null), CancellationToken.None);
            Assert.True(result.Sucesso);

            var item = await ctx.ItensPecaReposicao.FindAsync(itemId);
            Assert.Equal(3m, item!.QuantidadeEntregue);
            Assert.Equal(EStatusItemPeca.EntregueParcial, item.StatusItem);
            Assert.Equal(1, await ctx.MovimentosPeca.CountAsync(m => m.TipoMovimento == ETipoMovimentoPeca.Baixa));
            Assert.Equal(1, await ctx.OutboxMessages.CountAsync(o => o.EventType == "OrdemManutencaoConcluida"));
        }

        [Fact(DisplayName = "MAN-PEC | Baixa idempotente por operacao nao duplica")]
        public async Task Pec_Baixar_Idempotente()
        {
            var db = nameof(Pec_Baixar_Idempotente);
            var itemId = SeedItem(db, 5m);
            var op = Guid.NewGuid();

            using (var ctx1 = NovoContexto(db))
            {
                var h1 = new BaixarPecaCommandHandler(ctx1, new TP(TenantId), new CU(UserId));
                await h1.Handle(new BaixarPecaCommand(itemId, 2m, op, null), CancellationToken.None);
            }
            using var ctx2 = NovoContexto(db);
            var h2 = new BaixarPecaCommandHandler(ctx2, new TP(TenantId), new CU(UserId));
            var result = await h2.Handle(new BaixarPecaCommand(itemId, 2m, op, null), CancellationToken.None);
            Assert.True(result.Sucesso); // idempotente
            Assert.Equal(1, await ctx2.MovimentosPeca.CountAsync(m => m.TipoMovimento == ETipoMovimentoPeca.Baixa));
            var item = await ctx2.ItensPecaReposicao.FindAsync(itemId);
            Assert.Equal(2m, item!.QuantidadeEntregue); // nao dobrou
        }

        [Fact(DisplayName = "MAN-PEC | Baixa respeita o delta (nao excede solicitado)")]
        public async Task Pec_Baixar_RespeitaDelta()
        {
            var db = nameof(Pec_Baixar_RespeitaDelta);
            var itemId = SeedItem(db, 5m);

            using var ctx = NovoContexto(db);
            var handler = new BaixarPecaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await handler.Handle(new BaixarPecaCommand(itemId, 10m, null, null), CancellationToken.None);
            Assert.True(result.Sucesso);
            var item = await ctx.ItensPecaReposicao.FindAsync(itemId);
            Assert.Equal(5m, item!.QuantidadeEntregue); // limitado ao saldo
            Assert.Equal(EStatusItemPeca.EntregueTotal, item.StatusItem);
        }

        [Fact(DisplayName = "MAN-PEC | Devolucao reduz entregue (movimento inverso)")]
        public async Task Pec_Devolver_ReduzEntregue()
        {
            var db = nameof(Pec_Devolver_ReduzEntregue);
            var itemId = SeedItem(db, 5m);

            using (var ctx1 = NovoContexto(db))
            {
                var hb = new BaixarPecaCommandHandler(ctx1, new TP(TenantId), new CU(UserId));
                await hb.Handle(new BaixarPecaCommand(itemId, 4m, null, null), CancellationToken.None);
            }
            using var ctx = NovoContexto(db);
            var hd = new DevolverPecaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await hd.Handle(new DevolverPecaCommand(itemId, 1m, null), CancellationToken.None);
            Assert.True(result.Sucesso);
            var item = await ctx.ItensPecaReposicao.FindAsync(itemId);
            Assert.Equal(3m, item!.QuantidadeEntregue);
            Assert.Equal(1, await ctx.MovimentosPeca.CountAsync(m => m.TipoMovimento == ETipoMovimentoPeca.Devolucao));
            // T5 — devolucao enfileira o evento de estorno SIMETRICO (entrada compensatoria no motor unico).
            Assert.Equal(1, await ctx.OutboxMessages.CountAsync(o => o.EventType == Epros.Shared.Domain.Events.CatalogoEventosIntegracao.Operacoes.DevolucaoPecaManutencao));
        }

        [Fact(DisplayName = "MAN-PEC | Devolucao acima do entregue e bloqueada")]
        public async Task Pec_Devolver_AcimaEntregue_Falha()
        {
            var db = nameof(Pec_Devolver_AcimaEntregue_Falha);
            var itemId = SeedItem(db, 5m);
            using (var ctx1 = NovoContexto(db))
            {
                var hb = new BaixarPecaCommandHandler(ctx1, new TP(TenantId), new CU(UserId));
                await hb.Handle(new BaixarPecaCommand(itemId, 2m, null, null), CancellationToken.None);
            }
            using var ctx = NovoContexto(db);
            var hd = new DevolverPecaCommandHandler(ctx, new TP(TenantId), new CU(UserId));
            var result = await hd.Handle(new DevolverPecaCommand(itemId, 3m, null), CancellationToken.None);
            Assert.False(result.Sucesso);
        }

        private class TP : ITenantProvider
        {
            private readonly string _t;
            public TP(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class CU : ICurrentUser
        {
            private readonly string _u;
            public CU(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "man-pec";
            public string? GetUserEmail() => "man-pec@epros.com.br";
        }
    }
}
