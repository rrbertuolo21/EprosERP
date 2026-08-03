using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Epros.Modules.Estoque.Application.Commands;
using Epros.Modules.Estoque.Application.Handlers;
using Epros.Modules.Estoque.Application.Queries;
using Epros.Modules.Estoque.Infrastructure.Data;
using Epros.Shared.Application.Contracts;
using Epros.Tests.Integration;
using Xunit;

namespace Epros.Tests
{
    public class ProdutoCqrsTests
    {
        private ContextEstoque CreateInMemoryContext(string databaseName, string tenantId, string userId)
        {
            var options = new DbContextOptionsBuilder<ContextEstoque>()
                .UseInMemoryDatabase(databaseName)
                .Options;

            var tenantProvider = new TestTenantProvider(tenantId);
            var currentUser = new TestCurrentUser(userId);

            return new ContextEstoque(options, tenantProvider, currentUser);
        }

        [Fact]
        public async Task Deve_Criar_Listar_E_Atualizar_Produto_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_produto_test", "tenant-1", "user-1");
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");

            var createHandler = new CriarProdutoCommandHandler(context, tenantProvider, currentUser);
            var updateHandler = new AtualizarProdutoCommandHandler(context, currentUser);
            var deleteHandler = new DeletarProdutoCommandHandler(context, currentUser);

            var listQueryHandler = new ListarProdutosQueryHandler(context);
            var getQueryHandler = new ObterProdutoPorIdQueryHandler(context);

            // Act - Criar
            var createCmd = new CriarProdutoCommand(
                CategoriaId: null,
                MarcaProdutoId: null,
                UnidadeMedidaComercialId: null,
                Codigo: "PROD-TI",
                Descricao: "Produto Teste Integracao",
                Ean: "7891234567890",
                PesoLiquido: 1.2m,
                PesoBruto: 1.5m,
                ValorVenda: 45.90m,
                ValorVendaPrazo: 49.90m,
                ValorCompra: 20.00m,
                TipoProduto: Epros.Modules.Estoque.Domain.Enums.ETipoProduto.MateriaPrima,
                Ativo: true,
                Imagem: "http://image.url",
                CestId: null,
                CodigoAnpId: null,
                BalancaId: null,
                UtilizaBalanca: false,
                CodigoProdutoBalanca: null,
                NcmId: null
            );

            var createResult = await createHandler.Handle(createCmd, CancellationToken.None);

            // Assert - Criar
            Assert.True(createResult.Sucesso);
            var produtoId = (Guid)createResult.Dados!.GetType().GetProperty("ProdutoId")!.GetValue(createResult.Dados)!;
            var produtoSalvo = await context.Produtos.FindAsync(produtoId);
            Assert.NotNull(produtoSalvo);
            Assert.Equal("PROD-TI", produtoSalvo.Codigo);
            Assert.Equal("Produto Teste Integracao", produtoSalvo.Descricao);
            Assert.Equal(45.90m, produtoSalvo.PrecoVenda);

            // Act - Listar
            var listQuery = new ListarProdutosQuery("Produto", true, 1, 10);
            var listResult = await listQueryHandler.Handle(listQuery, CancellationToken.None);

            // Assert - Listar
            Assert.True(listResult.Sucesso);
            var listDados = listResult.Dados!;
            var total = (int)listDados.GetType().GetProperty("Total")!.GetValue(listDados)!;
            Assert.Equal(1, total);

            // Act - Obter Por ID
            var getResult = await getQueryHandler.Handle(new ObterProdutoPorIdQuery(produtoId), CancellationToken.None);
            Assert.True(getResult.Sucesso);

            // Act - Atualizar
            var updateCmd = new AtualizarProdutoCommand(
                Id: produtoId,
                CategoriaId: null,
                MarcaProdutoId: null,
                UnidadeMedidaComercialId: null,
                Codigo: "PROD-TI-EDIT",
                Descricao: "Produto Teste Integracao Editado",
                Ean: "7891234567890",
                PesoLiquido: 1.3m,
                PesoBruto: 1.6m,
                ValorVenda: 55.90m,
                ValorVendaPrazo: 59.90m,
                ValorCompra: 22.00m,
                TipoProduto: Epros.Modules.Estoque.Domain.Enums.ETipoProduto.MateriaPrima,
                Ativo: true,
                CestId: null,
                CodigoAnpId: null,
                BalancaId: null,
                UtilizaBalanca: false,
                CodigoProdutoBalanca: null,
                NcmId: null
            );
            var updateResult = await updateHandler.Handle(updateCmd, CancellationToken.None);
            Assert.True(updateResult.Sucesso);

            // Assert - Atualizar
            context.Entry(produtoSalvo).State = EntityState.Detached;
            var produtoEditado = await context.Produtos.FindAsync(produtoId);
            Assert.Equal("PROD-TI-EDIT", produtoEditado!.Codigo);
            Assert.Equal("Produto Teste Integracao Editado", produtoEditado.Descricao);
            Assert.Equal(55.90m, produtoEditado.PrecoVenda);

            // Act - Deletar
            var deleteResult = await deleteHandler.Handle(new DeletarProdutoCommand(produtoId), CancellationToken.None);
            Assert.True(deleteResult.Sucesso);

            // Assert - Deletar (Soft Delete check)
            context.Entry(produtoEditado).State = EntityState.Detached;
            var produtoDeletado = await context.Produtos.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == produtoId);
            Assert.NotNull(produtoDeletado);
            Assert.NotNull(produtoDeletado.DeletadoEm);
        }
    }
}
