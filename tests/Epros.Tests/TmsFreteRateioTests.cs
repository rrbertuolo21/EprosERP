using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do rateio de frete de entrada no TMS (NF-04): apropriação sobre os itens da compra compondo o
    /// custo (motor D1), com idempotência por compra. Reusa a calculadora estrutural de landed. InMemory.
    /// </summary>
    public class TmsFreteRateioTests
    {
        private const string TenantId = "tenant-tms-001";
        private const string UserId = "user-tms-001";

        private ContextEstoque CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private static async Task<Guid> SeedCompraAsync(ContextEstoque ctx, params decimal[] valores)
        {
            var compra = new Compra("14200166000187", "Fornecedor", "3001",
                "35200114200166000187550010000000035123456789", valores.Sum(), DateTime.UtcNow, TenantId, UserId);
            ctx.Compras.Add(compra);
            foreach (var v in valores)
                ctx.CompraItens.Add(new CompraItem(compra.Id, Guid.NewGuid(), 1m, v, 0m, 0m, TenantId, UserId));
            await ctx.SaveChangesAsync();
            return compra.Id;
        }

        [Fact(DisplayName = "NF-04 | Frete positivo é rateado sobre os itens e publica evento de custo")]
        public async Task RatearFrete_Ok()
        {
            var ctx = CreateContext(nameof(RatearFrete_Ok));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var compraId = await SeedCompraAsync(ctx, 100m, 300m);

            var res = await new RatearFreteCompraCommandHandler(ctx, tenant, user)
                .Handle(new RatearFreteCompraCommand(compraId, 40m, ERateioLandedMetodo.PorValor), CancellationToken.None);
            Assert.True(res.Sucesso);
            Assert.True(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Estoque.TmsFreteRateado));

            // Idempotência: não rateia duas vezes.
            var again = await new RatearFreteCompraCommandHandler(ctx, tenant, user)
                .Handle(new RatearFreteCompraCommand(compraId, 40m, ERateioLandedMetodo.PorValor), CancellationToken.None);
            Assert.False(again.Sucesso);
        }

        [Fact(DisplayName = "NF-04 | Frete não-positivo é rejeitado (TMS-030)")]
        public async Task RatearFrete_ValorInvalido()
        {
            var ctx = CreateContext(nameof(RatearFrete_ValorInvalido));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var compraId = await SeedCompraAsync(ctx, 100m);

            var res = await new RatearFreteCompraCommandHandler(ctx, tenant, user)
                .Handle(new RatearFreteCompraCommand(compraId, 0m), CancellationToken.None);
            Assert.False(res.Sucesso);
        }
    }
}
