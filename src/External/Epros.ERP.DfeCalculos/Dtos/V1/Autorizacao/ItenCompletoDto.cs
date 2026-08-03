using Epros.ERP.DfeCalculos.Models.Vendas;

namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class ItenCompletoDto
    {
        public ItenCompletoDto() { }

        public ItenCompletoDto(long produtoId, string? codigoProduto, string? nomeProduto, string? codigoBarras, string? ncm, VendaItemTributacaoCfop tributacaoCfop, string? unidade, string unidadeTributavel, decimal valorUnitario, decimal quantidade, string? origem, string? csosn, string? cstIcms, decimal valorAliquotaIcms, decimal valorReducaoIcmsPercentual, int tipoReducaoIcms, decimal valorReducaoIcmsPercentualSt, int tipoReducaoIcmsSt, decimal valorBaseCalculoStRetidoOperacaoAnterior, decimal valorAliquotaSt, decimal valorIcmsStRetidoOperacaoAnterior, decimal valorIcmsProprioSubstituto, string cstPisCofins, decimal valorAliquotaPis, decimal valorAliquotaPisReal, decimal valorAliquotaCofins, decimal valorAliquotaCofinsReal, int compoeValorTotal, decimal valorDesconto, decimal valorDescontoRateado, decimal valorFreteRateado, decimal valorSeguroRateado, decimal valorAcrescimoRateado, decimal valorOutroRateado, decimal valorAproximadoIbpt, decimal valorAliquotaMva, decimal valorAliquotaFcp, decimal valorAliquotaFcpSt, decimal valorAliquotaIcmsInterna, decimal valorAliquotaIcmsInterestadual, string? enquadramentoIpi, string? cstIpi, decimal valorAliquotaIpi, decimal valorReducaoIpiPercentual, int tipoReducaoIpi, string? cest, bool ipiEmbutido, bool difalTipoCalculoPorDentro, int tipoCalculoBaseIcmsSt, decimal valorUnitFixadoIcmsSt, string? cstIbsCbs, string? cClassTrib, decimal qtdeBCMonoRetido, decimal valorAliquotaAdRemRetido, string? informacoesAdProduto, int? numeroItemPedidoCompra, string? numeroPedidoCompra, string? fichaConteudoImportacao, string? codigoBeneficioFiscal, decimal valorCusto, VendaItemCombustivelDto? combustivel, VendaItemImposto imposto)
        {
            ProdutoId = produtoId;
            CodigoProduto = codigoProduto;
            NomeProduto = nomeProduto;
            CodigoBarras = codigoBarras;
            Ncm = ncm;
            TributacaoCfop = tributacaoCfop;
            Unidade = unidade;
            UnidadeTributavel = unidadeTributavel;
            ValorUnitario = valorUnitario;
            Quantidade = quantidade;
            Origem = origem;
            Csosn = csosn;
            CstIcms = cstIcms;
            ValorAliquotaIcms = valorAliquotaIcms;
            ValorReducaoIcmsPercentual = valorReducaoIcmsPercentual;
            TipoReducaoIcms = tipoReducaoIcms;
            ValorReducaoIcmsPercentualSt = valorReducaoIcmsPercentualSt;
            TipoReducaoIcmsSt = tipoReducaoIcmsSt;
            ValorBaseCalculoStRetidoOperacaoAnterior = valorBaseCalculoStRetidoOperacaoAnterior;
            ValorAliquotaSt = valorAliquotaSt;
            ValorIcmsStRetidoOperacaoAnterior = valorIcmsStRetidoOperacaoAnterior;
            ValorIcmsProprioSubstituto = valorIcmsProprioSubstituto;
            CstPisCofins = cstPisCofins;
            ValorAliquotaPis = valorAliquotaPis;
            ValorAliquotaPisReal = valorAliquotaPisReal;
            ValorAliquotaCofins = valorAliquotaCofins;
            ValorAliquotaCofinsReal = valorAliquotaCofinsReal;
            CompoeValorTotal = compoeValorTotal;
            ValorDesconto = valorDesconto;
            ValorDescontoRateado = valorDescontoRateado;
            ValorFreteRateado = valorFreteRateado;
            ValorSeguroRateado = valorSeguroRateado;
            ValorAcrescimoRateado = valorAcrescimoRateado;
            ValorOutroRateado = valorOutroRateado;
            ValorAproximadoIbpt = valorAproximadoIbpt;
            ValorAliquotaMva = valorAliquotaMva;
            ValorAliquotaFcp = valorAliquotaFcp;
            ValorAliquotaFcpSt = valorAliquotaFcpSt;
            ValorAliquotaIcmsInterna = valorAliquotaIcmsInterna;
            ValorAliquotaIcmsInterestadual = valorAliquotaIcmsInterestadual;
            EnquadramentoIpi = enquadramentoIpi;
            CstIpi = cstIpi;
            ValorAliquotaIpi = valorAliquotaIpi;
            ValorReducaoIpiPercentual = valorReducaoIpiPercentual;
            TipoReducaoIpi = tipoReducaoIpi;
            Cest = cest;
            IpiEmbutido = ipiEmbutido;
            DifalTipoCalculoPorDentro = difalTipoCalculoPorDentro;
            TipoCalculoBaseIcmsSt = tipoCalculoBaseIcmsSt;
            ValorUnitFixadoIcmsSt = valorUnitFixadoIcmsSt;
            CstIbsCbs = cstIbsCbs;
            CClassTrib = cClassTrib;

            QtdeBCMonoRetido = qtdeBCMonoRetido;
            ValorAliquotaAdRemRetido = valorAliquotaAdRemRetido;
            InformacoesAdProduto = informacoesAdProduto;
            NumeroItemPedidoCompra = numeroItemPedidoCompra;
            NumeroPedidoCompra = numeroPedidoCompra;
            FichaConteudoImportacao = fichaConteudoImportacao;
            CodigoBeneficioFiscal = codigoBeneficioFiscal;
            ValorCusto = valorCusto;
            Combustivel = combustivel;
            Imposto = imposto;
        }

        public long ProdutoId { get; set; }
        public string? CodigoProduto { get; set; }
        public string? NomeProduto { get; set; }
        public string? CodigoBarras { get; set; }
        public string? Ncm { get; set; }
        public VendaItemTributacaoCfop TributacaoCfop { get; set; } = null!;
        public string? Unidade { get; set; }
        public string UnidadeTributavel { get; set; } = null!;
        public decimal ValorUnitario { get; set; }
        public decimal Quantidade { get; set; }
        public string? Origem { get; set; }
        public string? Csosn { get; set; }
        public string? CstIcms { get; set; }
        public decimal ValorAliquotaIcms { get; set; }
        public decimal ValorReducaoIcmsPercentual { get; set; }
        public int TipoReducaoIcms { get; set; }
        public decimal ValorReducaoIcmsPercentualSt { get; set; }
        public int TipoReducaoIcmsSt { get; set; }
        public decimal ValorBaseCalculoStRetidoOperacaoAnterior { get; set; }
        public decimal ValorAliquotaSt { get; set; }
        public decimal ValorIcmsStRetidoOperacaoAnterior { get; set; }
        public decimal ValorIcmsProprioSubstituto { get; set; }
        public string CstPisCofins { get; set; } = null!;
        public decimal ValorAliquotaPis { get; set; }
        public decimal ValorAliquotaPisReal { get; set; }
        public decimal ValorAliquotaCofins { get; set; }
        public decimal ValorAliquotaCofinsReal { get; set; }
        public int CompoeValorTotal { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorDescontoRateado { get; set; }
        public decimal ValorFreteRateado { get; set; }
        public decimal ValorSeguroRateado { get; set; }
        public decimal ValorAcrescimoRateado { get; set; }
        public decimal ValorOutroRateado { get; set; }
        public decimal ValorAproximadoIbpt { get; set; }
        public decimal ValorAliquotaMva { get; set; }
        public decimal ValorAliquotaFcp { get; set; }
        public decimal ValorAliquotaFcpSt { get; set; }
        public decimal ValorAliquotaIcmsInterna { get; set; }
        public decimal ValorAliquotaIcmsInterestadual { get; set; }
        public string? EnquadramentoIpi { get; set; }
        public string? CstIpi { get; set; }
        public decimal ValorAliquotaIpi { get; set; }
        public decimal ValorReducaoIpiPercentual { get; set; }
        public int TipoReducaoIpi { get; set; }
        public string? Cest { get; set; }
        public bool IpiEmbutido { get; set; }
        public bool DifalTipoCalculoPorDentro { get; set; }
        public int TipoCalculoBaseIcmsSt { get; set; }
        public decimal ValorUnitFixadoIcmsSt { get; set; }
        public string? CstIbsCbs { get; set; }
        public string? CClassTrib { get; set; }
        public decimal QtdeBCMonoRetido { get; set; }
        public decimal ValorAliquotaAdRemRetido { get; set; }
        public string? InformacoesAdProduto { get; set; }
        public int? NumeroItemPedidoCompra { get; set; }
        public string? NumeroPedidoCompra { get; set; }
        public string? FichaConteudoImportacao { get; set; }
        public string? CodigoBeneficioFiscal { get; set; }
        public decimal ValorCusto { get; set; }
        public VendaItemCombustivelDto? Combustivel { get; set; }
        public VendaItemImposto? Imposto { get; set; }
    }
}
