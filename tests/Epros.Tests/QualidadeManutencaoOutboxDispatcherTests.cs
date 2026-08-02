using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Application.Outbox;
using Epros.Shared.Domain.Events;
using Epros.Infrastructure.Outbox;
using Epros.Modules.Estoque.Application.Outbox;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Qualidade.Infrastructure.Data;
using Epros.Modules.Manutencao.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// TRANSVERSAL T1 — prova que os Outboxes de QUALIDADE e MANUTENÇÃO, migrados para o DISPATCHER
    /// CENTRAL, roteiam por EventType para CONSUMIDORES REAIS no Estoque (que antes "morriam"):
    ///  - qld.acr.lote_bloqueado / qld.rst.bloqueio_solicitado -> contenção do lote (disponível->bloqueado),
    ///    sem recriar saldo (anti-dupla-contagem);
    ///  - InspecaoReprovada / OrdemManutencaoConcluida -> baixa pelo motor único.
    /// Idempotente na reexecução do dispatcher.
    /// </summary>
    public class QualidadeManutencaoOutboxDispatcherTests
    {
        [Fact]
        public async Task Dispatcher_Qualidade_Bloqueia_Lote_No_Estoque_Sem_Recriar_Saldo()
        {
            var tenantId = "tenant-qld-bloq";
            var tp = new TestTenantProvider(tenantId); var cu = new TestCurrentUser("u");
            var db = Guid.NewGuid().ToString("N");
            var qldCtx = CreateQualidade(db, tp, cu);
            var estCtx = CreateEstoque(db, tp, cu);

            // Lote rastreável no Estoque com 100 disponíveis.
            var lote = new LoteEstoque(Guid.Empty, Guid.NewGuid(), "LOTE-XYZ", 100m, null, null, null, null,
                EOrigemLote.Compra, null, tenantId, "seed");
            estCtx.LotesEstoque.Add(lote);
            await estCtx.SaveChangesAsync();

            // Evento de bloqueio publicado no Outbox de Qualidade (código do lote + quantidade afetada).
            var payload = JsonSerializer.Serialize(new { analiseId = Guid.NewGuid(), lote = "LOTE-XYZ", quantidade = 30m, resultado = "Rejeitado", tenantId });
            qldCtx.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Qualidade.AcrLoteBloqueado, payload));
            await qldCtx.SaveChangesAsync();

            var dispatcher = new OutboxDispatcher(
                new IOutboxConsumer[] { new QualidadeLoteBloqueadoConsumer(estCtx) },
                NullLogger<OutboxDispatcher>.Instance);

            var roteadas = await dispatcher.ProcessAsync(qldCtx);

            Assert.Equal(1, roteadas);
            Assert.NotNull((await qldCtx.OutboxMessages.IgnoreQueryFilters().FirstAsync()).ProcessadoEm);

            var lotePos = await estCtx.LotesEstoque.IgnoreQueryFilters().FirstAsync(l => l.Id == lote.Id);
            Assert.Equal(70m, lotePos.QuantidadeDisponivel); // 100 - 30
            Assert.Equal(30m, lotePos.QuantidadeBloqueada);
            Assert.Equal(100m, lotePos.QuantidadeRecebida); // saldo do lote NÃO foi recriado
            Assert.Equal(EStatusLoteRastreabilidade.Bloqueado, lotePos.Status);

            // Bloqueio + evento de estoque emitidos.
            Assert.Single(await estCtx.BloqueiosLote.IgnoreQueryFilters().Where(b => b.LoteId == lote.Id).ToListAsync());
            Assert.Single(await estCtx.OutboxMessages.IgnoreQueryFilters()
                .Where(m => m.EventType == CatalogoEventosIntegracao.Estoque.LoteBloqueado).ToListAsync());

            // Idempotência: reexecutar não bloqueia de novo.
            var roteadas2 = await dispatcher.ProcessAsync(qldCtx);
            Assert.Equal(0, roteadas2);
            var lotePos2 = await estCtx.LotesEstoque.IgnoreQueryFilters().FirstAsync(l => l.Id == lote.Id);
            Assert.Equal(70m, lotePos2.QuantidadeDisponivel);
        }

        [Fact]
        public async Task Dispatcher_Qualidade_Rst_Bloqueio_Solicitado_Contem_Lote()
        {
            var tenantId = "tenant-rst";
            var tp = new TestTenantProvider(tenantId); var cu = new TestCurrentUser("u");
            var db = Guid.NewGuid().ToString("N");
            var qldCtx = CreateQualidade(db, tp, cu);
            var estCtx = CreateEstoque(db, tp, cu);

            var lote = new LoteEstoque(Guid.Empty, Guid.NewGuid(), "LOTE-RST", 50m, null, null, null, null,
                EOrigemLote.Compra, null, tenantId, "seed");
            estCtx.LotesEstoque.Add(lote);
            await estCtx.SaveChangesAsync();

            var payload = JsonSerializer.Serialize(new { campanhaId = Guid.NewGuid(), bloqueioId = Guid.NewGuid(), Lote = "LOTE-RST", Serial = (string?)null, quantidade = 0m, tenantId });
            qldCtx.OutboxMessages.Add(new OutboxMessage(tenantId, CatalogoEventosIntegracao.Qualidade.RstBloqueioSolicitado, payload));
            await qldCtx.SaveChangesAsync();

            var dispatcher = new OutboxDispatcher(
                new IOutboxConsumer[] { new QualidadeRstBloqueioSolicitadoConsumer(estCtx) },
                NullLogger<OutboxDispatcher>.Instance);

            var roteadas = await dispatcher.ProcessAsync(qldCtx);
            Assert.Equal(1, roteadas);

            var lotePos = await estCtx.LotesEstoque.IgnoreQueryFilters().FirstAsync(l => l.Id == lote.Id);
            // quantidade 0 no payload -> contém TODO o disponível.
            Assert.Equal(0m, lotePos.QuantidadeDisponivel);
            Assert.Equal(50m, lotePos.QuantidadeBloqueada);
        }

        [Fact]
        public async Task Dispatcher_Manutencao_Baixa_Pecas_Pelo_Motor()
        {
            var tenantId = "tenant-man";
            var tp = new TestTenantProvider(tenantId); var cu = new TestCurrentUser("u");
            var db = Guid.NewGuid().ToString("N");
            var manCtx = CreateManutencao(db, tp, cu);
            var estCtx = CreateEstoque(db, tp, cu);

            var produto = new Produto("SKU-PECA", "Peça", 5m, tenantId, "seed");
            estCtx.Produtos.Add(produto);
            await estCtx.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(estCtx, tenantId, "seed", produto.Id, 10m, 5m);

            var osId = Guid.NewGuid();
            var payload = JsonSerializer.Serialize(new
            {
                OrdemManutencaoId = osId,
                EquipamentoId = Guid.NewGuid(),
                Pecas = new[] { new { ProdutoId = produto.Id, Quantidade = 4m } },
                TenantId = tenantId
            });
            manCtx.OutboxMessages.Add(new OutboxMessage(tenantId, "OrdemManutencaoConcluida", payload));
            await manCtx.SaveChangesAsync();

            var dispatcher = new OutboxDispatcher(
                new IOutboxConsumer[] { new OrdemManutencaoConcluidaEstoqueConsumer(estCtx) },
                NullLogger<OutboxDispatcher>.Instance);

            var roteadas = await dispatcher.ProcessAsync(manCtx);
            Assert.Equal(1, roteadas);

            var prodPos = await estCtx.Produtos.IgnoreQueryFilters().FirstAsync(p => p.Id == produto.Id);
            Assert.Equal(6m, prodPos.SaldoEstoque); // 10 - 4
            var movs = await estCtx.MovimentosEstoque.IgnoreQueryFilters().Where(m => m.Tipo == "Saida").ToListAsync();
            Assert.Single(movs);
            Assert.Contains($"Baixa OS {osId}", movs[0].Historico);

            // Idempotência.
            var roteadas2 = await dispatcher.ProcessAsync(manCtx);
            Assert.Equal(0, roteadas2);
            Assert.Single(await estCtx.MovimentosEstoque.IgnoreQueryFilters().Where(m => m.Tipo == "Saida").ToListAsync());
        }

        private ContextQualidade CreateQualidade(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextQualidade(new DbContextOptionsBuilder<ContextQualidade>().UseInMemoryDatabase("qld_" + db).Options, tp, cu);
        private ContextManutencao CreateManutencao(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextManutencao(new DbContextOptionsBuilder<ContextManutencao>().UseInMemoryDatabase("man_" + db).Options, tp, cu);
        private ContextEstoque CreateEstoque(string db, ITenantProvider tp, ICurrentUser cu)
            => new ContextEstoque(new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase("est_" + db).Options, tp, cu);

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }
        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u; public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test";
            public string? GetUserEmail() => "t@e.com";
        }
    }
}
