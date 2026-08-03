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
    /// Testes de CONTRATOS_GCC (CD5): aditivo contratual (vigência/preço) sobre contrato aprovado e a
    /// query de performance (aderência consumido/comprometido, saldo, vigência). CQRS InMemory.
    /// </summary>
    public class GccAditivoPerformanceTests
    {
        private const string TenantId = "tenant-gcc-001";
        private const string UserId = "user-gcc-001";

        private ContextEstoque CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        /// <summary>Cria um contrato APROVADO com um item comprometido e retorna (contratoId, itemId).</summary>
        private static async Task<(Guid contratoId, Guid itemId)> SeedContratoAprovadoAsync(ContextEstoque ctx, TestTenantProvider tenant, TestCurrentUser user, decimal comprometido = 100m, decimal preco = 10m)
        {
            var criar = await new CriarGccContratoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarGccContratoCompraCommand(Guid.NewGuid(), "CT-1", DateTime.UtcNow, DateTime.UtcNow.AddMonths(6), 1000m, null,
                    new List<GccContratoItemInput> { new(Guid.NewGuid(), preco, comprometido) }), CancellationToken.None);
            var contratoId = (Guid)criar.Dados!.GetType().GetProperty("Id")!.GetValue(criar.Dados)!;
            await new AprovarGccContratoCompraCommandHandler(ctx, tenant, user).Handle(new AprovarGccContratoCompraCommand(contratoId), CancellationToken.None);
            var item = await ctx.GccContratosCompraItens.FirstAsync(i => i.ContratoCompraId == contratoId);
            return (contratoId, item.Id);
        }

        [Fact(DisplayName = "CD5 | Aditivo em contrato NÃO aprovado é rejeitado (GCC-022)")]
        public async Task Aditivo_ContratoNaoAprovado_Falha()
        {
            var ctx = CreateContext(nameof(Aditivo_ContratoNaoAprovado_Falha));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var criar = await new CriarGccContratoCompraCommandHandler(ctx, tenant, user).Handle(
                new CriarGccContratoCompraCommand(Guid.NewGuid(), "CT-9", DateTime.UtcNow, DateTime.UtcNow.AddMonths(3), 500m, null,
                    new List<GccContratoItemInput> { new(Guid.NewGuid(), 10m, 50m) }), CancellationToken.None);
            var contratoId = (Guid)criar.Dados!.GetType().GetProperty("Id")!.GetValue(criar.Dados)!;

            var res = await new RegistrarGccAditivoCommandHandler(ctx, tenant, user).Handle(
                new RegistrarGccAditivoCommand(contratoId, ETipoAditivoContrato.Vigencia, NovaVigenciaFim: DateTime.UtcNow.AddYears(1)),
                CancellationToken.None);
            Assert.False(res.Sucesso);
        }

        [Fact(DisplayName = "CD5 | Aditivo de vigência estende a data-fim e publica evento")]
        public async Task Aditivo_Vigencia_Estende()
        {
            var ctx = CreateContext(nameof(Aditivo_Vigencia_Estende));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (contratoId, _) = await SeedContratoAprovadoAsync(ctx, tenant, user);
            var novaFim = DateTime.UtcNow.AddYears(1).Date;

            var res = await new RegistrarGccAditivoCommandHandler(ctx, tenant, user).Handle(
                new RegistrarGccAditivoCommand(contratoId, ETipoAditivoContrato.Vigencia, NumeroAditivo: "A1", NovaVigenciaFim: novaFim),
                CancellationToken.None);
            Assert.True(res.Sucesso);

            var contrato = await ctx.GccContratosCompra.FirstAsync(c => c.Id == contratoId);
            Assert.Equal(novaFim, contrato.VigenciaFim!.Value.Date);
            Assert.True(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Estoque.GccAditivoRegistrado));
        }

        [Fact(DisplayName = "CD5 | Aditivo de preço por item ajusta o preço e recompõe o saldo em valor")]
        public async Task Aditivo_Preco_AjustaItem()
        {
            var ctx = CreateContext(nameof(Aditivo_Preco_AjustaItem));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (contratoId, itemId) = await SeedContratoAprovadoAsync(ctx, tenant, user, comprometido: 100m, preco: 10m);

            var res = await new RegistrarGccAditivoCommandHandler(ctx, tenant, user).Handle(
                new RegistrarGccAditivoCommand(contratoId, ETipoAditivoContrato.Preco, ContratoCompraItemId: itemId, NovoPreco: 12m),
                CancellationToken.None);
            Assert.True(res.Sucesso);

            var item = await ctx.GccContratosCompraItens.FirstAsync(i => i.Id == itemId);
            Assert.Equal(12m, item.PrecoUnitario);
            Assert.Equal(1200m, item.SaldoValor); // 12 * 100 comprometido, nada consumido
        }

        [Fact(DisplayName = "CD5 | Performance calcula aderência consumido/comprometido e saldo")]
        public async Task Performance_CalculaAderencia()
        {
            var ctx = CreateContext(nameof(Performance_CalculaAderencia));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (contratoId, itemId) = await SeedContratoAprovadoAsync(ctx, tenant, user, comprometido: 100m, preco: 10m);

            // Consome 25 de 100 (25%).
            await new RegistrarGccConsumoContratoCommandHandler(ctx, tenant, user).Handle(
                new RegistrarGccConsumoContratoCommand(contratoId, itemId, null, 25m, 250m), CancellationToken.None);

            var res = await new PerformanceGccContratoQueryHandler(ctx)
                .Handle(new PerformanceGccContratoQuery(contratoId), CancellationToken.None);
            Assert.True(res.Sucesso);

            var aderencia = (decimal)res.Dados!.GetType().GetProperty("AderenciaGlobalPercent")!.GetValue(res.Dados)!;
            var saldo = (decimal)res.Dados!.GetType().GetProperty("SaldoTotal")!.GetValue(res.Dados)!;
            Assert.Equal(25m, aderencia);   // 250 consumido / 1000 comprometido
            Assert.Equal(750m, saldo);
        }
    }
}
