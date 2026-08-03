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
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Infrastructure.Data;

namespace Epros.Tests
{
    /// <summary>
    /// EST-INV — Inventário Físico e Contagem Cíclica. Cobre invariantes de domínio (divergência D3,
    /// máquina de estados) e o fluxo completo criar → contar → aprovar (acurácia D14) → aplicar ajuste
    /// por diferença via motor único (D1/D3), validando o efeito no saldo do kardex e a trilha.
    /// </summary>
    public class InventarioTests
    {
        private const string TenantId = "tenant-inv";
        private const string UserId = "user-inv";

        // ---------- Domínio ----------

        [Fact(DisplayName = "InventarioItem | Divergência = contado − sistema [D3]")]
        public void InventarioItem_RegistrarContagem_CalculaDivergencia()
        {
            var item = new InventarioItem(Guid.NewGuid(), Guid.NewGuid(), 10m, null, null, TenantId, UserId);
            item.RegistrarContagem(null, null, null, 8m, true, UserId);
            Assert.Equal(8m, item.QuantidadeContada);
            Assert.Equal(-2m, item.Divergencia);
            Assert.False(item.SemDivergencia());
        }

        [Fact(DisplayName = "InventarioItem | Sem divergência quando contado = sistema")]
        public void InventarioItem_SemDivergencia()
        {
            var item = new InventarioItem(Guid.NewGuid(), Guid.NewGuid(), 5m, null, null, TenantId, UserId);
            item.RegistrarContagem(null, null, null, 5m, true, UserId);
            Assert.Equal(0m, item.Divergencia);
            Assert.True(item.SemDivergencia());
        }

        [Fact(DisplayName = "InventarioItem | Produto obrigatório (INV-004)")]
        public void InventarioItem_SemProduto_Invalido()
        {
            var item = new InventarioItem(Guid.NewGuid(), Guid.Empty, 1m, null, null, TenantId, UserId);
            Assert.False(item.IsValid);
        }

        [Fact(DisplayName = "Inventario | Máquina de estados: aprovar exige contagem/conferência")]
        public void Inventario_Aprovar_ForaDeConferencia_Lanca()
        {
            var inv = new Inventario(Guid.NewGuid(), DateTime.UtcNow, ETipoInventario.Geral, null, TenantId, UserId);
            Assert.Throws<InvalidOperationException>(() => inv.Aprovar(1m, UserId)); // ainda em Rascunho
        }

        [Fact(DisplayName = "Inventario | Guid.Empty é o bucket 'empresa não segregada' (EmpresaPadrao) — válido")]
        public void Inventario_EmpresaPadrao_Valido()
        {
            var inv = new Inventario(Guid.Empty, DateTime.UtcNow, ETipoInventario.Geral, null, TenantId, UserId);
            Assert.True(inv.IsValid); // Guid.Empty = MotorMovimentacaoEstoque.EmpresaPadrao (INV-002 "quando disponível")
        }

        // ---------- Fluxo completo (integração via motor único) ----------

        [Fact(DisplayName = "INV | Fluxo criar→contar→aprovar→ajustar aplica diferença no kardex (D1/D3/D14)")]
        public async Task Fluxo_Completo_AjustaSaldoPelaDiferenca()
        {
            var ctx = CreateContext(nameof(Fluxo_Completo_AjustaSaldoPelaDiferenca));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);
            var empresa = MotorMovimentacaoEstoque.EmpresaPadrao;

            var produto = new Produto("SKU-INV-1", "Produto Inventário", 10m, TenantId, UserId);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, TenantId, UserId, produto.Id, 10m, 10m); // saldo 10 @ custo 10

            // Criar (QuantidadeSistema nula → fotografa 10 do kardex)
            var criar = new CriarInventarioCommandHandler(ctx, tp, cu);
            var rCriar = await criar.Handle(new CriarInventarioCommand(empresa, DateTime.UtcNow, ETipoInventario.Geral,
                new List<InventarioItemInput> { new(produto.Id) }), CancellationToken.None);
            Assert.True(rCriar.Sucesso);
            var invId = (Guid)rCriar.Dados!.GetType().GetProperty("Id")!.GetValue(rCriar.Dados)!;

            var item = await ctx.InventarioItens.FirstAsync(it => it.InventarioId == invId);
            Assert.Equal(10m, item.QuantidadeSistema);

            // Iniciar contagem → contar 8 → conferência → aprovar
            await new IniciarContagemInventarioCommandHandler(ctx, cu).Handle(new IniciarContagemInventarioCommand(invId), CancellationToken.None);
            await new RegistrarContagemInventarioItemCommandHandler(ctx, tp, cu).Handle(
                new RegistrarContagemInventarioItemCommand(invId, item.Id, null, null, null, 8m, true), CancellationToken.None);
            await new EnviarConferenciaInventarioCommandHandler(ctx, cu).Handle(new EnviarConferenciaInventarioCommand(invId), CancellationToken.None);

            var rAprovar = await new AprovarInventarioCommandHandler(ctx, tp, cu).Handle(new AprovarInventarioCommand(invId), CancellationToken.None);
            Assert.True(rAprovar.Sucesso);
            var invAprovado = await ctx.Inventarios.AsNoTracking().FirstAsync(i => i.Id == invId);
            Assert.Equal(ESituacaoInventario.Aprovado, invAprovado.Situacao);
            Assert.Equal(0m, invAprovado.Acuracidade); // 0 itens sem divergência / 1 contado

            // Aplicar ajuste → saldo cai de 10 para 8 pelo motor único
            var rAjuste = await new AplicarAjusteInventarioCommandHandler(ctx, tp, cu).Handle(new AplicarAjusteInventarioCommand(invId), CancellationToken.None);
            Assert.True(rAjuste.Sucesso, rAjuste.Mensagem);

            var saldo = await ctx.EstoqueProdutos.AsNoTracking().FirstAsync(e => e.EmpresaId == empresa && e.ProdutoId == produto.Id);
            Assert.Equal(8m, saldo.QuantidadeSaldoEstoque);

            var invFinal = await ctx.Inventarios.AsNoTracking().FirstAsync(i => i.Id == invId);
            Assert.Equal(ESituacaoInventario.Ajustado, invFinal.Situacao);
            Assert.True(invFinal.EstoqueAtualizado);

            // Trilha: vínculo movimento-ajuste + fato gerador + eventos de catálogo
            var vinculo = await ctx.InventarioMovimentoAjustes.AsNoTracking().FirstAsync(m => m.InventarioId == invId);
            Assert.Equal(-2m, vinculo.QuantidadeAplicada);
            Assert.NotNull(vinculo.FatoGeradorEstoqueId);
            Assert.Equal(ETipoAplicacaoAjusteInventario.AjustePorDiferenca, vinculo.TipoAplicacao);

            var eventos = await ctx.OutboxMessages.AsNoTracking().Select(o => o.EventType).ToListAsync();
            Assert.Contains(CatalogoEventosIntegracao.Estoque.InventarioCriado, eventos);
            Assert.Contains(CatalogoEventosIntegracao.Estoque.InventarioAprovado, eventos);
            Assert.Contains(CatalogoEventosIntegracao.Estoque.InventarioAjusteGerado, eventos);
        }

        [Fact(DisplayName = "INV | Acurácia = 1 quando não há divergência (D14)")]
        public async Task Acuracia_100_Quando_SemDivergencia()
        {
            var ctx = CreateContext(nameof(Acuracia_100_Quando_SemDivergencia));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);
            var empresa = MotorMovimentacaoEstoque.EmpresaPadrao;

            var produto = new Produto("SKU-INV-2", "Produto Inventário 2", 10m, TenantId, UserId);
            ctx.Produtos.Add(produto);
            await ctx.SaveChangesAsync();
            await EstoqueTestSeed.SemearSaldoAsync(ctx, TenantId, UserId, produto.Id, 7m, 10m);

            var rCriar = await new CriarInventarioCommandHandler(ctx, tp, cu).Handle(
                new CriarInventarioCommand(empresa, DateTime.UtcNow, ETipoInventario.Ciclico,
                    new List<InventarioItemInput> { new(produto.Id) }), CancellationToken.None);
            var invId = (Guid)rCriar.Dados!.GetType().GetProperty("Id")!.GetValue(rCriar.Dados)!;
            var item = await ctx.InventarioItens.FirstAsync(it => it.InventarioId == invId);

            await new IniciarContagemInventarioCommandHandler(ctx, cu).Handle(new IniciarContagemInventarioCommand(invId), CancellationToken.None);
            await new RegistrarContagemInventarioItemCommandHandler(ctx, tp, cu).Handle(
                new RegistrarContagemInventarioItemCommand(invId, item.Id, null, null, null, 7m, true), CancellationToken.None);
            await new AprovarInventarioCommandHandler(ctx, tp, cu).Handle(new AprovarInventarioCommand(invId), CancellationToken.None);

            var inv = await ctx.Inventarios.AsNoTracking().FirstAsync(i => i.Id == invId);
            Assert.Equal(1m, inv.Acuracidade);

            // Sem divergência → aplicar-ajuste não altera saldo (0 itens divergentes)
            var rAjuste = await new AplicarAjusteInventarioCommandHandler(ctx, tp, cu).Handle(new AplicarAjusteInventarioCommand(invId), CancellationToken.None);
            Assert.True(rAjuste.Sucesso);
            var saldo = await ctx.EstoqueProdutos.AsNoTracking().FirstAsync(e => e.EmpresaId == empresa && e.ProdutoId == produto.Id);
            Assert.Equal(7m, saldo.QuantidadeSaldoEstoque);
        }

        [Fact(DisplayName = "INV | Aplicar ajuste sem aprovação é bloqueado (CA-006)")]
        public async Task AplicarAjuste_SemAprovacao_Bloqueado()
        {
            var ctx = CreateContext(nameof(AplicarAjuste_SemAprovacao_Bloqueado));
            var tp = new TestTenantProvider(TenantId);
            var cu = new TestCurrentUser(UserId);

            var rCriar = await new CriarInventarioCommandHandler(ctx, tp, cu).Handle(
                new CriarInventarioCommand(MotorMovimentacaoEstoque.EmpresaPadrao, DateTime.UtcNow, ETipoInventario.Geral,
                    new List<InventarioItemInput> { new(Guid.NewGuid()) }), CancellationToken.None);
            var invId = (Guid)rCriar.Dados!.GetType().GetProperty("Id")!.GetValue(rCriar.Dados)!;

            var rAjuste = await new AplicarAjusteInventarioCommandHandler(ctx, tp, cu).Handle(new AplicarAjusteInventarioCommand(invId), CancellationToken.None);
            Assert.False(rAjuste.Sucesso);
        }

        private ContextEstoque CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>()
                .UseInMemoryDatabase("db_inv_" + dbName).Options;
            return new ContextEstoque(options, new TestTenantProvider(TenantId), new TestCurrentUser(UserId));
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _t;
            public TestTenantProvider(string t) => _t = t;
            public string GetTenantId() => _t;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _u;
            public TestCurrentUser(string u) => _u = u;
            public string? GetUserId() => _u;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }
    }
}
