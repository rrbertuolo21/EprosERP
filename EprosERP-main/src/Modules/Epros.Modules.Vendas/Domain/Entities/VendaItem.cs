using System;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Epros.Shared.Domain.Enums;
using Flunt.Validations;

namespace Epros.Modules.Vendas.Domain.Entities
{
    /// <summary>
    /// Porte fiel de VendaItem (item da venda). FK long -> Guid; herda EntidadeSaaSBase.
    /// Mantém o construtor simplificado usado pelos handlers existentes (RegistrarVenda)
    /// e adiciona a totalidade dos campos/filhos fiscais do legado.
    /// </summary>
    public class VendaItem : EntidadeSaaSBase
    {
        public Guid VendaId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal PrecoUnitario { get; private set; }
        public decimal ValorTotal { get; private set; }

        // Campos do legado (porte campo a campo)
        public string CodigoProduto { get; private set; } = string.Empty;   // 1-60
        public string? CodigoEan { get; private set; } = "SEM GTIN";        // 0,8,12,13,14
        public string DescricaoProduto { get; private set; } = string.Empty; // 1-120
        public string Ncm { get; private set; } = string.Empty;             // 8
        public string? ExcecaoNcmTipi { get; private set; }                 // 2-3
        public Guid? CestId { get; private set; }
        public string? Cest { get; private set; }                           // 7
        public Guid? CodigoAnpId { get; private set; }
        public string? CodigoAnp { get; private set; }                      // 9
        public int Cfop { get; private set; }                               // 4
        public string UnidadeComercial { get; private set; } = string.Empty; // 1-6
        public decimal QuantidadeComercial { get; private set; }            // 15,4
        public decimal ValorUnitarioComercial { get; private set; }         // 21,10
        public decimal ValorTotalBrutoProdutos { get; private set; }        // 15,2
        public string? CodigoEanTributavel { get; private set; } = "SEM GTIN";
        public string UnidadeTributavel { get; private set; } = string.Empty; // 1-6
        public decimal QuantidadeTributavel { get; private set; }           // 15,4
        public decimal ValorUnitarioTributavel { get; private set; }        // 21,10
        public decimal ValorDesconto { get; private set; }                  // 15,2
        public decimal ValorDescontoRateado { get; private set; }           // 15,2
        public decimal ValorFreteRateado { get; private set; }              // 15,2
        public decimal ValorSeguroRateado { get; private set; }             // 15,2
        public decimal ValorOutrasDepesasAcessoriasRateado { get; private set; } // 15,2
        public EIndicadorTotalizador CompoeValorTotal { get; private set; }
        public string? InformacoesAdicionaisDoProduto { get; private set; } // 500
        public int CfopCorrelacao { get; private set; }
        public bool IntegraFaturamento { get; private set; }
        public int NumeroItemPedidoCompra { get; private set; }
        public string? NumeroPedidoCompra { get; private set; }             // 60
        public string? FichaConteudoImportacao { get; private set; }        // 36
        public string? CodigoBeneficioFiscal { get; private set; }          // 36
        public decimal ValorCusto { get; private set; }

        // Navegações intra-módulo (filhos fiscais do item)
        public Venda Venda { get; private set; } = null!;
        public VendaItemImposto? Imposto { get; private set; }
        public VendaItemImpostoValorAproximado? ImpostoValorAproximado { get; private set; }
        public VendaItemCombustivel? Combustivel { get; private set; }
        public VendaItemImpostoIbsCbs? ImpostoIbsCbs { get; private set; }

        protected VendaItem() { } // EF Core

        /// <summary>
        /// Construtor simplificado (compatível com os handlers já existentes).
        /// </summary>
        public VendaItem(
            Guid vendaId,
            Guid produtoId,
            decimal quantidade,
            decimal precoUnitario,
            string tenantId,
            string criadoPor,
            string? codigoProduto = null,
            string? codigoEan = null,
            string? descricaoProduto = null,
            string? ncm = null,
            Guid? cestId = null,
            string? cest = null,
            Guid? codigoAnpId = null,
            string? codigoAnp = null,
            int cfop = 0,
            string? unidadeComercial = null,
            decimal valorDesconto = 0m,
            decimal valorFreteRateado = 0m,
            decimal valorCusto = 0m)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<VendaItem>()
                .Requires()
                .AreNotEquals(vendaId, Guid.Empty, nameof(VendaId), "O ID da venda é obrigatório.")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O ID do produto é obrigatório.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A Quantidade do item deve ser maior que zero.")
                .IsGreaterThan(precoUnitario, -0.01m, nameof(PrecoUnitario), "O Preço Unitário não pode ser negativo.")
            );

            VendaId = vendaId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            PrecoUnitario = precoUnitario;
            ValorTotal = quantidade * precoUnitario;

            CodigoProduto = codigoProduto ?? string.Empty;
            CodigoEan = codigoEan ?? "SEM GTIN";
            DescricaoProduto = descricaoProduto ?? string.Empty;
            Ncm = ncm ?? string.Empty;
            CestId = cestId;
            Cest = cest;
            CodigoAnpId = codigoAnpId;
            CodigoAnp = codigoAnp;
            Cfop = cfop;
            UnidadeComercial = unidadeComercial ?? string.Empty;
            QuantidadeComercial = quantidade;
            ValorUnitarioComercial = precoUnitario;
            ValorTotalBrutoProdutos = ValorTotal;
            CodigoEanTributavel = codigoEan ?? "SEM GTIN";
            UnidadeTributavel = unidadeComercial ?? string.Empty;
            QuantidadeTributavel = quantidade;
            ValorUnitarioTributavel = precoUnitario;
            ValorDesconto = valorDesconto;
            ValorFreteRateado = valorFreteRateado;
            ValorCusto = valorCusto;
        }

        /// <summary>
        /// Construtor completo (porte fiel do construtor do VendaItem legado, campo a campo).
        /// </summary>
        public VendaItem(
            Guid vendaId, Guid produtoId, string codigoProduto, string? codigoEan, string descricaoProduto,
            string ncm, string? excecaoNcmTipi, Guid? cestId, string? cest, Guid? codigoAnpId, string? codigoAnp,
            int cfop, string unidadeComercial, decimal quantidadeComercial, decimal valorUnitarioComercial,
            decimal valorTotalBrutoProdutos, string? codigoEanTributavel, string unidadeTributavel,
            decimal quantidadeTributavel, decimal valorUnitarioTributavel, decimal valorDesconto,
            decimal valorDescontoRateado, decimal valorFreteRateado, decimal valorSeguroRateado,
            decimal valorOutrasDepesasAcessoriasRateado, EIndicadorTotalizador compoeValorTotal,
            string? informacoesAdicionaisDoProduto, int cfopCorrelacao, bool integraFaturamento,
            int numeroItemPedidoCompra, string? numeroPedidoCompra, string? fichaConteudoImportacao,
            string? codigoBeneficioFiscal, decimal valorCusto,
            string tenantId, string criadoPor)
            : base(tenantId, criadoPor)
        {
            VendaId = vendaId;
            ProdutoId = produtoId;
            CodigoProduto = codigoProduto;
            CodigoEan = codigoEan;
            DescricaoProduto = descricaoProduto;
            Ncm = ncm;
            ExcecaoNcmTipi = excecaoNcmTipi;
            CestId = cestId;
            Cest = cest;
            CodigoAnpId = codigoAnpId;
            CodigoAnp = codigoAnp;
            Cfop = cfop;
            UnidadeComercial = unidadeComercial;
            QuantidadeComercial = quantidadeComercial;
            ValorUnitarioComercial = valorUnitarioComercial;
            ValorTotalBrutoProdutos = valorTotalBrutoProdutos;
            CodigoEanTributavel = codigoEanTributavel;
            UnidadeTributavel = unidadeTributavel;
            QuantidadeTributavel = quantidadeTributavel;
            ValorUnitarioTributavel = valorUnitarioTributavel;
            ValorDesconto = valorDesconto;
            ValorDescontoRateado = valorDescontoRateado;
            ValorFreteRateado = valorFreteRateado;
            ValorSeguroRateado = valorSeguroRateado;
            ValorOutrasDepesasAcessoriasRateado = valorOutrasDepesasAcessoriasRateado;
            CompoeValorTotal = compoeValorTotal;
            InformacoesAdicionaisDoProduto = informacoesAdicionaisDoProduto;
            CfopCorrelacao = cfopCorrelacao;
            IntegraFaturamento = integraFaturamento;
            NumeroItemPedidoCompra = numeroItemPedidoCompra;
            NumeroPedidoCompra = numeroPedidoCompra;
            FichaConteudoImportacao = fichaConteudoImportacao;
            CodigoBeneficioFiscal = codigoBeneficioFiscal;
            ValorCusto = valorCusto;
            ValorTotal = valorTotalBrutoProdutos;
            Quantidade = quantidadeComercial;
            PrecoUnitario = valorUnitarioComercial;
        }

        /// <summary>Porte fiel de VendaItem.Alterar (dados cadastrais do item).</summary>
        public void Alterar(Guid produtoId, string codigoProduto, string? codigoEan, string descricaoProduto, string ncm, string? excecaoNcmTipi, Guid? cestId, Guid? codigoAnpId, int cfop, string unidadeComercial, decimal quantidadeComercial, decimal valorUnitarioComercial, decimal valorTotalBrutoProdutos, string? codigoEanTributavel, string unidadeTributavel, decimal quantidadeTributavel, decimal valorUnitarioTributavel, decimal valorDesconto, decimal valorDescontoRateado, decimal valorFreteRateado, decimal valorSeguroRateado, decimal valorOutrasDepesasAcessoriasRateado, EIndicadorTotalizador compoeValorTotal, string? informacoesAdicionaisDoProduto, int cfopCorrelacao, bool integraFaturamento, int numeroItemPedidoCompra, string? numeroPedidoCompra, string? fichaConteudoImportacao, string alteradoPor)
        {
            ProdutoId = produtoId;
            CodigoProduto = codigoProduto;
            CodigoEan = codigoEan;
            DescricaoProduto = descricaoProduto;
            Ncm = ncm;
            ExcecaoNcmTipi = excecaoNcmTipi;
            CestId = cestId;
            CodigoAnpId = codigoAnpId;
            Cfop = cfop;
            UnidadeComercial = unidadeComercial;
            QuantidadeComercial = quantidadeComercial;
            ValorUnitarioComercial = valorUnitarioComercial;
            ValorTotalBrutoProdutos = valorTotalBrutoProdutos;
            CodigoEanTributavel = codigoEanTributavel;
            UnidadeTributavel = unidadeTributavel;
            QuantidadeTributavel = quantidadeTributavel;
            ValorUnitarioTributavel = valorUnitarioTributavel;
            ValorDesconto = valorDesconto;
            ValorDescontoRateado = valorDescontoRateado;
            ValorFreteRateado = valorFreteRateado;
            ValorSeguroRateado = valorSeguroRateado;
            ValorOutrasDepesasAcessoriasRateado = valorOutrasDepesasAcessoriasRateado;
            CompoeValorTotal = compoeValorTotal;
            InformacoesAdicionaisDoProduto = informacoesAdicionaisDoProduto;
            CfopCorrelacao = cfopCorrelacao;
            IntegraFaturamento = integraFaturamento;
            NumeroItemPedidoCompra = numeroItemPedidoCompra;
            NumeroPedidoCompra = numeroPedidoCompra;
            FichaConteudoImportacao = fichaConteudoImportacao;
            ValorTotal = valorTotalBrutoProdutos;
            Quantidade = quantidadeComercial;
            PrecoUnitario = valorUnitarioComercial;
            MarcarAlterado(alteradoPor);
        }

        /// <summary>Associa (ou atualiza) o imposto ICMS/ST/IPI/PIS/COFINS do item (porte de DefinirImposto).</summary>
        public void DefinirImposto(VendaItemImposto imposto)
        {
            Imposto = imposto;
            AddNotifications(imposto.Notifications);
        }

        /// <summary>Associa (ou atualiza) o imposto IBS/CBS do item (porte de DefinirImpostoIbsCbs).</summary>
        public void DefinirImpostoIbsCbs(VendaItemImpostoIbsCbs impostoIbsCbs)
        {
            ImpostoIbsCbs = impostoIbsCbs;
        }

        /// <summary>Associa (ou atualiza) o valor aproximado dos tributos (IBPT) do item.</summary>
        public void DefinirImpostoValorAproximado(VendaItemImpostoValorAproximado impostoValorAproximado)
        {
            ImpostoValorAproximado = impostoValorAproximado;
        }

        /// <summary>Associa (ou atualiza) os dados de combustível do item (porte de DefinirCombustivel).</summary>
        public void DefinirCombustivel(VendaItemCombustivel combustivel)
        {
            Combustivel = combustivel;
            AddNotifications(combustivel.Notifications);
        }

        /// <summary>
        /// Porte fiel de VendaItem.Duplicar (novo Id/FK, incl. imposto ICMS/ST/IPI/PIS/COFINS,
        /// imposto IBS/CBS (+tributação regular), valor aproximado IBPT e combustível (+origens)).
        /// </summary>
        public VendaItem Duplicar(Guid novaVendaId, string criadoPor)
        {
            var clone = new VendaItem(
                novaVendaId, ProdutoId, CodigoProduto, CodigoEan, DescricaoProduto, Ncm, ExcecaoNcmTipi, CestId, Cest,
                CodigoAnpId, CodigoAnp, Cfop, UnidadeComercial, QuantidadeComercial, ValorUnitarioComercial, ValorTotalBrutoProdutos,
                CodigoEanTributavel, UnidadeTributavel, QuantidadeTributavel, ValorUnitarioTributavel, ValorDesconto, ValorDescontoRateado,
                ValorFreteRateado, ValorSeguroRateado, ValorOutrasDepesasAcessoriasRateado, CompoeValorTotal, InformacoesAdicionaisDoProduto,
                CfopCorrelacao, IntegraFaturamento, NumeroItemPedidoCompra, NumeroPedidoCompra, FichaConteudoImportacao,
                CodigoBeneficioFiscal, ValorCusto, TenantId, criadoPor);

            if (Imposto is not null) clone.Imposto = Imposto.Duplicar(clone.Id, criadoPor);
            if (ImpostoIbsCbs is not null) clone.ImpostoIbsCbs = ImpostoIbsCbs.Duplicar(clone.Id, criadoPor);
            if (ImpostoValorAproximado is not null) clone.ImpostoValorAproximado = ImpostoValorAproximado.Duplicar(clone.Id, criadoPor);
            if (Combustivel is not null) clone.Combustivel = Combustivel.Duplicar(clone.Id, criadoPor);
            return clone;
        }
    }
}
