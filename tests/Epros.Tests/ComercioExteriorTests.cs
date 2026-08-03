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
using Epros.Modules.Estoque.Domain.Services;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do submódulo Comércio Exterior / Importação (CD1 / EF COMERCIO_EXTERIOR): incoterm/moeda/câmbio
    /// na compra (CEX-005), rateio landed parametrizável desligado por padrão (CEX-003/NF-02), a calculadora
    /// estrutural de rateio e a nacionalização com eventos (entrada D1 + títulos, CEX-006). CQRS InMemory.
    /// </summary>
    public class ComercioExteriorTests
    {
        private const string TenantId = "tenant-cex-001";
        private const string UserId = "user-cex-001";

        private ContextEstoque CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private static async Task<Guid> SeedCompraAsync(ContextEstoque ctx, params decimal[] valoresItens)
        {
            var compra = new Compra("14200166000187", "Exportador X", "2001",
                "35200114200166000187550010000000025123456789", valoresItens.Sum(), DateTime.UtcNow, TenantId, UserId);
            ctx.Compras.Add(compra);
            foreach (var v in valoresItens)
                ctx.CompraItens.Add(new CompraItem(compra.Id, Guid.NewGuid(), 1m, v, 0m, 0m, TenantId, UserId));
            await ctx.SaveChangesAsync();
            return compra.Id;
        }

        // ===================== Calculadora estrutural =====================

        [Fact(DisplayName = "RateioLanded | PorValor distribui proporcional e fecha o montante (resíduo na última)")]
        public void Rateio_PorValor_Fecha()
        {
            var itens = new List<RateioLandedItem>
            {
                new(Guid.NewGuid(), 100m, 1m, 1m),
                new(Guid.NewGuid(), 200m, 1m, 1m),
                new(Guid.NewGuid(), 300m, 1m, 1m),
            };
            var r = RateioLandedCalculadora.Ratear(itens, 60m, ERateioLandedMetodo.PorValor);
            Assert.Equal(60m, r.Sum(x => x.ParcelaLanded));       // fecha exatamente o montante
            Assert.Equal(10m, r[0].ParcelaLanded);                // 100/600 * 60
            Assert.Equal(110m, r[0].CustoFinal);                  // base + landed
        }

        [Fact(DisplayName = "RateioLanded | Chave total zero distribui igualmente sem perder o montante")]
        public void Rateio_ChaveZero_Igual()
        {
            var itens = new List<RateioLandedItem>
            {
                new(Guid.NewGuid(), 0m, 0m, 0m),
                new(Guid.NewGuid(), 0m, 0m, 0m),
            };
            var r = RateioLandedCalculadora.Ratear(itens, 50m, ERateioLandedMetodo.PorPeso);
            Assert.Equal(50m, r.Sum(x => x.ParcelaLanded));
        }

        // ===================== Compra: incoterm/moeda/câmbio =====================

        [Fact(DisplayName = "CQRS | Define incoterm/moeda/câmbio na compra (CEX-005)")]
        public async Task DefinirComercioExterior_Ok()
        {
            var ctx = CreateContext(nameof(DefinirComercioExterior_Ok));
            var user = new TestCurrentUser(UserId);
            var compraId = await SeedCompraAsync(ctx, 500m);

            var res = await new DefinirComercioExteriorCompraCommandHandler(ctx, user).Handle(
                new DefinirComercioExteriorCompraCommand(compraId, EIncotermCompra.FOB, "usd", 5.25m), CancellationToken.None);

            Assert.True(res.Sucesso);
            var compra = await ctx.Compras.FirstAsync(c => c.Id == compraId);
            Assert.Equal(EIncotermCompra.FOB, compra.Incoterm);
            Assert.Equal("USD", compra.Moeda);
            Assert.Equal(5.25m, compra.TaxaCambio);
        }

        [Fact(DisplayName = "CQRS | Câmbio não-positivo é rejeitado (CEX-005)")]
        public async Task DefinirComercioExterior_CambioInvalido()
        {
            var ctx = CreateContext(nameof(DefinirComercioExterior_CambioInvalido));
            var user = new TestCurrentUser(UserId);
            var compraId = await SeedCompraAsync(ctx, 500m);

            var res = await new DefinirComercioExteriorCompraCommandHandler(ctx, user).Handle(
                new DefinirComercioExteriorCompraCommand(compraId, EIncotermCompra.CIF, "USD", 0m), CancellationToken.None);
            Assert.False(res.Sucesso);
        }

        // ===================== Rateio config (default desligado) =====================

        [Fact(DisplayName = "Query | Sem config persistida devolve o padrão SEGURO (desligado, NF-02)")]
        public async Task RateioConfig_Padrao_Desligado()
        {
            var ctx = CreateContext(nameof(RateioConfig_Padrao_Desligado));
            var res = await new ObterRateioLandedConfigQueryHandler(ctx)
                .Handle(new ObterRateioLandedConfigQuery(null), CancellationToken.None);
            Assert.True(res.Sucesso);
            var habilitado = (bool)res.Dados!.GetType().GetProperty("Habilitado")!.GetValue(res.Dados)!;
            Assert.False(habilitado);
        }

        // ===================== Nacionalização =====================

        [Fact(DisplayName = "CQRS | Nacionalizar com landed DESLIGADO: custo = valor do item, sem parcela landed")]
        public async Task Nacionalizar_LandedDesligado()
        {
            var ctx = CreateContext(nameof(Nacionalizar_LandedDesligado));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var compraId = await SeedCompraAsync(ctx, 100m, 300m);

            var res = await new NacionalizarImportacaoCommandHandler(ctx, tenant, user).Handle(
                new NacionalizarImportacaoCommand(compraId, MontanteTributos: 40m, MontanteFrete: 20m), CancellationToken.None);
            Assert.True(res.Sucesso);

            var evt = await ctx.OutboxMessages.FirstAsync(o => o.EventType == CatalogoEventosIntegracao.Compras.ImportacaoNacionalizada);
            Assert.Contains("\"landedAplicado\":false", evt.Payload);
            // Sem landed, ainda publica títulos financeiros (tributos+frete factuais).
            Assert.True(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Compras.ImportacaoTitulosFinanceiros));
        }

        [Fact(DisplayName = "CQRS | Nacionalizar com landed LIGADO compõe o custo e é idempotente (CEX-006)")]
        public async Task Nacionalizar_LandedLigado_Idempotente()
        {
            var ctx = CreateContext(nameof(Nacionalizar_LandedLigado_Idempotente));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var compraId = await SeedCompraAsync(ctx, 100m, 300m);

            // Habilita rateio (tributos + frete) por valor.
            await new SalvarRateioLandedConfigCommandHandler(ctx, tenant, user).Handle(
                new SalvarRateioLandedConfigCommand(true, true, true, false, ERateioLandedMetodo.PorValor), CancellationToken.None);

            var res = await new NacionalizarImportacaoCommandHandler(ctx, tenant, user).Handle(
                new NacionalizarImportacaoCommand(compraId, MontanteTributos: 40m, MontanteFrete: 40m), CancellationToken.None);
            Assert.True(res.Sucesso);
            var evt = await ctx.OutboxMessages.FirstAsync(o => o.EventType == CatalogoEventosIntegracao.Compras.ImportacaoNacionalizada);
            Assert.Contains("\"landedAplicado\":true", evt.Payload);

            // Idempotência: nacionalizar de novo falha (CEX-006).
            var again = await new NacionalizarImportacaoCommandHandler(ctx, tenant, user).Handle(
                new NacionalizarImportacaoCommand(compraId, MontanteTributos: 40m, MontanteFrete: 40m), CancellationToken.None);
            Assert.False(again.Sucesso);
        }
    }
}
