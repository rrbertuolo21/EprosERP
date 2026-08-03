using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Events;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// GAP 3 (Estoque/LDE) — ANTI-DUPLA-CONTAGEM do crédito de estoque de compra. O lançamento fiscal
    /// (LancarCompra) é a ÚNICA autoridade que credita o kardex; a confirmação da Logística de Entrada (LDE)
    /// só publica eventos e NUNCA recredita. Chave de idempotência do crédito = ORIGEM (CompraId).
    /// </summary>
    public class LdeAntiDuplaContagemTests
    {
        private const string TenantId = "tenant-lde-adc";
        private const string UserId = "user-lde-adc";

        [Fact]
        public async Task LDE_Nao_Recredita_Estoque_Ja_Creditado_Por_LancarCompra()
        {
            var ctx = CreateContext(nameof(LDE_Nao_Recredita_Estoque_Ja_Creditado_Por_LancarCompra));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);

            // 1. Lançamento fiscal credita o estoque UMA vez (fato gerador com CompraId).
            var lancar = new LancarCompraCommandHandler(ctx, tp, cu);
            var rLancar = await lancar.Handle(new LancarCompraCommand(
                "12345678000199", "Fornecedor X", "000123", "22222222222222222222222222222222222222222222",
                1000m, DateTime.UtcNow,
                new List<ItemCompraInput> { new("SKU-ADC", "Produto ADC", 10m, 100m, 0m, 0m) }), CancellationToken.None);
            Assert.True(rLancar.Sucesso, rLancar.Mensagem);
            var compraId = (Guid)rLancar.Dados!.GetType().GetProperty("CompraId")!.GetValue(rLancar.Dados)!;

            var fichasAposLancamento = await ctx.ProdutoFichaEstoqueEntradas.IgnoreQueryFilters().CountAsync();
            var fatosAposLancamento = await ctx.FatosGeradoresEstoque.IgnoreQueryFilters().CountAsync(f => f.CompraId == compraId);
            Assert.Equal(1, fatosAposLancamento);
            Assert.Equal(1, fichasAposLancamento);

            // 2. LDE do MESMO compraId: cria entrada, vincula documento e confirma.
            var criar = new CriarLdeEntradaCommandHandler(ctx, tp, cu);
            var rCriar = await criar.Handle(new CriarLdeEntradaCommand(compraId, Guid.NewGuid()), CancellationToken.None);
            var entradaId = (Guid)rCriar.Dados!.GetType().GetProperty("Id")!.GetValue(rCriar.Dados)!;

            var vincular = new VincularLdeDocumentoCommandHandler(ctx, tp, cu);
            await vincular.Handle(new VincularLdeDocumentoCommand(
                EntradaId: entradaId, ChaveAcesso: "33333333333333333333333333333333333333333333",
                Numero: "000123", Serie: "1", DataEmissao: DateTime.UtcNow, NaturezaOperacao: "Compra",
                ValorTotal: 1000m, FornecedorId: Guid.NewGuid(), DestinatarioId: Guid.NewGuid(), EmitenteId: Guid.NewGuid(),
                Itens: new List<LdeDocumentoItemInput> { new(Guid.NewGuid(), 10m, 100m, null) },
                Duplicatas: new List<LdeDocumentoDuplicataInput>()), CancellationToken.None);

            var confirmar = new ConfirmarLdeEntradaCommandHandler(ctx, tp, cu);
            var rConf = await confirmar.Handle(new ConfirmarLdeEntradaCommand(entradaId), CancellationToken.None);
            Assert.True(rConf.Sucesso, rConf.Mensagem);

            // 3. A LDE NÃO tocou o kardex: nenhuma ficha nova, nenhum fato gerador novo para a compra.
            Assert.Equal(fichasAposLancamento, await ctx.ProdutoFichaEstoqueEntradas.IgnoreQueryFilters().CountAsync());
            Assert.Equal(1, await ctx.FatosGeradoresEstoque.IgnoreQueryFilters().CountAsync(f => f.CompraId == compraId));

            // 4. O evento MercadoriaRecebida sinaliza que o estoque JÁ foi creditado (anti-dupla-contagem).
            var msg = await ctx.OutboxMessages.AsNoTracking()
                .FirstAsync(o => o.EventType == CatalogoEventosIntegracao.Estoque.MercadoriaRecebida);
            Assert.Contains("\"estoqueJaCreditado\":true", msg.Payload);
        }

        [Fact]
        public async Task LDE_Sem_Lancamento_Fiscal_Sinaliza_Estoque_Nao_Creditado()
        {
            var ctx = CreateContext(nameof(LDE_Sem_Lancamento_Fiscal_Sinaliza_Estoque_Nao_Creditado));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);

            var compraId = Guid.NewGuid(); // nenhuma compra fiscal lançada para este id
            var criar = new CriarLdeEntradaCommandHandler(ctx, tp, cu);
            var rCriar = await criar.Handle(new CriarLdeEntradaCommand(compraId, Guid.NewGuid()), CancellationToken.None);
            var entradaId = (Guid)rCriar.Dados!.GetType().GetProperty("Id")!.GetValue(rCriar.Dados)!;

            var vincular = new VincularLdeDocumentoCommandHandler(ctx, tp, cu);
            await vincular.Handle(new VincularLdeDocumentoCommand(
                EntradaId: entradaId, ChaveAcesso: "44444444444444444444444444444444444444444444",
                Numero: "000777", Serie: "1", DataEmissao: DateTime.UtcNow, NaturezaOperacao: "Compra",
                ValorTotal: 500m, FornecedorId: Guid.NewGuid(), DestinatarioId: Guid.NewGuid(), EmitenteId: Guid.NewGuid(),
                Itens: new List<LdeDocumentoItemInput> { new(Guid.NewGuid(), 5m, 100m, null) },
                Duplicatas: new List<LdeDocumentoDuplicataInput>()), CancellationToken.None);

            var confirmar = new ConfirmarLdeEntradaCommandHandler(ctx, tp, cu);
            var rConf = await confirmar.Handle(new ConfirmarLdeEntradaCommand(entradaId), CancellationToken.None);
            Assert.True(rConf.Sucesso, rConf.Mensagem);

            var msg = await ctx.OutboxMessages.AsNoTracking()
                .FirstAsync(o => o.EventType == CatalogoEventosIntegracao.Estoque.MercadoriaRecebida);
            Assert.Contains("\"estoqueJaCreditado\":false", msg.Payload);
        }

        private ContextEstoque CreateContext(string dbName)
            => new ContextEstoque(new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase("db_lde_adc_" + dbName).Options,
                new TestTenantProvider(TenantId), new TestCurrentUser(UserId));

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
