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
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    /// <summary>
    /// Testes do pacote de relatórios de COMPRAS (CD7): curva ABC de fornecedor, savings de cotação,
    /// lead time de pedido e aderência de alçada. CQRS InMemory.
    /// </summary>
    public class ComprasRelatoriosTests
    {
        private const string TenantId = "tenant-rel-001";
        private const string UserId = "user-rel-001";

        private ContextEstoque CreateContext(string db)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>().UseInMemoryDatabase(db).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        [Fact(DisplayName = "CD7 | Curva ABC classifica fornecedores por participação acumulada")]
        public async Task CurvaAbc_Classifica()
        {
            var ctx = CreateContext(nameof(CurvaAbc_Classifica));
            // Fornecedor A domina (900) vs B (100).
            ctx.Compras.Add(new Compra("11111111111111", "Fornecedor A", "1", "35200114200166000187550010000000015123456781", 900m, DateTime.UtcNow, TenantId, UserId));
            ctx.Compras.Add(new Compra("22222222222222", "Fornecedor B", "2", "35200114200166000187550010000000015123456782", 100m, DateTime.UtcNow, TenantId, UserId));
            await ctx.SaveChangesAsync();

            var res = await new CurvaAbcFornecedorQueryHandler(ctx).Handle(new CurvaAbcFornecedorQuery(), CancellationToken.None);
            Assert.True(res.Sucesso);
            var total = (decimal)res.Dados!.GetType().GetProperty("TotalGeral")!.GetValue(res.Dados)!;
            Assert.Equal(1000m, total);
        }

        [Fact(DisplayName = "CD7 | Savings de cotação = mais caro menos vencedor")]
        public async Task Savings_Calcula()
        {
            var ctx = CreateContext(nameof(Savings_Calcula));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);

            var cot = new ScCotacao(DateTime.UtcNow, "Cotação", "ABERTA", TenantId, UserId);
            ctx.ScCotacoes.Add(cot);
            var fornA = new ScCotacaoFornecedor(cot.Id, Guid.NewGuid(), "10d", "30ddl", 100m, 0m, 100m, TenantId, UserId);
            var fornB = new ScCotacaoFornecedor(cot.Id, Guid.NewGuid(), "5d", "vista", 80m, 0m, 80m, TenantId, UserId);
            ctx.ScCotacaoFornecedores.AddRange(fornA, fornB);
            await ctx.SaveChangesAsync();

            // Vencedor = B (80). Economia vs mais caro (100) = 20.
            await new SelecionarVencedorCotacaoCommandHandler(ctx, tenant, user)
                .Handle(new SelecionarVencedorCotacaoCommand(cot.Id, fornB.FornecedorId), CancellationToken.None);

            var res = await new SavingsCotacaoQueryHandler(ctx).Handle(new SavingsCotacaoQuery(), CancellationToken.None);
            Assert.True(res.Sucesso);
            var savings = (decimal)res.Dados!.GetType().GetProperty("SavingsTotalVsMaisCaro")!.GetValue(res.Dados)!;
            Assert.Equal(20m, savings);
        }

        [Fact(DisplayName = "CD7 | Aderência de alçada reporta taxa de aprovação")]
        public async Task Aderencia_Alcada()
        {
            var ctx = CreateContext(nameof(Aderencia_Alcada));
            var tenant = new TestTenantProvider(TenantId);
            var user = new TestCurrentUser(UserId);

            // Sem regra aplicável a 100 → nasce aprovado.
            await new SolicitarAprovacaoCompraCommandHandler(ctx, tenant, user)
                .Handle(new SolicitarAprovacaoCompraCommand(EOrigemAprovacaoCompra.Compra, Guid.NewGuid(), 100m), CancellationToken.None);

            var res = await new AderenciaAlcadaQueryHandler(ctx).Handle(new AderenciaAlcadaQuery(), CancellationToken.None);
            Assert.True(res.Sucesso);
            var total = (int)res.Dados!.GetType().GetProperty("Total")!.GetValue(res.Dados)!;
            var aprovados = (int)res.Dados!.GetType().GetProperty("Aprovados")!.GetValue(res.Dados)!;
            Assert.Equal(1, total);
            Assert.Equal(1, aprovados);
        }

        [Fact(DisplayName = "CD7 | Lead time médio calcula a diferença pedido→entrega")]
        public async Task LeadTime_Medio()
        {
            var ctx = CreateContext(nameof(LeadTime_Medio));
            var pedido = new ScPedidoCompra(Guid.NewGuid(), Guid.NewGuid(), null,
                DateTime.UtcNow, DateTime.UtcNow.AddDays(10), null, "Entrega", "Cobranca", "Contato", "Boleto", null, null, null, TenantId, UserId);
            ctx.ScPedidosCompra.Add(pedido);
            await ctx.SaveChangesAsync();

            var res = await new LeadTimeComprasQueryHandler(ctx).Handle(new LeadTimeComprasQuery(), CancellationToken.None);
            Assert.True(res.Sucesso);
            var media = (decimal)res.Dados!.GetType().GetProperty("LeadTimeMedioDias")!.GetValue(res.Dados)!;
            Assert.Equal(10m, media);
        }
    }
}
