using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Services;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Estoque.Application.Queries;

namespace Epros.Tests
{
    public class EstoqueTests
    {
        #region Testes de Domínio — Motor único (kardex): custo médio móvel (D4/D13) e estoque negativo (D8)

        // Helpers do motor: todo movimento passa pelo MotorMovimentacaoEstoque (D1). O saldo/custo verdadeiro
        // vive no kardex (EstoqueProduto); Produto.SaldoEstoque/CustoMedio são apenas espelho denormalizado.

        private const string TenantDom = "tenant-dom";

        private async Task<(ContextEstoque ctx, Guid produtoId)> NovoProdutoAsync(string db, bool permiteNegativo = false)
        {
            var ctx = CreateInMemoryContext(db, TenantDom, "user-1");
            var p = new Produto("SKU-DOM", "Produto Dominio", 10m, TenantDom, "user-1");
            if (permiteNegativo) p.DefinirPermiteEstoqueNegativo(true, "user-1");
            ctx.Produtos.Add(p);
            await ctx.SaveChangesAsync();
            return (ctx, p.Id);
        }

        private static async Task<ResultadoMovimentacao> EntradaAsync(ContextEstoque ctx, Guid produtoId, decimal quantidade, decimal valorUnitario)
        {
            var motor = new MotorMovimentacaoEstoque(ctx, TenantDom, "user-1");
            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.MovimentoManual, TenantDom, "user-1");
            ctx.FatosGeradoresEstoque.Add(fato);
            var r = await motor.AplicarEntradaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, produtoId, ETipoEstoque.Geral, quantidade, valorUnitario, fato.Id, null, null, null, ETipoCusteioEstoque.CustoMedio, CancellationToken.None);
            await ctx.SaveChangesAsync();
            return r;
        }

        private static async Task<ResultadoMovimentacao> SaidaAsync(ContextEstoque ctx, Guid produtoId, decimal quantidade)
        {
            var motor = new MotorMovimentacaoEstoque(ctx, TenantDom, "user-1");
            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.MovimentoManual, TenantDom, "user-1");
            ctx.FatosGeradoresEstoque.Add(fato);
            var r = await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, produtoId, quantidade, fato.Id, null, CancellationToken.None);
            await ctx.SaveChangesAsync();
            return r;
        }

        private static async Task<EstoqueProduto> SaldoAsync(ContextEstoque ctx, Guid produtoId) =>
            await ctx.EstoqueProdutos.FirstAsync(e => e.EmpresaId == MotorMovimentacaoEstoque.EmpresaPadrao && e.ProdutoId == produtoId);

        [Fact]
        public async Task D4_Custo_Medio_Movel_Recalcula_A_Cada_Entrada()
        {
            var (ctx, produtoId) = await NovoProdutoAsync("dom_media_movel");

            Assert.True((await EntradaAsync(ctx, produtoId, 10m, 100.00m)).Sucesso);
            var s1 = await SaldoAsync(ctx, produtoId);
            Assert.Equal(10m, s1.QuantidadeSaldoEstoque);
            Assert.Equal(100.00m, s1.ValorCustoMedio);

            // ((10*100) + (5*130)) / 15 = 1650 / 15 = 110
            Assert.True((await EntradaAsync(ctx, produtoId, 5m, 130.00m)).Sucesso);
            var s2 = await SaldoAsync(ctx, produtoId);
            Assert.Equal(15m, s2.QuantidadeSaldoEstoque);
            Assert.Equal(110.00m, s2.ValorCustoMedio);

            // Espelho denormalizado do produto acompanha o kardex
            var produto = await ctx.Produtos.FindAsync(produtoId);
            Assert.Equal(15m, produto!.SaldoEstoque);
            Assert.Equal(110.00m, produto.CustoMedio);
        }

        [Fact]
        public async Task D13_Entrada_Sobre_Saldo_Zero_Assume_O_Custo_Da_Nova_Entrada()
        {
            var (ctx, produtoId) = await NovoProdutoAsync("dom_qty_zero");

            Assert.True((await EntradaAsync(ctx, produtoId, 10m, 100.00m)).Sucesso);
            Assert.True((await SaidaAsync(ctx, produtoId, 10m)).Sucesso); // zera o saldo (nunca divide por zero)

            var zerado = await SaldoAsync(ctx, produtoId);
            Assert.Equal(0m, zerado.QuantidadeSaldoEstoque);

            // Nova entrada sobre saldo zero assume o custo da entrada (D13)
            Assert.True((await EntradaAsync(ctx, produtoId, 5m, 200.00m)).Sucesso);
            var s = await SaldoAsync(ctx, produtoId);
            Assert.Equal(5m, s.QuantidadeSaldoEstoque);
            Assert.Equal(200.00m, s.ValorCustoMedio);
        }

        [Fact]
        public async Task D4_Saida_Sai_Pela_Media_Vigente_E_Nao_Altera_A_Media()
        {
            var (ctx, produtoId) = await NovoProdutoAsync("dom_saida_media");
            await EntradaAsync(ctx, produtoId, 10m, 100.00m);
            await EntradaAsync(ctx, produtoId, 5m, 130.00m); // média = 110

            Assert.True((await SaidaAsync(ctx, produtoId, 8m)).Sucesso);

            var s = await SaldoAsync(ctx, produtoId);
            Assert.Equal(7m, s.QuantidadeSaldoEstoque);
            Assert.Equal(110.00m, s.ValorCustoMedio); // custo médio não muda na saída
        }

        [Fact]
        public async Task D8_Saida_Acima_Do_Saldo_Bloqueada_Por_Padrao()
        {
            var (ctx, produtoId) = await NovoProdutoAsync("dom_neg_bloqueia");
            await EntradaAsync(ctx, produtoId, 5m, 100.00m);

            var r = await SaidaAsync(ctx, produtoId, 8m);

            Assert.False(r.Sucesso);
            Assert.Contains("insuficiente", r.Erro ?? string.Empty);
            var s = await SaldoAsync(ctx, produtoId);
            Assert.Equal(5m, s.QuantidadeSaldoEstoque); // saldo intacto
        }

        [Fact]
        public async Task D8_Saida_Acima_Do_Saldo_Permitida_Quando_Produto_Permite_Negativo()
        {
            var (ctx, produtoId) = await NovoProdutoAsync("dom_neg_permite", permiteNegativo: true);
            await EntradaAsync(ctx, produtoId, 5m, 100.00m);

            var r = await SaidaAsync(ctx, produtoId, 8m);

            Assert.True(r.Sucesso);
            var s = await SaldoAsync(ctx, produtoId);
            Assert.Equal(-3m, s.QuantidadeSaldoEstoque); // saldo negativo autorizado
        }

        #endregion

        #region Testes de Handlers (CQRS)

        [Fact]
        public async Task Deve_Lancar_Compra_E_Cadastrar_Produtos_Inexistentes_Automaticamente()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_compra_success", "tenant-1", "user-1");

            var handler = new LancarCompraCommandHandler(context, tenantProvider, currentUser);

            var itens = new List<ItemCompraInput>
            {
                new ItemCompraInput("SKU-PROD-A", "Produto Novo A", 10, 50.00m, 9.00m, 5.00m),
                new ItemCompraInput("SKU-PROD-B", "Produto Novo B", 5, 120.00m, 21.60m, 12.00m)
            };

            var accessKey44D = new string('9', 44);
            var command = new LancarCompraCommand("12.345.678/0001-99", "Fornecedor Teste S/A", "001234", accessKey44D, 1100.00m, DateTime.UtcNow, itens);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result.Sucesso);

            // Verificar se os produtos foram inseridos no banco e o estoque foi computado
            var prodA = await context.Produtos.FirstOrDefaultAsync(p => p.Sku == "SKU-PROD-A");
            Assert.NotNull(prodA);
            Assert.Equal("Produto Novo A", prodA.Nome);
            Assert.Equal(10, prodA.SaldoEstoque);
            Assert.Equal(50.00m, prodA.CustoMedio);

            var prodB = await context.Produtos.FirstOrDefaultAsync(p => p.Sku == "SKU-PROD-B");
            Assert.NotNull(prodB);
            Assert.Equal("Produto Novo B", prodB.Nome);
            Assert.Equal(5, prodB.SaldoEstoque);
            Assert.Equal(120.00m, prodB.CustoMedio);

            // Verificar se a compra e itens foram persistidos
            var compraSalva = await context.Compras.Include(c => c.Itens).FirstOrDefaultAsync();
            Assert.NotNull(compraSalva);
            Assert.Equal("Fornecedor Teste S/A", compraSalva.FornecedorNome);
            Assert.Equal(2, compraSalva.Itens.Count);
            Assert.Equal(1100.00m, compraSalva.ValorTotal);
            Assert.Equal(Epros.Modules.Estoque.Domain.Enums.EVendaStatus.Transmitido, compraSalva.Status);

            // Verificar as movimentações de estoque
            var movimentos = await context.MovimentosEstoque.ToListAsync();
            Assert.Equal(2, movimentos.Count);
            Assert.Contains(movimentos, m => m.ProdutoId == prodA.Id && m.Tipo == "Entrada" && m.Quantidade == 10);
            Assert.Contains(movimentos, m => m.ProdutoId == prodB.Id && m.Tipo == "Entrada" && m.Quantidade == 5);

            // Verificar se a mensagem de Outbox foi gerada transacionalmente
            var outboxMsg = await context.OutboxMessages.FirstOrDefaultAsync();
            Assert.NotNull(outboxMsg);
            Assert.Equal("CompraLancada", outboxMsg.EventType);
            Assert.Contains("SKU-PROD-A", outboxMsg.Payload);
            Assert.Equal("tenant-1", outboxMsg.TenantId);
        }

        [Fact]
        public async Task Nao_Deve_Permitir_Duplicidade_De_Nota_Fiscal_Com_Mesma_Chave_De_Acesso()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");
            var context = CreateInMemoryContext("db_compra_duplicate", "tenant-1", "user-1");

            var accessKey44D = new string('7', 44);
            var itens = new List<ItemCompraInput>
            {
                new ItemCompraInput("SKU-1", "Prod 1", 10, 50.00m, 9.00m, 5.00m)
            };

            // Salva compra existente no banco
            var compraExistente = new Compra("12.345.678/0001-99", "Forn", "123", accessKey44D, 500m, DateTime.UtcNow, "tenant-1", "user-1");
            context.Compras.Add(compraExistente);
            await context.SaveChangesAsync();

            var handler = new LancarCompraCommandHandler(context, tenantProvider, currentUser);
            var command = new LancarCompraCommand("12.345.678/0001-99", "Fornecedor Teste S/A", "124", accessKey44D, 500m, DateTime.UtcNow, itens);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result.Sucesso);
            Assert.Contains(result.Erros, e => e.Contains("já foi lançada"));
        }

        [Fact]
        public async Task Deve_Obter_Produtos_Sync_Delta_Filtrando_Por_Tenant_E_Data()
        {
            // Arrange
            var tenantProvider = new TestTenantProvider("tenant-1");
            var context = CreateInMemoryContext("db_produtos_sync", "tenant-1", "user-1");
            var contextTenant2 = CreateInMemoryContext("db_produtos_sync", "tenant-2", "user-1");

            var prod1 = new Produto("SKU-SYNC-1", "Prod Sync 1", 100m, "tenant-1", "user-1");
            var prod2 = new Produto("SKU-SYNC-2", "Prod Sync 2", 150m, "tenant-1", "user-1");
            var prodTenant2 = new Produto("SKU-SYNC-3", "Prod Outro Tenant", 200m, "tenant-2", "user-1");

            context.Produtos.AddRange(prod1, prod2);
            await context.SaveChangesAsync();

            contextTenant2.Produtos.Add(prodTenant2);
            await contextTenant2.SaveChangesAsync();

            // Pequeno delay para garantir avanço do relógio
            await Task.Delay(20);
            var since = DateTime.UtcNow;
            await Task.Delay(20);

            // Simula alteração do produto 2
            prod2.MarcarAlterado("user-1");
            await context.SaveChangesAsync();

            var queryHandler = new ObterProdutosSyncQueryHandler(context, tenantProvider);

            // Act - Sincronização inicial (since = MinValue)
            var queryCompleta = new ObterProdutosSyncQuery(DateTime.MinValue);
            var resultadoCompleto = await queryHandler.Handle(queryCompleta, CancellationToken.None);

            // Act - Sincronização incremental
            var queryIncremental = new ObterProdutosSyncQuery(since);
            var resultadoIncremental = await queryHandler.Handle(queryIncremental, CancellationToken.None);

            // Assert
            Assert.Equal(2, resultadoCompleto.Count());
            Assert.Contains(resultadoCompleto, p => p.Sku == "SKU-SYNC-1");
            Assert.Contains(resultadoCompleto, p => p.Sku == "SKU-SYNC-2");

            Assert.Single(resultadoIncremental);
            Assert.Equal("SKU-SYNC-2", resultadoIncremental.First().Sku);
        }

        #endregion

        #region Testes Movimentação Manual e Ajustes (EST-MVM-001)

        private static readonly Guid EmpresaTeste = Guid.Parse("11111111-1111-1111-1111-111111111111");

        private async Task<Guid> CriarProdutoAsync(ContextEstoque context, string sku)
        {
            var produto = new Produto(sku, "Produto MVM", 100m, "tenant-mvm", "user-1");
            context.Produtos.Add(produto);
            await context.SaveChangesAsync();
            return produto.Id;
        }

        [Fact]
        public async Task Movimento_Entrada_Deve_Criar_Ficha_Atualizar_Saldo_E_Custo()
        {
            var context = CreateInMemoryContext("mvm_entrada", "tenant-mvm", "user-1");
            var tenant = new TestTenantProvider("tenant-mvm");
            var user = new TestCurrentUser("user-1");
            var produtoId = await CriarProdutoAsync(context, "SKU-ENT");

            var handler = new CriarEstoqueMovimentoManualCommandHandler(context, tenant, user);
            var cmd = new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Entrada, 10m, 100m);

            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.True(result.Sucesso);
            var saldo = await context.EstoqueProdutos.FirstOrDefaultAsync(e => e.EmpresaId == EmpresaTeste && e.ProdutoId == produtoId);
            Assert.NotNull(saldo);
            Assert.Equal(10m, saldo.QuantidadeSaldoEstoque);
            Assert.Equal(1000m, saldo.ValorSaldo);
            Assert.Equal(100m, saldo.ValorCustoMedio);

            var ficha = await context.ProdutoFichaEstoqueEntradas.FirstOrDefaultAsync(f => f.ProdutoId == produtoId);
            Assert.NotNull(ficha);
            Assert.Equal(10m, ficha.QuantidadeSaldo);

            var movimento = await context.EstoqueMovimentosManuais.FirstOrDefaultAsync(m => m.ProdutoId == produtoId);
            Assert.NotNull(movimento);
            Assert.Equal(Epros.Modules.Estoque.Domain.Enums.EStatusMovimentoEstoque.Aplicado, movimento.Situacao);
        }

        [Fact]
        public async Task Movimento_Sem_Quantidade_Deve_Ser_Bloqueado()
        {
            var context = CreateInMemoryContext("mvm_invalido", "tenant-mvm", "user-1");
            var handler = new CriarEstoqueMovimentoManualCommandHandler(context, new TestTenantProvider("tenant-mvm"), new TestCurrentUser("user-1"));
            var produtoId = await CriarProdutoAsync(context, "SKU-INV");

            var cmd = new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Entrada, 0m, 100m);
            var result = await handler.Handle(cmd, CancellationToken.None);

            Assert.False(result.Sucesso); // MVM-004
        }

        [Fact]
        public async Task Saida_Maior_Que_Saldo_Deve_Ser_Bloqueada()
        {
            var context = CreateInMemoryContext("mvm_saida_insuf", "tenant-mvm", "user-1");
            var tenant = new TestTenantProvider("tenant-mvm");
            var user = new TestCurrentUser("user-1");
            var produtoId = await CriarProdutoAsync(context, "SKU-SAI");
            var handler = new CriarEstoqueMovimentoManualCommandHandler(context, tenant, user);

            await handler.Handle(new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Entrada, 5m, 100m), CancellationToken.None);

            var result = await handler.Handle(new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Saida, 8m, 100m), CancellationToken.None);

            Assert.False(result.Sucesso); // MVM-009
            Assert.Contains(result.Erros, e => e.Contains("insuficiente"));

            var saldo = await context.EstoqueProdutos.FirstAsync(e => e.ProdutoId == produtoId);
            Assert.Equal(5m, saldo.QuantidadeSaldoEstoque);
        }

        [Fact]
        public async Task Saida_PEPS_Deve_Consumir_Ficha_Mais_Antiga_Primeiro()
        {
            var context = CreateInMemoryContext("mvm_peps", "tenant-mvm", "user-1");
            var tenant = new TestTenantProvider("tenant-mvm");
            var user = new TestCurrentUser("user-1");
            var produtoId = await CriarProdutoAsync(context, "SKU-PEPS");

            // Saldo com custeio PEPS
            context.EstoqueProdutos.Add(new EstoqueProduto(EmpresaTeste, produtoId, 0m, 0m, 0m, 0m, 0m, Epros.Modules.Estoque.Domain.Enums.ETipoCusteioEstoque.PEPS, "tenant-mvm", "user-1"));
            await context.SaveChangesAsync();

            var handler = new CriarEstoqueMovimentoManualCommandHandler(context, tenant, user);
            await handler.Handle(new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Entrada, 10m, 100m), CancellationToken.None);
            await Task.Delay(15);
            await handler.Handle(new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Entrada, 10m, 130m), CancellationToken.None);

            var result = await handler.Handle(new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Saida, 5m, 0m), CancellationToken.None);
            Assert.True(result.Sucesso);

            var fichas = await context.ProdutoFichaEstoqueEntradas.Where(f => f.ProdutoId == produtoId).OrderBy(f => f.CriadoEm).ToListAsync();
            Assert.Equal(5m, fichas[0].QuantidadeSaldo);  // ficha mais antiga consumida (10 - 5)
            Assert.Equal(10m, fichas[1].QuantidadeSaldo); // ficha mais recente intacta

            var saldo = await context.EstoqueProdutos.FirstAsync(e => e.ProdutoId == produtoId);
            Assert.Equal(15m, saldo.QuantidadeSaldoEstoque);
        }

        [Fact]
        public async Task Estorno_Deve_Reverter_Saldo_Do_Movimento_Aplicado()
        {
            var context = CreateInMemoryContext("mvm_estorno", "tenant-mvm", "user-1");
            var tenant = new TestTenantProvider("tenant-mvm");
            var user = new TestCurrentUser("user-1");
            var produtoId = await CriarProdutoAsync(context, "SKU-EST");

            var criar = new CriarEstoqueMovimentoManualCommandHandler(context, tenant, user);
            var criarResult = await criar.Handle(new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Entrada, 10m, 100m), CancellationToken.None);
            Assert.True(criarResult.Sucesso);

            var movimento = await context.EstoqueMovimentosManuais.FirstAsync(m => m.ProdutoId == produtoId);
            var estornar = new EstornarEstoqueMovimentoManualCommandHandler(context, tenant, user);
            var result = await estornar.Handle(new EstornarEstoqueMovimentoManualCommand(movimento.Id, EmpresaTeste, "Erro de lançamento"), CancellationToken.None);

            Assert.True(result.Sucesso);
            var saldo = await context.EstoqueProdutos.FirstAsync(e => e.ProdutoId == produtoId);
            Assert.Equal(0m, saldo.QuantidadeSaldoEstoque);
            var movEstornado = await context.EstoqueMovimentosManuais.FirstAsync(m => m.Id == movimento.Id);
            Assert.Equal(Epros.Modules.Estoque.Domain.Enums.EStatusMovimentoEstoque.Estornado, movEstornado.Situacao);
        }

        [Fact]
        public async Task Ajuste_Sem_Motivo_Deve_Ser_Bloqueado_E_Negativo_Reduz_Saldo()
        {
            var context = CreateInMemoryContext("mvm_ajuste", "tenant-mvm", "user-1");
            var tenant = new TestTenantProvider("tenant-mvm");
            var user = new TestCurrentUser("user-1");
            var produtoId = await CriarProdutoAsync(context, "SKU-AJU");

            var criar = new CriarEstoqueMovimentoManualCommandHandler(context, tenant, user);
            await criar.Handle(new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Entrada, 10m, 100m), CancellationToken.None);

            var ajusteHandler = new CriarAjusteEstoqueCommandHandler(context, tenant, user);
            var itens = new System.Collections.Generic.List<AjusteEstoqueItemInput> { new AjusteEstoqueItemInput(produtoId, -3m, 100m, null) };

            // Sem motivo -> bloqueado (MVM-025)
            var semMotivo = await ajusteHandler.Handle(new CriarAjusteEstoqueCommand(EmpresaTeste, null, DateTime.UtcNow, Epros.Modules.Estoque.Domain.Enums.ETipoAjusteEstoque.Normal, null, "", itens), CancellationToken.None);
            Assert.False(semMotivo.Sucesso);

            // Com motivo e quantidade negativa -> saída controlada
            var comMotivo = await ajusteHandler.Handle(new CriarAjusteEstoqueCommand(EmpresaTeste, null, DateTime.UtcNow, Epros.Modules.Estoque.Domain.Enums.ETipoAjusteEstoque.Normal, null, "Perda de inventário", itens), CancellationToken.None);
            Assert.True(comMotivo.Sucesso);

            var saldo = await context.EstoqueProdutos.FirstAsync(e => e.ProdutoId == produtoId);
            Assert.Equal(7m, saldo.QuantidadeSaldoEstoque);
        }

        [Fact]
        public async Task Movimento_Deve_Respeitar_Isolamento_Por_Tenant()
        {
            var contextT1 = CreateInMemoryContext("mvm_tenant_iso", "tenant-1", "user-1");
            var contextT2 = CreateInMemoryContext("mvm_tenant_iso", "tenant-2", "user-1");
            var produtoId = await CriarProdutoAsync(contextT1, "SKU-ISO");

            var handler = new CriarEstoqueMovimentoManualCommandHandler(contextT1, new TestTenantProvider("tenant-1"), new TestCurrentUser("user-1"));
            await handler.Handle(new CriarEstoqueMovimentoManualCommand(EmpresaTeste, produtoId, Epros.Modules.Estoque.Domain.Enums.ETipoEstoque.ProdutoAcabado, Epros.Shared.Domain.Enums.ETipoMovimento.Entrada, 10m, 100m), CancellationToken.None);

            var movimentosT1 = await contextT1.EstoqueMovimentosManuais.ToListAsync();
            var movimentosT2 = await contextT2.EstoqueMovimentosManuais.ToListAsync();

            Assert.Single(movimentosT1);
            Assert.Empty(movimentosT2); // VAL-MVM-016: filtro global de tenant
        }

        #endregion

        #region D2 — Saldo por Local + Lote/Série (grão fino) e FEFO (D10)

        private const string TenantD2 = "tenant-d2";

        private static async Task EntradaGrainAsync(ContextEstoque ctx, Guid produtoId, decimal qtd, decimal valorUnit, Guid? localId, string? lote, DateTime? validade)
        {
            var motor = new MotorMovimentacaoEstoque(ctx, TenantD2, "user-1");
            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.MovimentoManual, TenantD2, "user-1");
            ctx.FatosGeradoresEstoque.Add(fato);
            var r = await motor.AplicarEntradaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, produtoId, ETipoEstoque.Geral, qtd, valorUnit, fato.Id, localId, lote, validade, ETipoCusteioEstoque.CustoMedio, CancellationToken.None);
            Assert.True(r.Sucesso, r.Erro);
            await ctx.SaveChangesAsync();
        }

        private static async Task SaidaGrainAsync(ContextEstoque ctx, Guid produtoId, decimal qtd, Guid? localId)
        {
            var motor = new MotorMovimentacaoEstoque(ctx, TenantD2, "user-1");
            var fato = new FatoGeradorEstoque(null, null, null, EOrigemFatoGeradorEstoque.MovimentoManual, TenantD2, "user-1");
            ctx.FatosGeradoresEstoque.Add(fato);
            var r = await motor.AplicarSaidaAsync(MotorMovimentacaoEstoque.EmpresaPadrao, produtoId, qtd, fato.Id, localId, CancellationToken.None);
            Assert.True(r.Sucesso, r.Erro);
            await ctx.SaveChangesAsync();
        }

        [Fact]
        public async Task D2_Grao_Fino_Reconcilia_Com_Agregado_Na_Entrada_E_Saida()
        {
            var ctx = CreateInMemoryContext("d2_reconcilia", TenantD2, "user-1");
            var p = new Produto("SKU-D2", "Produto D2", 10m, TenantD2, "user-1");
            ctx.Produtos.Add(p);
            await ctx.SaveChangesAsync();

            var localA = Guid.NewGuid();
            var localB = Guid.NewGuid();
            await EntradaGrainAsync(ctx, p.Id, 10m, 100m, localA, "L1", null);
            await EntradaGrainAsync(ctx, p.Id, 6m, 100m, localB, "L2", null);
            await SaidaGrainAsync(ctx, p.Id, 4m, localA); // baixa 4 do local A

            var agregado = await ctx.EstoqueProdutos.FirstAsync(e => e.ProdutoId == p.Id);
            var somaGrao = await ctx.EstoqueSaldosLocais.Where(s => s.ProdutoId == p.Id).SumAsync(s => s.QuantidadeSaldo);

            // Invariante D2: soma do grão fino == saldo do agregado (a verdade da suíte).
            Assert.Equal(12m, agregado.QuantidadeSaldoEstoque);
            Assert.Equal(agregado.QuantidadeSaldoEstoque, somaGrao);

            var posA = await ctx.EstoqueSaldosLocais.FirstAsync(s => s.ProdutoId == p.Id && s.LocalId == localA);
            var posB = await ctx.EstoqueSaldosLocais.FirstAsync(s => s.ProdutoId == p.Id && s.LocalId == localB);
            Assert.Equal(6m, posA.QuantidadeSaldo); // 10 - 4
            Assert.Equal(6m, posB.QuantidadeSaldo); // intacto
        }

        [Fact]
        public async Task D2_Entrada_Cria_Linha_Distinta_Por_Local_E_Lote()
        {
            var ctx = CreateInMemoryContext("d2_grao_linhas", TenantD2, "user-1");
            var p = new Produto("SKU-D2B", "Produto D2B", 10m, TenantD2, "user-1");
            ctx.Produtos.Add(p);
            await ctx.SaveChangesAsync();

            var local = Guid.NewGuid();
            await EntradaGrainAsync(ctx, p.Id, 5m, 100m, local, "LOTE-A", null);
            await EntradaGrainAsync(ctx, p.Id, 3m, 100m, local, "LOTE-B", null);
            await EntradaGrainAsync(ctx, p.Id, 2m, 100m, local, "LOTE-A", null); // mesma chave → acumula na linha existente

            var linhas = await ctx.EstoqueSaldosLocais.Where(s => s.ProdutoId == p.Id).ToListAsync();
            Assert.Equal(2, linhas.Count); // LOTE-A e LOTE-B
            Assert.Equal(7m, linhas.First(l => l.CodigoLote == "LOTE-A").QuantidadeSaldo); // 5 + 2
            Assert.Equal(3m, linhas.First(l => l.CodigoLote == "LOTE-B").QuantidadeSaldo);
        }

        [Fact]
        public async Task D10_Saida_FEFO_Consome_Menor_Validade_Primeiro()
        {
            var ctx = CreateInMemoryContext("d2_fefo", TenantD2, "user-1");
            var p = new Produto("SKU-FEFO", "Produto FEFO", 10m, TenantD2, "user-1");
            p.DefinirControleRastreabilidade(controlaLote: true, exigeSerializacao: false, "user-1");
            ctx.Produtos.Add(p);
            await ctx.SaveChangesAsync();

            var local = Guid.NewGuid();
            // LOTE-VELHO entra PRIMEIRO mas vence DEPOIS; LOTE-NOVO entra depois mas vence ANTES.
            await EntradaGrainAsync(ctx, p.Id, 5m, 100m, local, "LOTE-VELHO", new DateTime(2027, 12, 1, 0, 0, 0, DateTimeKind.Utc));
            await EntradaGrainAsync(ctx, p.Id, 5m, 100m, local, "LOTE-NOVO", new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            await SaidaGrainAsync(ctx, p.Id, 4m, local);

            var loteNovo = await ctx.EstoqueSaldosLocais.FirstAsync(s => s.ProdutoId == p.Id && s.CodigoLote == "LOTE-NOVO");
            var loteVelho = await ctx.EstoqueSaldosLocais.FirstAsync(s => s.ProdutoId == p.Id && s.CodigoLote == "LOTE-VELHO");

            // FEFO: consome o de MENOR validade (LOTE-NOVO) primeiro, não o mais antigo (PEPS baixaria LOTE-VELHO).
            Assert.Equal(1m, loteNovo.QuantidadeSaldo);  // 5 - 4
            Assert.Equal(5m, loteVelho.QuantidadeSaldo);  // intacto
        }

        #endregion

        #region D5 — WMS operacional: tarefa de separação/conferência move o grão fino

        [Fact]
        public async Task D5_Separacao_Reserva_Origem_E_Conferencia_Transfere_Para_Destino()
        {
            var ctx = CreateInMemoryContext("d5_separacao", TenantD2, "user-1");
            var p = new Produto("SKU-WMS", "Produto WMS", 10m, TenantD2, "user-1");
            ctx.Produtos.Add(p);
            await ctx.SaveChangesAsync();

            var origem = Guid.NewGuid();
            var destino = Guid.NewGuid();
            await EntradaGrainAsync(ctx, p.Id, 10m, 100m, origem, "L1", null); // saldo na posição de origem

            var svc = new WmsSeparacaoService(ctx, TenantD2, "user-1");

            // Criar tarefa → reserva 4 na origem (disponível cai para 6, saldo total continua 10).
            var criar = await svc.CriarTarefaAsync(Guid.NewGuid(), MotorMovimentacaoEstoque.EmpresaPadrao, p.Id, origem, destino, "L1", null, 4m, null, CancellationToken.None);
            Assert.True(criar.Sucesso, criar.Erro);
            await ctx.SaveChangesAsync();

            var posOrigem = await ctx.EstoqueSaldosLocais.FirstAsync(s => s.ProdutoId == p.Id && s.LocalId == origem);
            Assert.Equal(10m, posOrigem.QuantidadeSaldo);
            Assert.Equal(4m, posOrigem.QuantidadeReservada);
            Assert.Equal(6m, posOrigem.QuantidadeDisponivel());

            // Conferir → baixa 4 da origem, credita 4 no destino. Soma do grão do produto permanece 10.
            var conf = await svc.ConferirTarefaAsync(criar.Tarefa!.Id, 4m, CancellationToken.None);
            Assert.True(conf.Sucesso, conf.Erro);
            await ctx.SaveChangesAsync();

            posOrigem = await ctx.EstoqueSaldosLocais.FirstAsync(s => s.ProdutoId == p.Id && s.LocalId == origem);
            var posDestino = await ctx.EstoqueSaldosLocais.FirstAsync(s => s.ProdutoId == p.Id && s.LocalId == destino);
            Assert.Equal(6m, posOrigem.QuantidadeSaldo);
            Assert.Equal(0m, posOrigem.QuantidadeReservada);
            Assert.Equal(4m, posDestino.QuantidadeSaldo);

            var somaGrao = await ctx.EstoqueSaldosLocais.Where(s => s.ProdutoId == p.Id).SumAsync(s => s.QuantidadeSaldo);
            var agregado = await ctx.EstoqueProdutos.FirstAsync(e => e.ProdutoId == p.Id);
            Assert.Equal(10m, somaGrao);                                   // transferência interna não altera total
            Assert.Equal(10m, agregado.QuantidadeSaldoEstoque);           // agregado intocado por movimento de armazém
            Assert.Equal(EStatusTarefaSeparacao.Conferida, conf.Tarefa!.Status);
        }

        [Fact]
        public async Task D5_Separacao_Sem_Saldo_Disponivel_Falha()
        {
            var ctx = CreateInMemoryContext("d5_sem_saldo", TenantD2, "user-1");
            var p = new Produto("SKU-WMS2", "Produto WMS2", 10m, TenantD2, "user-1");
            ctx.Produtos.Add(p);
            await ctx.SaveChangesAsync();

            var origem = Guid.NewGuid();
            await EntradaGrainAsync(ctx, p.Id, 3m, 100m, origem, "L1", null);

            var svc = new WmsSeparacaoService(ctx, TenantD2, "user-1");
            var criar = await svc.CriarTarefaAsync(Guid.NewGuid(), MotorMovimentacaoEstoque.EmpresaPadrao, p.Id, origem, null, "L1", null, 5m, null, CancellationToken.None);
            Assert.False(criar.Sucesso); // 5 > 3 disponível
        }

        [Fact]
        public async Task D5_Cancelar_Libera_Reserva_Da_Origem()
        {
            var ctx = CreateInMemoryContext("d5_cancelar", TenantD2, "user-1");
            var p = new Produto("SKU-WMS3", "Produto WMS3", 10m, TenantD2, "user-1");
            ctx.Produtos.Add(p);
            await ctx.SaveChangesAsync();

            var origem = Guid.NewGuid();
            await EntradaGrainAsync(ctx, p.Id, 10m, 100m, origem, "L1", null);

            var svc = new WmsSeparacaoService(ctx, TenantD2, "user-1");
            var criar = await svc.CriarTarefaAsync(Guid.NewGuid(), MotorMovimentacaoEstoque.EmpresaPadrao, p.Id, origem, null, "L1", null, 4m, null, CancellationToken.None);
            await ctx.SaveChangesAsync();

            var cancelar = await svc.CancelarTarefaAsync(criar.Tarefa!.Id, CancellationToken.None);
            Assert.True(cancelar.Sucesso, cancelar.Erro);
            await ctx.SaveChangesAsync();

            var pos = await ctx.EstoqueSaldosLocais.FirstAsync(s => s.ProdutoId == p.Id && s.LocalId == origem);
            Assert.Equal(0m, pos.QuantidadeReservada);         // reserva devolvida
            Assert.Equal(10m, pos.QuantidadeDisponivel());
        }

        #endregion

        #region Helpers e Doubles de Teste

        private ContextEstoque CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextEstoque(options, tenantProvider, currentUser);
        }

        private class TestTenantProvider : ITenantProvider
        {
            private readonly string _tenantId;
            public TestTenantProvider(string tenantId) => _tenantId = tenantId;
            public string GetTenantId() => _tenantId;
        }

        private class TestCurrentUser : ICurrentUser
        {
            private readonly string _userId;
            public TestCurrentUser(string userId) => _userId = userId;
            public string? GetUserId() => _userId;
            public string? GetUserName() => "Test User";
            public string? GetUserEmail() => "test@epros.com";
        }

        #endregion
    }
}
