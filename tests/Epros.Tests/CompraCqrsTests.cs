using System;
using System.Collections.Generic;
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
    public class CompraCqrsTests
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
        public async Task Deve_Lancar_E_Listar_Compras_Com_Sucesso()
        {
            // Arrange
            var context = CreateInMemoryContext("db_compra_test", "tenant-1", "user-1");
            var tenantProvider = new TestTenantProvider("tenant-1");
            var currentUser = new TestCurrentUser("user-1");

            // Precisa de um produto cadastrado para lançar compra
            var produtoHandler = new CriarProdutoCommandHandler(context, tenantProvider, currentUser);
            var prodCmd = new CriarProdutoCommand(
                CategoriaId: null,
                MarcaProdutoId: null,
                UnidadeMedidaComercialId: null,
                Codigo: "SKU-C1",
                Descricao: "Produto Teste Compra",
                Ean: "7890000000001",
                PesoLiquido: 1m,
                PesoBruto: 1.1m,
                ValorVenda: 10m,
                ValorVendaPrazo: 11m,
                ValorCompra: 5m,
                TipoProduto: Epros.Modules.Estoque.Domain.Enums.ETipoProduto.MateriaPrima,
                Ativo: true,
                Imagem: "",
                CestId: null,
                CodigoAnpId: null,
                BalancaId: null,
                UtilizaBalanca: false,
                CodigoProdutoBalanca: null,
                NcmId: null
            );
            await produtoHandler.Handle(prodCmd, CancellationToken.None);

            var lancarHandler = new LancarCompraCommandHandler(context, tenantProvider, currentUser);
            var listQueryHandler = new ListarComprasQueryHandler(context);
            var getQueryHandler = new ObterCompraPorIdQueryHandler(context);

            var command = new LancarCompraCommand(
                FornecedorCnpj: "12345678000199",
                FornecedorNome: "Fornecedor Teste Ltda",
                NumeroNota: "999",
                ChaveAcesso: "35210712345678000199550010000009991234567890",
                ValorTotal: 50.00m,
                DataEmissao: DateTime.UtcNow,
                Itens: new List<ItemCompraInput>
                {
                    new ItemCompraInput("SKU-C1", "Produto Teste Compra", 10m, 5m, 0.90m, 0.50m)
                }
            );

            // Act - Lancar
            var result = await lancarHandler.Handle(command, CancellationToken.None);
            Assert.True(result.Sucesso);
            var compraId = (Guid)result.Dados.GetType().GetProperty("CompraId")!.GetValue(result.Dados)!;

            // Act - Listar
            var listResult = await listQueryHandler.Handle(new ListarComprasQuery("Fornecedor", 1, 10), CancellationToken.None);
            Assert.True(listResult.Sucesso);
            var listDados = listResult.Dados;
            var total = (int)listDados.GetType().GetProperty("Total")!.GetValue(listDados)!;
            Assert.Equal(1, total);

            // Act - Obter por ID
            var getResult = await getQueryHandler.Handle(new ObterCompraPorIdQuery(compraId), CancellationToken.None);
            Assert.True(getResult.Sucesso);
        }
    }
}
