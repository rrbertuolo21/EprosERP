using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// EST-RLT — Rastreabilidade de Lote e Serialização. Invariantes de domínio (RLT-003/015/016/017),
    /// unicidade de serial (RLT-005), recall que bloqueia o lote (RLT-008) e a sugestão FEFO (D11 — sugere).
    /// </summary>
    public class RastreabilidadeTests
    {
        private const string TenantId = "tenant-rlt";
        private const string UserId = "user-rlt";

        // ---------- Domínio ----------

        [Fact(DisplayName = "LoteEstoque | Código obrigatório e quantidade > 0 (RLT-003)")]
        public void Lote_Invariantes()
        {
            Assert.False(new LoteEstoque(Guid.Empty, Guid.NewGuid(), "", 10m, null, null, null, null, EOrigemLote.Compra, null, TenantId, UserId).IsValid);
            Assert.False(new LoteEstoque(Guid.Empty, Guid.NewGuid(), "L1", 0m, null, null, null, null, EOrigemLote.Compra, null, TenantId, UserId).IsValid);
            Assert.True(new LoteEstoque(Guid.Empty, Guid.NewGuid(), "L1", 10m, null, null, null, null, EOrigemLote.Compra, null, TenantId, UserId).IsValid);
        }

        [Fact(DisplayName = "LoteEstoque | Consumo não pode exceder disponível (RLT-017)")]
        public void Lote_Consumir_Excede_Lanca()
        {
            var lote = new LoteEstoque(Guid.Empty, Guid.NewGuid(), "L1", 10m, null, null, null, null, EOrigemLote.Compra, null, TenantId, UserId);
            lote.Consumir(4m, UserId);
            Assert.Equal(6m, lote.QuantidadeDisponivel);
            Assert.Equal(4m, lote.QuantidadeConsumida);
            Assert.Throws<InvalidOperationException>(() => lote.Consumir(7m, UserId));
        }

        [Fact(DisplayName = "LoteEstoque | Bloqueado não pode ser consumido (RLT-007)")]
        public void Lote_Bloqueado_NaoConsome()
        {
            var lote = new LoteEstoque(Guid.Empty, Guid.NewGuid(), "L1", 10m, null, null, null, null, EOrigemLote.Compra, null, TenantId, UserId);
            lote.Bloquear(null, UserId);
            Assert.False(lote.PodeConsumir());
            Assert.Throws<InvalidOperationException>(() => lote.Consumir(1m, UserId));
        }

        [Fact(DisplayName = "NumeroSerial | Consumido não reutiliza (RLT-015) e bloqueado não sai (RLT-016)")]
        public void Serial_Consumo_Invariantes()
        {
            var s = new NumeroSerial(Guid.NewGuid(), "SN-1", null, null, TenantId, UserId);
            s.Consumir(Guid.NewGuid(), UserId);
            Assert.Equal(EStatusNumeroSerial.Consumido, s.Status);
            Assert.Throws<InvalidOperationException>(() => s.Consumir(Guid.NewGuid(), UserId));

            var s2 = new NumeroSerial(Guid.NewGuid(), "SN-2", null, null, TenantId, UserId);
            s2.Bloquear(UserId);
            Assert.Throws<InvalidOperationException>(() => s2.Consumir(Guid.NewGuid(), UserId));
        }

        // ---------- Integração ----------

        [Fact(DisplayName = "RLT | Serial duplicado no produto é bloqueado (RLT-005)")]
        public async Task Serial_Duplicado_Bloqueado()
        {
            var ctx = CreateContext(nameof(Serial_Duplicado_Bloqueado));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var produtoId = Guid.NewGuid();
            var h = new RegistrarNumeroSerialCommandHandler(ctx, tp, cu);

            var r1 = await h.Handle(new RegistrarNumeroSerialCommand(produtoId, "IMEI-123"), CancellationToken.None);
            Assert.True(r1.Sucesso);
            var r2 = await h.Handle(new RegistrarNumeroSerialCommand(produtoId, "IMEI-123"), CancellationToken.None);
            Assert.False(r2.Sucesso);
        }

        [Fact(DisplayName = "RLT | FEFO sugere o lote de menor validade primeiro (D11)")]
        public async Task Fefo_Sugere_MenorValidade()
        {
            var ctx = CreateContext(nameof(Fefo_Sugere_MenorValidade));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var empresa = Guid.NewGuid(); var produtoId = Guid.NewGuid();
            var criar = new CriarLoteCommandHandler(ctx, tp, cu);

            await criar.Handle(new CriarLoteCommand(empresa, produtoId, "LOTE-TARDE", 5m, EOrigemLote.Compra, DataValidade: DateTime.UtcNow.AddDays(60)), CancellationToken.None);
            await criar.Handle(new CriarLoteCommand(empresa, produtoId, "LOTE-CEDO", 5m, EOrigemLote.Compra, DataValidade: DateTime.UtcNow.AddDays(10)), CancellationToken.None);

            var q = new SugerirLoteFefoQueryHandler(ctx);
            var r = await q.Handle(new SugerirLoteFefoQuery(empresa, produtoId, 4m), CancellationToken.None);
            Assert.True(r.Sucesso);

            var sugestao = r.Dados!.GetType().GetProperty("Sugestao")!.GetValue(r.Dados)!;
            var lista = ((System.Collections.IEnumerable)sugestao).Cast<object>().ToList();
            var primeiro = lista[0];
            var codigo = (string)primeiro.GetType().GetProperty("CodigoLote")!.GetValue(primeiro)!;
            var qtdSug = (decimal)primeiro.GetType().GetProperty("QuantidadeSugerida")!.GetValue(primeiro)!;
            Assert.Equal("LOTE-CEDO", codigo);   // menor validade primeiro
            Assert.Equal(4m, qtdSug);            // 4 cabem no primeiro lote (5 disp)
            Assert.True((bool)r.Dados.GetType().GetProperty("Atendido")!.GetValue(r.Dados)!);
        }

        [Fact(DisplayName = "RLT | Abrir recall bloqueia o lote e emite evento (RLT-008)")]
        public async Task Recall_Bloqueia_Lote()
        {
            var ctx = CreateContext(nameof(Recall_Bloqueia_Lote));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var empresa = Guid.NewGuid(); var produtoId = Guid.NewGuid();

            var rCriar = await new CriarLoteCommandHandler(ctx, tp, cu).Handle(
                new CriarLoteCommand(empresa, produtoId, "LOTE-R", 10m, EOrigemLote.Compra), CancellationToken.None);
            var loteId = (Guid)rCriar.Dados!.GetType().GetProperty("Id")!.GetValue(rCriar.Dados)!;

            var rRecall = await new AbrirRecallLoteCommandHandler(ctx, tp, cu).Handle(
                new AbrirRecallLoteCommand(loteId, "Contaminação detectada"), CancellationToken.None);
            Assert.True(rRecall.Sucesso);

            var lote = await ctx.LotesEstoque.AsNoTracking().FirstAsync(l => l.Id == loteId);
            Assert.Equal(EStatusLoteRastreabilidade.Bloqueado, lote.Status);
            Assert.False(lote.PodeConsumir());

            var eventos = await ctx.OutboxMessages.AsNoTracking().Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Estoque.RecallAberto, eventos);
            Assert.True(CatalogoEventosIntegracao.EhEventoConhecido(CatalogoEventosIntegracao.Estoque.RecallAberto));
        }

        [Fact(DisplayName = "RLT | Bloquear lote sem motivo é rejeitado (RLT-012)")]
        public async Task Bloquear_SemMotivo_Rejeitado()
        {
            var ctx = CreateContext(nameof(Bloquear_SemMotivo_Rejeitado));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);
            var rCriar = await new CriarLoteCommandHandler(ctx, tp, cu).Handle(
                new CriarLoteCommand(Guid.NewGuid(), Guid.NewGuid(), "L", 5m, EOrigemLote.Manual), CancellationToken.None);
            var loteId = (Guid)rCriar.Dados!.GetType().GetProperty("Id")!.GetValue(rCriar.Dados)!;

            var r = await new BloquearLoteCommandHandler(ctx, tp, cu).Handle(
                new BloquearLoteCommand(loteId, ETipoBloqueioLote.Qualidade, "  "), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        private ContextEstoque CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase("db_rlt_" + dbName).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t; public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u; public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
