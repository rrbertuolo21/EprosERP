using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Epros.Shared.Application.Contracts;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Modules.Estoque.Application.Queries;

namespace Epros.Tests
{
    public class EstoqueTests
    {
        #region Testes de Domínio (Produto e Custo Médio Ponderado)

        [Fact]
        public void Deve_Calcular_Custo_Medio_Ponderado_Corretamente_Em_Multiplas_Entradas()
        {
            // Arrange
            var produto = new Produto("SKU-1", "Produto Teste", 200m, "tenant-a", "user-1");

            // Act - Primeira Entrada (Quantidade: 10, Preço: 100.00)
            produto.LancarEntradaEstoque(10, 100.00m, "user-1");

            // Assert
            Assert.Equal(10, produto.SaldoEstoque);
            Assert.Equal(100.00m, produto.CustoMedio);

            // Act - Segunda Entrada (Quantidade: 5, Preço: 130.00)
            // Fórmula: ((10 * 100.00) + (5 * 130.00)) / 15 = (1000 + 650) / 15 = 1650 / 15 = 110.00
            produto.LancarEntradaEstoque(5, 130.00m, "user-1");

            // Assert
            Assert.Equal(15, produto.SaldoEstoque);
            Assert.Equal(110.00m, produto.CustoMedio);
        }

        [Fact]
        public void Nao_Deve_Permitir_Entrada_De_Estoque_Com_Valores_Negativos_Ou_Zerados()
        {
            // Arrange
            var produto = new Produto("SKU-1", "Produto Teste", 200m, "tenant-a", "user-1");

            // Act
            produto.LancarEntradaEstoque(0, 100.00m, "user-1");
            produto.LancarEntradaEstoque(10, 0m, "user-1");
            produto.LancarEntradaEstoque(-5, 100.00m, "user-1");

            // Assert
            Assert.False(produto.IsValid);
            Assert.Equal(0, produto.SaldoEstoque);
            Assert.Equal(0, produto.CustoMedio);
        }

        [Fact]
        public void Deve_Debitar_Estoque_Corretamente()
        {
            // Arrange
            var produto = new Produto("SKU-1", "Produto Teste", 200m, "tenant-a", "user-1");
            produto.LancarEntradaEstoque(20, 100.00m, "user-1");

            // Act
            produto.LancarSaidaEstoque(8, "user-2");

            // Assert
            Assert.Equal(12, produto.SaldoEstoque);
            Assert.Equal(100.00m, produto.CustoMedio); // Custo médio não se altera em saídas
        }

        [Fact]
        public void Nao_Deve_Permitir_Saida_Se_Saldo_For_Insuficiente()
        {
            // Arrange
            var produto = new Produto("SKU-1", "Produto Teste", 200m, "tenant-a", "user-1");
            produto.LancarEntradaEstoque(5, 100.00m, "user-1");

            // Act
            produto.LancarSaidaEstoque(8, "user-2");

            // Assert
            Assert.False(produto.IsValid);
            Assert.Equal(5, produto.SaldoEstoque);
            Assert.Contains(produto.Notifications, n => n.Message.Contains("insuficiente"));
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
