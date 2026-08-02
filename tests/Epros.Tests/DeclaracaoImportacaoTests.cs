using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// GAP 4 (Estoque/Comércio Exterior) — CQRS da Declaração de Importação (DI) por item de compra e suas
    /// adições (CEX-001..023). As entidades CompraItemImportacao/Adicao existiam sem command/handler/endpoint;
    /// aqui cobrimos o CRUD completo. valida-contador: montantes (AFRMM/desconto) são factuais.
    /// </summary>
    public class DeclaracaoImportacaoTests
    {
        private const string TenantId = "tenant-di"; private const string UserId = "user-di";

        private ContextEstoque CreateContext(string db)
            => new ContextEstoque(new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options,
                new TestTenantProvider(TenantId), new TestCurrentUser(UserId));

        private async Task<Guid> SeedItemAsync(ContextEstoque ctx)
        {
            var compra = new Compra("14200166000187", "Exportador X", "2001",
                "35200114200166000187550010000000025123456789", 100m, DateTime.UtcNow, TenantId, UserId);
            ctx.Compras.Add(compra);
            var item = new CompraItem(compra.Id, Guid.NewGuid(), 1m, 100m, 0m, 0m, TenantId, UserId);
            ctx.CompraItens.Add(item);
            await ctx.SaveChangesAsync();
            return item.Id;
        }

        private RegistrarDeclaracaoImportacaoCommand NovaDi(Guid itemId, string numero = "DI-0001")
            => new(itemId, numero, DateTime.UtcNow, "Porto de Santos", "SP", DateTime.UtcNow,
                ETipoViaTransporte.Maritima, 1234.56m, ETipoIntermedioImportacao.ContaPropria,
                Cnpj: "14200166000187", CodigoExportador: "EXP-1");

        [Fact]
        public async Task Registra_Altera_Lista_E_Exclui_DI_Com_Adicoes()
        {
            // Um DbContext novo por operação (mesma base InMemory), espelhando o escopo por-request de produção.
            var db = "db_di_" + Guid.NewGuid().ToString("N");
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);

            Guid itemId;
            using (var ctx = CreateContext(db)) itemId = await SeedItemAsync(ctx);

            // Registrar DI
            Guid diId;
            using (var ctx = CreateContext(db))
            {
                var rReg = await new RegistrarDeclaracaoImportacaoCommandHandler(ctx, tp, cu)
                    .Handle(NovaDi(itemId), CancellationToken.None);
                Assert.True(rReg.Sucesso, string.Join("; ", rReg.Erros));
                diId = (Guid)rReg.Dados!.GetType().GetProperty("Id")!.GetValue(rReg.Dados)!;
            }

            // Alterar DI
            using (var ctx = CreateContext(db))
            {
                var rAlt = await new AlterarDeclaracaoImportacaoCommandHandler(ctx, cu).Handle(
                    new AlterarDeclaracaoImportacaoCommand(diId, "DI-0001-B", DateTime.UtcNow, "Porto de Itajaí", "SC",
                        DateTime.UtcNow, ETipoViaTransporte.Aerea, 999m, ETipoIntermedioImportacao.ContaeOrdem),
                    CancellationToken.None);
                Assert.True(rAlt.Sucesso, string.Join("; ", rAlt.Erros));
            }

            // Adicionar adição
            Guid adicaoId;
            using (var ctx = CreateContext(db))
            {
                var rAd = await new AdicionarAdicaoImportacaoCommandHandler(ctx, cu).Handle(
                    new AdicionarAdicaoImportacaoCommand(diId, 1, 1, "FAB-1", 50m, "ATO-1"), CancellationToken.None);
                Assert.True(rAd.Sucesso, string.Join("; ", rAd.Erros));
                adicaoId = (Guid)rAd.Dados!.GetType().GetProperty("adicaoId")!.GetValue(rAd.Dados)!;
            }

            // Alterar adição
            using (var ctx = CreateContext(db))
            {
                var rAdAlt = await new AlterarAdicaoImportacaoCommandHandler(ctx, cu).Handle(
                    new AlterarAdicaoImportacaoCommand(diId, adicaoId, 2, 2, "FAB-2", 75m, null), CancellationToken.None);
                Assert.True(rAdAlt.Sucesso, string.Join("; ", rAdAlt.Erros));
            }

            // Listar (query) + verificar dados alterados
            using (var ctx = CreateContext(db))
            {
                var rList = await new ObterDeclaracoesImportacaoPorItemQueryHandler(ctx)
                    .Handle(new ObterDeclaracoesImportacaoPorItemQuery(itemId), CancellationToken.None);
                Assert.True(rList.Sucesso);

                var di = await ctx.CompraItemImportacoes.Include(d => d.Adicoes).FirstAsync(d => d.Id == diId);
                Assert.Equal("DI-0001-B", di.NumeroDeclaracaoImportacao);
                Assert.Equal(ETipoViaTransporte.Aerea, di.TipoViaTransporte);
                var ad = di.Adicoes.Single(a => a.DeletadoEm == null);
                Assert.Equal(2, ad.NumeroAdicao);
                Assert.Equal(75m, ad.ValorDesconto);
            }

            // Excluir adição
            using (var ctx = CreateContext(db))
            {
                var rAdDel = await new ExcluirAdicaoImportacaoCommandHandler(ctx, cu)
                    .Handle(new ExcluirAdicaoImportacaoCommand(diId, adicaoId), CancellationToken.None);
                Assert.True(rAdDel.Sucesso, string.Join("; ", rAdDel.Erros));
            }

            // Excluir DI (soft-delete)
            using (var ctx = CreateContext(db))
            {
                var rDel = await new ExcluirDeclaracaoImportacaoCommandHandler(ctx, cu)
                    .Handle(new ExcluirDeclaracaoImportacaoCommand(diId), CancellationToken.None);
                Assert.True(rDel.Sucesso, string.Join("; ", rDel.Erros));
            }

            using (var ctx = CreateContext(db))
            {
                var diPos = await ctx.CompraItemImportacoes.IgnoreQueryFilters().FirstAsync(d => d.Id == diId);
                Assert.NotNull(diPos.DeletadoEm);
            }
        }

        [Fact]
        public async Task Registrar_DI_Sem_Item_Existente_Falha()
        {
            var ctx = CreateContext(nameof(Registrar_DI_Sem_Item_Existente_Falha));
            var tp = new TestTenantProvider(TenantId); var cu = new TestCurrentUser(UserId);

            var r = await new RegistrarDeclaracaoImportacaoCommandHandler(ctx, tp, cu)
                .Handle(NovaDi(Guid.NewGuid()), CancellationToken.None);
            Assert.False(r.Sucesso);
        }

        [Fact]
        public async Task Adicionar_Adicao_Em_DI_Inexistente_Falha()
        {
            var ctx = CreateContext(nameof(Adicionar_Adicao_Em_DI_Inexistente_Falha));
            var cu = new TestCurrentUser(UserId);

            var r = await new AdicionarAdicaoImportacaoCommandHandler(ctx, cu)
                .Handle(new AdicionarAdicaoImportacaoCommand(Guid.NewGuid(), 1, 1, "FAB", 0m, null), CancellationToken.None);
            Assert.False(r.Sucesso);
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
            public string? GetUserName() => "Test";
            public string? GetUserEmail() => "t@e.com";
        }
    }
}
