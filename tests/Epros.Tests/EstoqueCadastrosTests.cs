using System;
using System.Linq;
using Xunit;
using Epros.Modules.Estoque.Domain.Entities;
using Epros.Modules.Estoque.Domain.Enums;

namespace Epros.Tests
{
    /// <summary>
    /// Testes de domínio (puros, sem banco) das entidades de cadastro do módulo Estoque:
    /// CategoriaProduto, MarcaProduto, UnidadeMedidaComercial, Balanca, Adicionais, ProdutoGrupo.
    /// Cobrem caminho feliz, falhas de validação e regras portadas do legado.
    /// </summary>
    public class EstoqueCadastrosTests
    {
        private const string TenantId = "tenant-test";
        private const string Usuario = "user-test";

        // ============================ CategoriaProduto ============================

        [Fact]
        public void CategoriaProduto_Criar_ComDescricaoValida_DeveSerValida()
        {
            var categoria = new CategoriaProduto("Bebidas", TenantId, Usuario);

            Assert.True(categoria.IsValid);
            Assert.Equal("Bebidas", categoria.Descricao);
            Assert.Null(categoria.ProdutoGrupoId);
        }

        [Fact]
        public void CategoriaProduto_Criar_ComDescricaoVazia_DeveSerInvalida()
        {
            var categoria = new CategoriaProduto(string.Empty, TenantId, Usuario);

            Assert.False(categoria.IsValid);
            Assert.Contains(categoria.Notifications, n => n.Key == nameof(CategoriaProduto.Descricao));
        }

        [Fact]
        public void CategoriaProduto_Criar_ComDescricaoAcimaDoLimite_DeveSerInvalida()
        {
            var categoria = new CategoriaProduto(new string('x', 151), TenantId, Usuario);

            Assert.False(categoria.IsValid);
            Assert.Contains(categoria.Notifications, n => n.Key == nameof(CategoriaProduto.Descricao));
        }

        [Fact]
        public void CategoriaProduto_Criar_ComProdutoGrupo_DevePreservarFk()
        {
            var grupoId = Guid.NewGuid();
            var categoria = new CategoriaProduto("Bebidas", grupoId, TenantId, Usuario);

            Assert.True(categoria.IsValid);
            Assert.Equal(grupoId, categoria.ProdutoGrupoId);
        }

        [Fact]
        public void CategoriaProduto_Alterar_ComDadosValidos_DeveAtualizar()
        {
            var categoria = new CategoriaProduto("Bebidas", TenantId, Usuario);
            var grupoId = Guid.NewGuid();

            categoria.Alterar("Alimentos", grupoId, Usuario);

            Assert.True(categoria.IsValid);
            Assert.Equal("Alimentos", categoria.Descricao);
            Assert.Equal(grupoId, categoria.ProdutoGrupoId);
        }

        // ============================ MarcaProduto ============================

        [Fact]
        public void MarcaProduto_Criar_ComDescricaoValida_DeveSerValida()
        {
            var marca = new MarcaProduto("Marca X", TenantId, Usuario);

            Assert.True(marca.IsValid);
            Assert.Equal("Marca X", marca.Descricao);
        }

        [Fact]
        public void MarcaProduto_Criar_ComDescricaoVazia_DeveSerInvalida()
        {
            var marca = new MarcaProduto("", TenantId, Usuario);

            Assert.False(marca.IsValid);
            Assert.Contains(marca.Notifications, n => n.Key == nameof(MarcaProduto.Descricao));
        }

        [Fact]
        public void MarcaProduto_Alterar_ComDescricaoAcimaDoLimite_DeveSerInvalida()
        {
            var marca = new MarcaProduto("Marca X", TenantId, Usuario);

            marca.Alterar(new string('y', 151), null, Usuario);

            Assert.False(marca.IsValid);
            Assert.Contains(marca.Notifications, n => n.Key == nameof(MarcaProduto.Descricao));
        }

        // ============================ UnidadeMedidaComercial ============================

        [Fact]
        public void UnidadeMedidaComercial_Criar_ComDadosValidos_DeveSerValida()
        {
            var un = new UnidadeMedidaComercial("UN", "Unidade", 1m, TenantId, Usuario);

            Assert.True(un.IsValid);
            Assert.Equal("UN", un.UnidadeMedida);
            Assert.Equal("Unidade", un.Descricao);
            Assert.Equal(1m, un.Fator);
        }

        [Fact]
        public void UnidadeMedidaComercial_Criar_ComFatorZero_DeveSerInvalida()
        {
            var un = new UnidadeMedidaComercial("UN", "Unidade", 0m, TenantId, Usuario);

            Assert.False(un.IsValid);
            Assert.Contains(un.Notifications, n => n.Key == nameof(UnidadeMedidaComercial.Fator));
        }

        [Fact]
        public void UnidadeMedidaComercial_Criar_ComUnidadeAcimaDe6Caracteres_DeveSerInvalida()
        {
            var un = new UnidadeMedidaComercial("UNIDADE", "Unidade", 1m, TenantId, Usuario);

            Assert.False(un.IsValid);
            Assert.Contains(un.Notifications, n => n.Key == nameof(UnidadeMedidaComercial.UnidadeMedida));
        }

        [Fact]
        public void UnidadeMedidaComercial_Criar_ComDescricaoVazia_DeveSerInvalida()
        {
            var un = new UnidadeMedidaComercial("UN", "", 1m, TenantId, Usuario);

            Assert.False(un.IsValid);
            Assert.Contains(un.Notifications, n => n.Key == nameof(UnidadeMedidaComercial.Descricao));
        }

        [Fact]
        public void UnidadeMedidaComercial_Alterar_ComDadosValidos_DeveAtualizar()
        {
            var un = new UnidadeMedidaComercial("UN", "Unidade", 1m, TenantId, Usuario);

            un.Alterar("KG", "Quilograma", 2m, Usuario);

            Assert.True(un.IsValid);
            Assert.Equal("KG", un.UnidadeMedida);
            Assert.Equal("Quilograma", un.Descricao);
            Assert.Equal(2m, un.Fator);
        }

        // ============================ Balanca ============================

        [Fact]
        public void Balanca_Criar_ComDadosValidos_DeveSerValida()
        {
            var balanca = new Balanca("Balanca Caixa 1", 2, 5, 6, 2, ETipoValorBalanca.Peso, TenantId, Usuario);

            Assert.True(balanca.IsValid);
            Assert.Equal("Balanca Caixa 1", balanca.Nome);
            Assert.Equal(ETipoValorBalanca.Peso, balanca.TipoValor);
        }

        [Theory]
        [InlineData(0, 5, 6, 2)] // QntDigitoIdentificador
        [InlineData(2, 0, 6, 2)] // QntDigitoCodigoProduto
        [InlineData(2, 5, 0, 2)] // QntDigitoValorProduto
        [InlineData(2, 5, 6, 0)] // QntCasaDecimal
        public void Balanca_Criar_ComQuantidadeNaoPositiva_DeveSerInvalida(int ident, int codigo, int valor, int casas)
        {
            var balanca = new Balanca("Balanca", ident, codigo, valor, casas, ETipoValorBalanca.Peso, TenantId, Usuario);

            Assert.False(balanca.IsValid);
        }

        [Fact]
        public void Balanca_Criar_ComTipoValorInvalido_DeveSerInvalida()
        {
            var balanca = new Balanca("Balanca", 2, 5, 6, 2, (ETipoValorBalanca)99, TenantId, Usuario);

            Assert.False(balanca.IsValid);
            Assert.Contains(balanca.Notifications, n => n.Key == nameof(Balanca.TipoValor));
        }

        [Fact]
        public void Balanca_DeletarSeNaoVinculada_SemProdutoVinculado_DeveDeletar()
        {
            var balanca = new Balanca("Balanca", 2, 5, 6, 2, ETipoValorBalanca.Peso, TenantId, Usuario);

            balanca.DeletarSeNaoVinculada(Usuario);

            Assert.NotNull(balanca.DeletadoEm);
        }

        [Fact]
        public void Balanca_DeletarSeNaoVinculada_ComProdutoVinculado_DeveNotificarERecusar()
        {
            var balanca = new Balanca("Balanca", 2, 5, 6, 2, ETipoValorBalanca.Peso, TenantId, Usuario);
            var produto = new Produto(
                categoriaId: null,
                marcaProdutoId: null,
                unidadeMedidaComercialId: null,
                codigo: "PROD-1",
                descricao: "Produto 1",
                ean: "7891234567890",
                pesoLiquido: 1m,
                pesoBruto: 1m,
                valorVenda: 10m,
                valorVendaPrazo: 10m,
                valorCompra: 5m,
                tipoProduto: ETipoProduto.MateriaPrima,
                ativo: true,
                imagem: "",
                cestId: null,
                codigoAnpId: null,
                balancaId: balanca.Id,
                utilizaBalanca: false,
                codigoProdutoBalanca: null,
                ncmId: null,
                tenantId: TenantId,
                criadoPor: Usuario);
            balanca.Produtos.Add(produto);

            balanca.DeletarSeNaoVinculada(Usuario);

            Assert.Null(balanca.DeletadoEm);
            Assert.Contains(balanca.Notifications, n => n.Key == "Delecao");
        }

        // ============================ Adicionais ============================

        [Fact]
        public void Adicionais_Criar_ComDescricaoValida_DeveSerValida()
        {
            var adicional = new Adicionais("Bacon extra", 5.50m, TenantId, Usuario);

            Assert.True(adicional.IsValid);
            Assert.Equal("Bacon extra", adicional.Descricao);
            Assert.Equal(5.50m, adicional.ValorPreco);
        }

        [Fact]
        public void Adicionais_Criar_ComDescricaoAcimaDoLimite_DeveSerInvalida()
        {
            var adicional = new Adicionais(new string('z', 61), 5.50m, TenantId, Usuario);

            Assert.False(adicional.IsValid);
            Assert.Contains(adicional.Notifications, n => n.Key == nameof(Adicionais.Descricao));
        }

        [Fact]
        public void Adicionais_Alterar_DeveAtualizarDescricaoEPreco()
        {
            var adicional = new Adicionais("Bacon", 5m, TenantId, Usuario);

            adicional.Alterar("Bacon duplo", 8m, Usuario);

            Assert.True(adicional.IsValid);
            Assert.Equal("Bacon duplo", adicional.Descricao);
            Assert.Equal(8m, adicional.ValorPreco);
        }

        // ============================ ProdutoGrupo ============================

        [Fact]
        public void ProdutoGrupo_Criar_ComDescricaoValida_DeveSerValido()
        {
            var grupo = new ProdutoGrupo("Grupo A", TenantId, Usuario);

            Assert.True(grupo.IsValid);
            Assert.Equal("Grupo A", grupo.Descricao);
        }

        [Fact]
        public void ProdutoGrupo_Criar_ComDescricaoAcimaDoLimite_DeveSerInvalido()
        {
            var grupo = new ProdutoGrupo(new string('g', 101), TenantId, Usuario);

            Assert.False(grupo.IsValid);
            Assert.Contains(grupo.Notifications, n => n.Key == nameof(ProdutoGrupo.Descricao));
        }

        [Fact]
        public void ProdutoGrupo_VincularEmpresa_DeveEvitarDuplicidade()
        {
            var grupo = new ProdutoGrupo("Grupo A", TenantId, Usuario);
            var empresaId = Guid.NewGuid();

            grupo.VincularEmpresa(empresaId, Usuario);
            grupo.VincularEmpresa(empresaId, Usuario); // duplicado — deve ser ignorado

            Assert.Single(grupo.Empresas);
            Assert.Equal(empresaId, grupo.Empresas.First().EmpresaId);
        }

        [Fact]
        public void ProdutoGrupo_DesvincularEmpresa_DeveSoftDeleteDaJuncao()
        {
            var grupo = new ProdutoGrupo("Grupo A", TenantId, Usuario);
            var empresaId = Guid.NewGuid();
            grupo.VincularEmpresa(empresaId, Usuario);

            grupo.DesvincularEmpresa(empresaId, Usuario);

            Assert.NotNull(grupo.Empresas.First().DeletadoEm);
        }
    }
}
