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
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Domain.Events;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do mapa comparativo de cotação multi-fornecedor (CD2 / EF SOURCING): construção do mapa
    /// (menor preço por produto) e a escolha do vencedor (SRC-020/021) com evento de decisão. CQRS InMemory.
    /// </summary>
    public class SourcingMapaComparativoTests
    {
        private const string TenantId = "tenant-src-001";
        private const string UserId = "user-src-001";

        private ContextEstoque CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        /// <summary>Semeia uma cotação com 2 fornecedores cotando o mesmo produto a preços diferentes.</summary>
        private static async Task<(Guid cotacaoId, Guid fornAId, Guid fornBId, Guid produtoId)> SeedCotacaoAsync(ContextEstoque ctx)
        {
            var cot = new ScCotacao(DateTime.UtcNow, "Cotação MRO", "ABERTA", TenantId, UserId);
            ctx.ScCotacoes.Add(cot);

            var fornA = new ScCotacaoFornecedor(cot.Id, Guid.NewGuid(), "10 dias", "30 ddl", 100m, 0m, 100m, TenantId, UserId);
            var fornB = new ScCotacaoFornecedor(cot.Id, Guid.NewGuid(), "5 dias", "à vista", 90m, 0m, 90m, TenantId, UserId);
            ctx.ScCotacaoFornecedores.AddRange(fornA, fornB);

            var produtoId = Guid.NewGuid();
            ctx.ScCotacaoItens.Add(new ScCotacaoItem(cot.Id, fornA.Id, produtoId, 10m, 10m, 0m, 100m, TenantId, UserId));
            ctx.ScCotacaoItens.Add(new ScCotacaoItem(cot.Id, fornB.Id, produtoId, 10m, 9m, 0m, 90m, TenantId, UserId));
            await ctx.SaveChangesAsync();
            return (cot.Id, fornA.FornecedorId, fornB.FornecedorId, produtoId);
        }

        [Fact(DisplayName = "CD2 | Mapa comparativo aponta o menor preço unitário por produto")]
        public async Task Mapa_ApontaMenorPreco()
        {
            var ctx = CreateContext(nameof(Mapa_ApontaMenorPreco));
            var (cotacaoId, _, fornBId, _) = await SeedCotacaoAsync(ctx);

            var res = await new MapaComparativoCotacaoQueryHandler(ctx)
                .Handle(new MapaComparativoCotacaoQuery(cotacaoId), CancellationToken.None);
            Assert.True(res.Sucesso);

            // O melhor preço do produto é o fornecedor B (9 < 10).
            var linhas = res.Dados!.GetType().GetProperty("Linhas")!.GetValue(res.Dados)!;
            var primeira = ((System.Collections.IEnumerable)linhas).Cast<object>().First();
            var melhor = (Guid?)primeira.GetType().GetProperty("MelhorPrecoFornecedorId")!.GetValue(primeira);
            Assert.Equal(fornBId, melhor);
        }

        [Fact(DisplayName = "CD2 | Selecionar vencedor válido decide a cotação e publica evento")]
        public async Task SelecionarVencedor_Ok()
        {
            var ctx = CreateContext(nameof(SelecionarVencedor_Ok));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (cotacaoId, _, fornBId, _) = await SeedCotacaoAsync(ctx);

            var res = await new SelecionarVencedorCotacaoCommandHandler(ctx, tenant, user)
                .Handle(new SelecionarVencedorCotacaoCommand(cotacaoId, fornBId), CancellationToken.None);
            Assert.True(res.Sucesso);

            var cot = await ctx.ScCotacoes.FirstAsync(c => c.Id == cotacaoId);
            Assert.Equal(fornBId, cot.FornecedorVencedorId);
            Assert.Equal(ScCotacao.SituacaoDecidida, cot.Situacao);
            Assert.True(await ctx.OutboxMessages.AnyAsync(o => o.EventType == CatalogoEventosIntegracao.Estoque.ScCotacaoDecidida));
        }

        [Fact(DisplayName = "CD2 | Vencedor fora da cotação é rejeitado (SRC-021); não decide duas vezes (SRC-020)")]
        public async Task SelecionarVencedor_Invalido()
        {
            var ctx = CreateContext(nameof(SelecionarVencedor_Invalido));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);
            var (cotacaoId, _, fornBId, _) = await SeedCotacaoAsync(ctx);

            // Fornecedor que não participa da cotação → rejeitado.
            var estranho = await new SelecionarVencedorCotacaoCommandHandler(ctx, tenant, user)
                .Handle(new SelecionarVencedorCotacaoCommand(cotacaoId, Guid.NewGuid()), CancellationToken.None);
            Assert.False(estranho.Sucesso);

            // Decide uma vez...
            await new SelecionarVencedorCotacaoCommandHandler(ctx, tenant, user)
                .Handle(new SelecionarVencedorCotacaoCommand(cotacaoId, fornBId), CancellationToken.None);
            // ...e não decide de novo.
            var denovo = await new SelecionarVencedorCotacaoCommandHandler(ctx, tenant, user)
                .Handle(new SelecionarVencedorCotacaoCommand(cotacaoId, fornBId), CancellationToken.None);
            Assert.False(denovo.Sucesso);
        }
    }
}
