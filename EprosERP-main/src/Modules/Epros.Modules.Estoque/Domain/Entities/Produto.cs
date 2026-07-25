using System;
using System.Collections.Generic;
using System.Linq;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    public class Produto : EntidadeSaaSBase
    {
        // Sequência de exibição/UX (porte do legado SequenciaTenantId — exceção oficial, nome fixo).
        public long? SequenciaExibicao { get; private set; }

        // Campos de Compatibilidade Novo Modelo
        public string Sku { get; private set; } = string.Empty;
        public string Nome { get; private set; } = string.Empty;
        public decimal PrecoVenda { get; private set; }
        public decimal SaldoEstoque { get; private set; }
        public decimal CustoMedio { get; private set; }

        // Campos do Legado
        public Guid? ProdutoGrupoId { get; private set; }
        public Guid? CategoriaId { get; private set; }
        public Guid? MarcaProdutoId { get; private set; }
        public Guid? UnidadeMedidaComercialId { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Ean { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public decimal PesoLiquido { get; private set; }
        public decimal PesoBruto { get; private set; }
        public decimal ValorVenda { get; private set; }
        public decimal ValorVendaPrazo { get; private set; }
        public decimal ValorCompra { get; private set; }
        public ETipoProduto? TipoProduto { get; private set; }
        public bool Ativo { get; private set; }
        public string Imagem { get; private set; } = string.Empty;
        public bool UtilizaBalanca { get; private set; }
        public string CodigoProdutoBalanca { get; private set; } = string.Empty;

        // Fiscais / Outros
        public Guid? NcmId { get; private set; }
        public Guid? CestId { get; private set; }
        public Guid? CodigoAnpId { get; private set; }
        public Guid? BalancaId { get; private set; }

        // Navegação EF Core
        public CategoriaProduto? Categoria { get; private set; }
        public MarcaProduto? MarcaProduto { get; private set; }
        public UnidadeMedidaComercial? UnidadeMedidaComercial { get; private set; }
        public ProdutoGrupo? ProdutoGrupo { get; private set; }
        public Balanca? Balanca { get; private set; }
        public ProdutoEspecifico? ProdutoEspecifico { get; private set; }
        public ICollection<AdicionaisProduto> AdicionaisProduto { get; private set; } = new List<AdicionaisProduto>();
        public ICollection<ProdutoHistoricoReajuste> ProdutoHistoricoReajustes { get; private set; } = new List<ProdutoHistoricoReajuste>();

        protected Produto() { } // EF Core

        public Produto(
            Guid? categoriaId,
            Guid? marcaProdutoId,
            Guid? unidadeMedidaComercialId,
            string? codigo,
            string descricao,
            string? ean,
            decimal pesoLiquido,
            decimal pesoBruto,
            decimal valorVenda,
            decimal valorVendaPrazo,
            decimal valorCompra,
            ETipoProduto? tipoProduto,
            bool ativo,
            string imagem,
            Guid? cestId,
            Guid? codigoAnpId,
            Guid? balancaId,
            bool utilizaBalanca,
            string? codigoProdutoBalanca,
            Guid? ncmId,
            string tenantId,
            string criadoPor)
            : base(tenantId, criadoPor)
        {
            CategoriaId = categoriaId;
            MarcaProdutoId = marcaProdutoId;
            UnidadeMedidaComercialId = unidadeMedidaComercialId;
            Codigo = codigo ?? string.Empty;
            Descricao = descricao;
            Ean = ean ?? "Sem GTIN";
            PesoLiquido = pesoLiquido;
            PesoBruto = pesoBruto;
            ValorVenda = valorVenda;
            ValorVendaPrazo = valorVendaPrazo;
            ValorCompra = valorCompra;
            TipoProduto = tipoProduto;
            Ativo = ativo;
            Imagem = imagem;
            CestId = cestId;
            CodigoAnpId = codigoAnpId;
            BalancaId = balancaId;
            UtilizaBalanca = utilizaBalanca;
            CodigoProdutoBalanca = codigoProdutoBalanca ?? string.Empty;
            NcmId = ncmId;

            // Atribuições para o Novo Modelo
            Sku = Codigo;
            Nome = Descricao;
            PrecoVenda = ValorVenda;
            SaldoEstoque = 0;
            CustoMedio = 0;

            Validar();
        }

        public Produto(string sku, string nome, decimal precoVenda, string tenantId, string criadoPor)
            : this(
                  categoriaId: null,
                  marcaProdutoId: null,
                  unidadeMedidaComercialId: null,
                  codigo: sku,
                  descricao: nome,
                  ean: "Sem GTIN",
                  pesoLiquido: 0m,
                  pesoBruto: 0m,
                  valorVenda: precoVenda,
                  valorVendaPrazo: precoVenda,
                  valorCompra: 0m,
                  tipoProduto: ETipoProduto.MateriaPrima,
                  ativo: true,
                  imagem: string.Empty,
                  cestId: null,
                  codigoAnpId: null,
                  balancaId: null,
                  utilizaBalanca: false,
                  codigoProdutoBalanca: null,
                  ncmId: null,
                  tenantId: tenantId,
                  criadoPor: criadoPor)
        {
        }

        public void Validar()
        {
            AddNotifications(new Contract<Produto>()
                .Requires()
                .IsLowerOrEqualsThan(Codigo.Length, 60, nameof(Codigo), "O campo Codigo deve ter no máximo 60 caracteres [Origem: Produto]")
                .IsLowerOrEqualsThan(Descricao.Length, 120, nameof(Descricao), "O campo Descricao deve ter no máximo 120 caracteres [Origem: Produto]")
                .IsLowerOrEqualsThan(Ean.Length, 14, nameof(Ean), "O campo Ean deve ter no máximo 14 caracteres [Origem: Produto]")
                .IsLowerOrEqualsThan(CodigoProdutoBalanca.Length, 13, nameof(CodigoProdutoBalanca), "O campo Código de Produto da Balança deve ter no máximo 13 caracteres [Origem: Produto]")
                .IsGreaterThan(ValorVenda, -0.01m, nameof(ValorVenda), "O Preço de Venda não pode ser negativo [Origem: Produto]")
            );

            if (UtilizaBalanca)
            {
                if (BalancaId == Guid.Empty || BalancaId == null)
                {
                    AddNotification("BalancaIdInvalido", "O campo ID da Balança não pode ser Nulo ou Vazio [Origem: Produto]");
                }

                if (!IsDigitsOnly(CodigoProdutoBalanca))
                {
                    AddNotification("CodigoProdutoBalancaInvalido", "O campo Código de Produto da Balança deve conter apenas números e não pode estar vazio [Origem: Produto]");
                }
            }
        }

        private bool IsDigitsOnly(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return false;
            foreach (char c in str)
            {
                if (!char.IsDigit(c)) return false;
            }
            return true;
        }

        public void Alterar(
            Guid? categoriaId,
            Guid? marcaProdutoId,
            Guid? unidadeMedidaComercialId,
            string? codigo,
            string descricao,
            string? ean,
            decimal pesoLiquido,
            decimal pesoBruto,
            decimal valorVenda,
            decimal valorVendaPrazo,
            decimal valorCompra,
            ETipoProduto? tipoProduto,
            bool ativo,
            Guid? cestId,
            Guid? codigoAnpId,
            Guid? balancaId,
            bool utilizaBalanca,
            string? codigoProdutoBalanca,
            Guid? ncmId,
            string usuario)
        {
            CategoriaId = categoriaId;
            MarcaProdutoId = marcaProdutoId;
            UnidadeMedidaComercialId = unidadeMedidaComercialId;
            Codigo = codigo ?? string.Empty;
            Descricao = descricao;
            Ean = ean ?? "Sem GTIN";
            PesoLiquido = pesoLiquido;
            PesoBruto = pesoBruto;
            ValorVenda = valorVenda;
            ValorVendaPrazo = valorVendaPrazo;
            ValorCompra = valorCompra;
            TipoProduto = tipoProduto;
            Ativo = ativo;
            CestId = cestId;
            CodigoAnpId = codigoAnpId;
            BalancaId = balancaId;
            UtilizaBalanca = utilizaBalanca;
            CodigoProdutoBalanca = codigoProdutoBalanca ?? string.Empty;
            NcmId = ncmId;

            // Sincroniza Novo Modelo
            Sku = Codigo;
            Nome = Descricao;
            PrecoVenda = ValorVenda;

            MarcarAlterado(usuario);
            Validar();
        }

        public void LancarEntradaEstoque(decimal quantidade, decimal precoUnitario, string usuario)
        {
            if (quantidade <= 0)
            {
                AddNotification(nameof(SaldoEstoque), "A quantidade de entrada deve ser maior que zero.");
                return;
            }

            if (precoUnitario <= 0)
            {
                AddNotification(nameof(CustoMedio), "O preço unitário de entrada deve ser maior que zero.");
                return;
            }

            decimal novoSaldo = SaldoEstoque + quantidade;
            decimal custoTotalAnterior = SaldoEstoque * CustoMedio;
            decimal custoTotalNovoItem = quantidade * precoUnitario;
            decimal novoCustoMedio = (custoTotalAnterior + custoTotalNovoItem) / novoSaldo;

            SaldoEstoque = novoSaldo;
            CustoMedio = novoCustoMedio;

            MarcarAlterado(usuario);
        }

        public void LancarSaidaEstoque(decimal quantidade, string usuario)
        {
            if (quantidade <= 0)
            {
                AddNotification(nameof(SaldoEstoque), "A quantidade de saída deve ser maior que zero.");
                return;
            }

            if (SaldoEstoque < quantidade)
            {
                AddNotification(nameof(SaldoEstoque), "Saldo de estoque insuficiente para realizar a saída.");
                return;
            }

            SaldoEstoque -= quantidade;
            MarcarAlterado(usuario);
        }

        public void RestaurarEstoque(decimal quantidade, string usuario)
        {
            if (quantidade <= 0)
            {
                AddNotification(nameof(SaldoEstoque), "A quantidade a restaurar deve ser maior que zero.");
                return;
            }

            SaldoEstoque += quantidade;
            MarcarAlterado(usuario);
        }

        // ------- Métodos portados do legado (Produto) -------

        /// <summary>Porte fiel de Produto.AtualizarSequenciaTenantId(): define a sequência de exibição/UX.</summary>
        public void AtualizarSequenciaExibicao(long sequenciaExibicao)
        {
            SequenciaExibicao = sequenciaExibicao;
        }

        /// <summary>Porte fiel de Produto.AlterarImagem().</summary>
        public void AlterarImagem(string imagem, string usuario)
        {
            Imagem = imagem;
            MarcarAlterado(usuario);
        }

        /// <summary>Define/atualiza o grupo de produtos (campo do legado ProdutoGrupoId).</summary>
        public void DefinirProdutoGrupo(Guid? produtoGrupoId, string usuario)
        {
            ProdutoGrupoId = produtoGrupoId;
            MarcarAlterado(usuario);
        }

        /// <summary>Porte fiel de Produto.AlterarValorVenda(): grava o novo valor e registra o histórico de reajuste.</summary>
        public void AlterarValorVenda(decimal valorAtual, decimal fator, decimal valorFixo, decimal valorNovo, string? motivo, string usuario)
        {
            ValorVenda = valorNovo;
            PrecoVenda = valorNovo;
            MarcarAlterado(usuario);

            var historico = new ProdutoHistoricoReajuste(
                Id,
                Codigo,
                valorAtual,
                ETipoReajuste.ValorVenda,
                fator,
                valorFixo,
                valorNovo,
                motivo,
                TenantId,
                usuario);

            ProdutoHistoricoReajustes.Add(historico);
        }

        /// <summary>Porte fiel de Produto.AlterarValorCompra(): grava o novo valor e registra o histórico de reajuste.</summary>
        public void AlterarValorCompra(decimal valorAtual, decimal fator, decimal valorFixo, decimal valorNovo, string? motivo, string usuario)
        {
            ValorCompra = valorNovo;
            MarcarAlterado(usuario);

            var historico = new ProdutoHistoricoReajuste(
                Id,
                Codigo,
                valorAtual,
                ETipoReajuste.Valorcompra,
                fator,
                valorFixo,
                valorNovo,
                motivo,
                TenantId,
                usuario);

            ProdutoHistoricoReajustes.Add(historico);
        }

        /// <summary>Porte fiel de Produto.AdicionarAdicionaisProduto().</summary>
        public void AdicionarAdicionaisProduto(Guid adicionaisId, string usuario)
        {
            AdicionaisProduto.Add(new AdicionaisProduto(Id, adicionaisId, TenantId, usuario));
        }

        /// <summary>Porte fiel de Produto.AlterarAdicionaisProduto().</summary>
        public void AlterarAdicionaisProduto(Guid adicionaisProdutoId, Guid adicionaisId, string usuario)
        {
            var obj = AdicionaisProduto.FirstOrDefault(a => a.Id == adicionaisProdutoId);
            obj?.Alterar(adicionaisId, usuario);
        }

        /// <summary>Porte fiel de Produto.DeletarAdicionaisProduto().</summary>
        public void DeletarAdicionaisProduto(Guid adicionaisProdutoId, string usuario)
        {
            var obj = AdicionaisProduto.FirstOrDefault(a => a.Id == adicionaisProdutoId);
            obj?.Deletar(usuario);
        }

        /// <summary>Porte fiel de Produto.AdicionarProdutoEspecifico().</summary>
        public void AdicionarProdutoEspecifico(decimal valorPercentualGlpDerivadoPetroleo, decimal valorPercentualGasNaturalNacional, decimal valorPercentualGasNaturalImportado, decimal valorPartida, EEstado ufConsumo, string usuario)
        {
            ProdutoEspecifico = new ProdutoEspecifico(Id, valorPercentualGlpDerivadoPetroleo, valorPercentualGasNaturalNacional, valorPercentualGasNaturalImportado, valorPartida, ufConsumo, TenantId, usuario);

            if (ProdutoEspecifico != null)
            {
                AddNotifications(ProdutoEspecifico.Notifications);
            }
        }

        /// <summary>Porte fiel de Produto.AlterarProdutoEspecifico().</summary>
        public void AlterarProdutoEspecifico(decimal valorPercentualGlpDerivadoPetroleo, decimal valorPercentualGasNaturalNacional, decimal valorPercentualGasNaturalImportado, decimal valorPartida, EEstado ufConsumo, string usuario)
        {
            ProdutoEspecifico?.Alterar(valorPercentualGlpDerivadoPetroleo, valorPercentualGasNaturalNacional, valorPercentualGasNaturalImportado, valorPartida, ufConsumo, usuario);

            if (ProdutoEspecifico != null)
            {
                AddNotifications(ProdutoEspecifico.Notifications);
            }
        }

        /// <summary>Porte fiel de Produto.Deletar(): soft-delete em cascata de adicionais e específico.</summary>
        public void DeletarEmCascata(string usuario)
        {
            Deletar(usuario);
            AdicionaisProduto.ToList().ForEach(o => o.Deletar(usuario));
            ProdutoEspecifico?.DeletarComOrigens(usuario);
        }
    }
}
