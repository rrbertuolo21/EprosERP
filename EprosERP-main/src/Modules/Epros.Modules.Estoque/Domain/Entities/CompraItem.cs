using System;
using System.Collections.Generic;
using Epros.Modules.Estoque.Domain.Enums;
using Epros.Shared.Domain.Entities;
using Flunt.Validations;

namespace Epros.Modules.Estoque.Domain.Entities
{
    public class CompraItem : EntidadeSaaSBase
    {
        public Guid CompraId { get; private set; }
        public Guid ProdutoId { get; private set; }
        public decimal Quantidade { get; private set; }
        public decimal PrecoUnitario { get; private set; }
        public decimal ValorIms { get; private set; } // ICMS
        public decimal ValorIpi { get; private set; }
        public decimal ValorTotal { get; private set; }

        // Campos Expandidos do Legado
        public string CodigoProduto { get; private set; } = string.Empty;
        public string? CodigoEan { get; private set; } = "SEM GTIN";
        public string DescricaoProduto { get; private set; } = string.Empty;
        public string Ncm { get; private set; } = string.Empty;
        public string? ExcecaoNcmTipi { get; private set; }  // 2-3
        public Guid? CestId { get; private set; }
        public string? Cest { get; private set; }
        public Guid? CodigoAnpId { get; private set; }
        public string? CodigoAnp { get; private set; }
        public int Cfop { get; private set; }
        public string UnidadeComercial { get; private set; } = string.Empty;
        public decimal QuantidadeComercial { get; private set; }
        public decimal ValorUnitarioComercial { get; private set; }
        public decimal ValorTotalBrutoProdutos { get; private set; }
        public string? CodigoEanTributavel { get; private set; } = "SEM GTIN";
        public string UnidadeTributavel { get; private set; } = string.Empty;
        public decimal QuantidadeTributavel { get; private set; }
        public decimal ValorUnitarioTributavel { get; private set; }
        public decimal ValorDesconto { get; private set; }
        public decimal ValorDescontoRateado { get; private set; }
        public decimal ValorFreteRateado { get; private set; }
        public decimal ValorSeguroRateado { get; private set; }
        public decimal ValorOutrasDespesasAcessoriasRateado { get; private set; }
        public EIndicadorTotalizador CompoeValorTotal { get; private set; }
        public string? InformacoesAdicionaisDoProduto { get; private set; }  // 500
        public int CfopCorrelacao { get; private set; }
        public bool IntegraFaturamento { get; private set; }
        public int NumeroItemPedidoCompra { get; private set; }
        public string? NumeroPedidoCompra { get; private set; }  // 60
        public string? FichaConteudoImportacao { get; private set; }  // 36
        public string? CodigoBeneficioFiscal { get; private set; }
        public decimal ValorCusto { get; private set; }

        // Navegação intra-módulo (agregados fiscais e satélites do item)
        public CompraItemImposto? Imposto { get; private set; }
        public CompraItemImpostoValorAproximado? ImpostoValorAproximado { get; private set; }
        public CompraItemImpostoIbsCbs? ImpostoIbsCbs { get; private set; }
        public CompraItemCombustivel? Combustivel { get; private set; }
        public ICollection<CompraItemImportacao> Importacoes { get; private set; } = new List<CompraItemImportacao>();
        public Compra? Compra { get; private set; }

        protected CompraItem() { } // EF Core

        public CompraItem(
            Guid compraId,
            Guid produtoId,
            decimal quantidade,
            decimal precoUnitario,
            decimal valorIms,
            decimal valorIpi,
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
            decimal valorCusto = 0m,
            string? excecaoNcmTipi = null,
            string? codigoEanTributavel = null,
            string? unidadeTributavel = null,
            decimal valorDescontoRateado = 0m,
            decimal valorSeguroRateado = 0m,
            decimal valorOutrasDespesasAcessoriasRateado = 0m,
            EIndicadorTotalizador compoeValorTotal = EIndicadorTotalizador.ValorDoItemCompoeTotalNF,
            string? informacoesAdicionaisDoProduto = null,
            int cfopCorrelacao = 0,
            bool integraFaturamento = false,
            int numeroItemPedidoCompra = 0,
            string? numeroPedidoCompra = null,
            string? fichaConteudoImportacao = null,
            string? codigoBeneficioFiscal = null)
            : base(tenantId, criadoPor)
        {
            AddNotifications(new Contract<CompraItem>()
                .Requires()
                .AreNotEquals(compraId, Guid.Empty, nameof(CompraId), "O ID da compra é obrigatório.")
                .AreNotEquals(produtoId, Guid.Empty, nameof(ProdutoId), "O ID do produto é obrigatório.")
                .IsGreaterThan(quantidade, 0, nameof(Quantidade), "A Quantidade do item deve ser maior que zero.")
                .IsGreaterThan(precoUnitario, -0.01m, nameof(PrecoUnitario), "O Preço Unitário não pode ser negativo.")
                .IsGreaterThan(valorIms, -0.01m, nameof(ValorIms), "O Valor de ICMS não pode ser negativo.")
                .IsGreaterThan(valorIpi, -0.01m, nameof(ValorIpi), "O Valor de IPI não pode ser negativo.")
            );

            CompraId = compraId;
            ProdutoId = produtoId;
            Quantidade = quantidade;
            PrecoUnitario = precoUnitario;
            ValorIms = valorIms;
            ValorIpi = valorIpi;
            ValorTotal = quantidade * precoUnitario;

            CodigoProduto = codigoProduto ?? string.Empty;
            CodigoEan = codigoEan ?? "SEM GTIN";
            DescricaoProduto = descricaoProduto ?? string.Empty;
            Ncm = ncm ?? string.Empty;
            ExcecaoNcmTipi = excecaoNcmTipi;
            CestId = cestId;
            Cest = cest;
            CodigoAnpId = codigoAnpId;
            CodigoAnp = codigoAnp;
            Cfop = cfop;
            UnidadeComercial = unidadeComercial ?? string.Empty;
            QuantidadeComercial = quantidade;
            ValorUnitarioComercial = precoUnitario;
            ValorTotalBrutoProdutos = ValorTotal;
            CodigoEanTributavel = codigoEanTributavel ?? "SEM GTIN";
            UnidadeTributavel = unidadeTributavel ?? (unidadeComercial ?? string.Empty);
            QuantidadeTributavel = quantidade;
            ValorUnitarioTributavel = precoUnitario;
            ValorDesconto = valorDesconto;
            ValorDescontoRateado = valorDescontoRateado;
            ValorFreteRateado = valorFreteRateado;
            ValorSeguroRateado = valorSeguroRateado;
            ValorOutrasDespesasAcessoriasRateado = valorOutrasDespesasAcessoriasRateado;
            CompoeValorTotal = compoeValorTotal;
            InformacoesAdicionaisDoProduto = informacoesAdicionaisDoProduto;
            CfopCorrelacao = cfopCorrelacao;
            IntegraFaturamento = integraFaturamento;
            NumeroItemPedidoCompra = numeroItemPedidoCompra;
            NumeroPedidoCompra = numeroPedidoCompra;
            FichaConteudoImportacao = fichaConteudoImportacao;
            CodigoBeneficioFiscal = codigoBeneficioFiscal;
            ValorCusto = valorCusto;
        }

        /// <summary>Porte fiel de CompraItem.Alterar(): atualiza os dados comerciais/tributáveis do item.</summary>
        public void Alterar(
            Guid produtoId,
            string codigoProduto,
            string? codigoEan,
            string descricaoProduto,
            string ncm,
            string? excecaoNcmTipi,
            Guid? cestId,
            Guid? codigoAnpId,
            int cfop,
            string unidadeComercial,
            decimal quantidadeComercial,
            decimal valorUnitarioComercial,
            decimal valorTotalBrutoProdutos,
            string? codigoEanTributavel,
            string unidadeTributavel,
            decimal quantidadeTributavel,
            decimal valorUnitarioTributavel,
            decimal valorDesconto,
            decimal valorDescontoRateado,
            decimal valorFreteRateado,
            decimal valorSeguroRateado,
            decimal valorOutrasDespesasAcessoriasRateado,
            EIndicadorTotalizador compoeValorTotal,
            string? informacoesAdicionaisDoProduto,
            int cfopCorrelacao,
            bool integraFaturamento,
            int numeroItemPedidoCompra,
            string? numeroPedidoCompra,
            string? fichaConteudoImportacao,
            string usuario)
        {
            ProdutoId = produtoId;
            CodigoProduto = codigoProduto ?? string.Empty;
            CodigoEan = codigoEan;
            DescricaoProduto = descricaoProduto ?? string.Empty;
            Ncm = ncm ?? string.Empty;
            ExcecaoNcmTipi = excecaoNcmTipi;
            CestId = cestId;
            CodigoAnpId = codigoAnpId;
            Cfop = cfop;
            UnidadeComercial = unidadeComercial ?? string.Empty;
            QuantidadeComercial = quantidadeComercial;
            ValorUnitarioComercial = valorUnitarioComercial;
            ValorTotalBrutoProdutos = valorTotalBrutoProdutos;
            CodigoEanTributavel = codigoEanTributavel;
            UnidadeTributavel = unidadeTributavel ?? string.Empty;
            QuantidadeTributavel = quantidadeTributavel;
            ValorUnitarioTributavel = valorUnitarioTributavel;
            ValorDesconto = valorDesconto;
            ValorDescontoRateado = valorDescontoRateado;
            ValorFreteRateado = valorFreteRateado;
            ValorSeguroRateado = valorSeguroRateado;
            ValorOutrasDespesasAcessoriasRateado = valorOutrasDespesasAcessoriasRateado;
            CompoeValorTotal = compoeValorTotal;
            InformacoesAdicionaisDoProduto = informacoesAdicionaisDoProduto;
            CfopCorrelacao = cfopCorrelacao;
            IntegraFaturamento = integraFaturamento;
            NumeroItemPedidoCompra = numeroItemPedidoCompra;
            NumeroPedidoCompra = numeroPedidoCompra;
            FichaConteudoImportacao = fichaConteudoImportacao;
            MarcarAlterado(usuario);
        }

        /// <summary>Vincula o agregado de imposto (ICMS/ST/FCP/IPI/PIS/COFINS) ao item.</summary>
        public void DefinirImposto(CompraItemImposto imposto)
        {
            Imposto = imposto;
            if (imposto != null && imposto.Notifications.Count > 0)
                AddNotifications(imposto.Notifications);
        }

        /// <summary>Vincula o agregado de imposto IBS/CBS (reforma tributária) ao item.</summary>
        public void DefinirImpostoIbsCbs(CompraItemImpostoIbsCbs impostoIbsCbs)
        {
            ImpostoIbsCbs = impostoIbsCbs;
        }

        /// <summary>Vincula o valor aproximado dos tributos (Lei da Transparência) ao item.</summary>
        public void DefinirImpostoValorAproximado(CompraItemImpostoValorAproximado impostoValorAproximado)
        {
            ImpostoValorAproximado = impostoValorAproximado;
        }

        /// <summary>Vincula os dados de combustível (ANP) ao item.</summary>
        public void DefinirCombustivel(CompraItemCombustivel combustivel)
        {
            Combustivel = combustivel;
            if (combustivel != null && combustivel.Notifications.Count > 0)
                AddNotifications(combustivel.Notifications);
        }

        /// <summary>Adiciona uma declaração de importação ao item.</summary>
        public void AdicionarItemImportacao(CompraItemImportacao importacao)
        {
            Importacoes.Add(importacao);
        }
    }
}
