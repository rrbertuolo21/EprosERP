using System.ComponentModel.DataAnnotations;

namespace Epros.ERP.DfeCalculos.Dtos
{
    public class VendaCompleta2TransmitirDto
    {
        public long VendaId { get; set; }
    }

    public class VendaCompleta2CreateDto
    {
        public long Id { get; set; }
        public int ModeloFiscal { get; set; }
        public int ModalidadeFrete { get; set; }
        public string? InformacoesComplementares { get; set; }
        public string? InformacoesAdicionaisFisco { get; set; }
        public int Status { get; set; }
        public string NaturezaOperacao { get; set; } = null!;
        public VendaDocumento? Documento { get; set; }
        public VendaEmitente2CreateDto Emitente { get; set; } = null!;
        public VendaDestinatario2CreateDto? Destinatario { get; set; }
        public VendaTransporte2CreateDto? Transporte { get; set; }
        public VendaCompletaTotal2CreateDto Total { get; set; } = null!;
        public VendaCompletaTotalIbsCbs2CreateDto? TotalIbsCbs { get; set; }
        public VendaFaturaNfce2CreateDto? Fatura { get; set; }
        public ICollection<VendaPagamento2CreateDto> Pagamentos { get; set; } = null!;
        public ICollection<VendaCompletaItem2CreateDto> Itens { get; set; } = null!;
        public VendaCompletaConfiguracao2CreateDto Configuracao { get; set; } = null!;

        public VendaCompletaIntermediadorCreateDto? Intermediador { get; set; }
        public string[]? DocumentosAutorizacaoXml { get; set; }
        public VendaExportacaoDto? Exportacao { get; set; }
    }

    public class VendaDocumento
    {
        public DateTime DataEmissao { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public bool EmbuteFrete { get; set; }
        public bool EmbuteSeguro { get; set; }
        public bool CobraFrete { get; set; }
        public bool EmbuteAcrescimo { get; set; }
        public bool EmbuteOutro { get; set; }
        public string[]? ChavesReferenciadasNFe { get; set; }
    }

    public class VendaEmitente2CreateDto
    {
        public long EmpresaId { get; set; }
    }

    public class VendaDestinatario2CreateDto
    {
        public long? PessoaId { get; set; }
        public string? DocumentoConsumidor { get; set; }
        public bool EnviarDestinatatioNaNfce { get; set; }
        public long EnderecoEntregaId { get; set; }
        public long EnderecoCobrancaId { get; set; }
    }

    public class VendaTransporte2CreateDto
    {
        public VendaTransporteTransportadora2CreateDto? Transportadora { get; set; }
        public VendaTransporteVeiculo2CreateDto? Veiculo { get; set; }
        public ICollection<VendaTransporteReboque2CreateDto>? Reboques { get; set; }
        public ICollection<VendaTransporteVolume2CreateDto>? Volumes { get; set; }
    }

    public class VendaTransporteTransportadora2CreateDto
    {
        public long? PessoaId { get; set; }
        public string? Cnpj { get; set; }
        public string? Cpf { get; set; }
        public string? RazaoSocial { get; set; }
        public string? InscricaoEstadual { get; set; }
        public VendaTransporteTransportadoraEndereco2Dto? Endereco { get; set; }
    }

    public class VendaTransporteTransportadoraEndereco2Dto
    {
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
    }

    public class VendaTransporteVeiculo2CreateDto
    {
        public long? VeiculoId { get; set; }
        public string? Placa { get; set; }
        public string? Uf { get; set; }
        public string? Rntc { get; set; }
    }

    public class VendaTransporteReboque2CreateDto
    {
        public long? VeiculoId { get; set; }
        public string? Placa { get; set; }
        public string? Uf { get; set; }
        public string? Rntc { get; set; }
    }

    public class VendaTransporteVolume2CreateDto
    {
        public int QuantidadeVolumes { get; set; }
        public string? Especie { get; set; }
        public string? NumeroVolumes { get; set; }
        public decimal PesoLiquido { get; set; }
        public decimal PesoBruto { get; set; }
        public string? Marca { get; set; }
    }

    public class VendaCompletaTotal2CreateDto
    {
        public decimal ValorBaseDeCalculoIcms { get; set; }
        public decimal ValorIcms { get; set; }
        public decimal ValorIcmsDesonerado { get; set; }
        public decimal ValorFcp { get; set; }
        public decimal ValorBaseDeCalculoSt { get; set; }
        public decimal ValorSt { get; set; }
        public decimal ValorFcpSt { get; set; }
        public decimal ValorFcpRetido { get; set; }
        public decimal ValorProduto { get; set; }
        public decimal ValorFrete { get; set; }
        public decimal ValorSeguro { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorImpostoImportacao { get; set; }
        public decimal ValorIpi { get; set; }
        public decimal ValorIpiDevolucao { get; set; }
        public decimal ValorPis { get; set; }
        public decimal ValorCofins { get; set; }
        public decimal ValorOutro { get; set; }
        public decimal ValorNotaFiscal { get; set; }
    }

    public class VendaCompletaTotalIbsCbs2CreateDto
    {
        public decimal ValorBaseDeCalculo { get; set; }
        public decimal ValorImpostoDevidoEstadual { get; set; }
        public decimal ValorImpostoDevidoMunicipal { get; set; }
        public decimal ValorImpostoDevidoCbs { get; set; }
    }

    public class VendaFaturaNfce2CreateDto
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public decimal ValorOriginal { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public decimal ValorDesconto { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public decimal ValorLiquido { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public ICollection<VendaFaturaDuplicataNfce2CreateDto> Duplicatas { get; set; } = null!;
    }

    public class VendaFaturaDuplicataNfce2CreateDto
    {
        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        [StringLength(60, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres!")]
        public string NumeroDuplicata { get; set; } = null!;

        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public DateTime DataVencimento { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório!")]
        public decimal ValorDuplicata { get; set; }
    }

    public class VendaPagamento2CreateDto
    {
        public decimal ValorTroco { get; set; }
        public int TipoPagamento { get; set; }
        public decimal ValorPagamento { get; set; }
        //public int CartaoTipoIntegracao { get; set; }
        //public string? CartaoCnpjIntermediadorFinanceira { get; set; }
        //public int CartaoBandeira { get; set; }
        //public string? CartaoCodigoAutorizacaoOperacao { get; set; }
    }

    public class VendaCompletaItemCombustivelOrigem2Dto
    {
        public int IndicadorImportacao { get; set; }
        public string? UfOrigem { get; set; }
        public decimal PercentualOrigem { get; set; }
    }
    public class VendaCompletaItemCombustivel2Dto
    {
        public string? CodigoAnp { get; set; }
        public string? DescricaoAnp { get; set; }
        public decimal QuantidadeCombustivelFaturada { get; set; }
        public string? UfConsumo { get; set; }
        public decimal PercentualGlpDerivadoPetroleo { get; set; }
        public decimal PercentualGasNaturalNacional { get; set; }
        public decimal PercentualGasNaturalImportado { get; set; }
        public decimal ValorPartida { get; set; }

        public ICollection<VendaCompletaItemCombustivelOrigem2Dto> Origens { get; set; } = null!;
    }

    public class VendaCompletaItem2CreateDto
    {
        public long ProdutoId { get; set; }
        public string Ncm { get; set; } = null!;
        public string? ExcecaoNcmTipi { get; set; }
        public long? CestId { get; set; }
        public int Cfop { get; set; }
        public string UnidadeComercial { get; set; } = null!;
        public string? UnidadeTributavel { get; set; }
        public decimal QuantidadeComercial { get; set; }
        public decimal ValorUnitarioComercial { get; set; }
        public decimal QuantidadeTributavel { get; set; }
        public decimal ValorUnitarioTributavel { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorDescontoRateado { get; set; }
        public decimal ValorFreteRateado { get; set; }
        public decimal ValorSeguroRateado { get; set; }
        public decimal ValorOutrasDepesasAcessoriasRateado { get; set; }
        public int CompoeValorTotal { get; set; }
        public string? InformacoesAdicionaisDoProduto { get; set; }
        public bool IntegraFaturamento { get; set; }
        public int? NumeroItemPedidoCompra { get; set; }
        public string? NumeroPedidoCompra { get; set; }
        public string? FichaConteudoImportacao { get; set; }
        public string? CodigoBeneficioFiscal { get; set; }
        public decimal ValorCusto { get; set; }

        //public VendaCompletaItemCombustivel2Dto? Combustivel { get; set; }
        public VendaCompletaItemImposto2CreateDto? Imposto { get; set; } //completa
        public VendaCompletaItemImpostoValorAproximado2CreateDto? ImpostoValorAproximado { get; set; } //completa
        public VendaCompletaItemImpostoIbsCbs2CreateDto? ImpostoIbsCbs { get; set; } //completa
    }

    public class VendaCompletaItemImposto2CreateDto
    {
        //public VendaCompletaItemImpostoCreateDto() { }
        public VendaCompletaItemImposto2CreateDto(long vendaItemId, string origem, string? cstIcms, string? csosn, int modalidadeDeterminacaoBaseCalculoIcms, decimal valorBaseDeCalculoIcms, decimal percentualReducaoBaseDeCalculoIcms, decimal aliquotaIcms, decimal valorImpostoIcms, int modalidadeBaseDeCalculosST, decimal percentualMvaBaseDeCalculoST, decimal percentualReducaoBaseDeCalculoST, decimal valorBaseDeCalculoSt, decimal aliquotaSt, decimal valorImpostoSt, int motivoDesoneracaoIcms, decimal valorBaseDeCalculoStRetido, decimal valorImpostoStRetido, decimal percentualCreditoSimplesNacionalIcms, decimal valorImpostoCreditoSimplesNacionalIcms, decimal valorBaseDeCalculoFcp, decimal percentualFcp, decimal valorImpostoFcp, decimal valorOperacaoDiferimentoIcms, decimal percentualDiferimentoIcms, decimal valorImpostoDiferimentoIcms, string? cstIpiSaida, decimal valorBaseDeCalculoIpi, decimal aliquotaIpi, decimal valorImpostoDiferimentoIpi, decimal valorQuantidadeTotalParaTributacaoIpi, decimal valorPorUnidadeTributavelIpi, string? cstPis, decimal valorBaseDeCalculoPis, decimal aliquotaPis, decimal valorQuantidadeVendidaProdutoPis, decimal aliquotaPorUnidadeVendidaPis, decimal valorImpostoDiferimentoPis, string? cstCofins, decimal valorBaseDeCalculoCofins, decimal aliquotaCofins, decimal valorQuantidadeVendidaProdutoCofins, decimal aliquotaPorUnidadeVendidaCofins, decimal valorImpostoDiferimentoCofins, decimal valorBaseDeCalculoFcpSt, decimal percentualFcpSt, decimal valorImpostoFcpSt, decimal valorAliquotaIcmsInterna, decimal valorAliquotaIcmsInterestadual, int enquadramentoIpi, decimal valorReducaoIpiPercentual, bool ipiEmbutido, bool difalTipoCalculoPorDentro, int tipoReducaoIpi, int tipoCalculoBaseIcmsSt, decimal valorUnitFixadoIcmsSt, decimal valorBaseDeCalculoDifal, decimal valorImpostoDevidoDifal, decimal valorImpostoDevidoRecolherSt, decimal valorImpostoDevidoFcp, decimal valorIcmsIsento, decimal valorIcmsOutros, string? icmsObservacao, decimal valorIpiIsento, decimal valorIpiOutros, string? ipiObservacao)
        {
            VendaItemId = vendaItemId;
            Origem = origem;
            CstIcms = cstIcms;
            Csosn = csosn;
            ModalidadeDeterminacaoBaseCalculoIcms = modalidadeDeterminacaoBaseCalculoIcms;
            ValorBaseDeCalculoIcms = valorBaseDeCalculoIcms;
            PercentualReducaoBaseDeCalculoIcms = percentualReducaoBaseDeCalculoIcms;
            AliquotaIcms = aliquotaIcms;
            ValorImpostoIcms = valorImpostoIcms;
            ModalidadeBaseDeCalculosST = modalidadeBaseDeCalculosST;
            PercentualMvaBaseDeCalculoST = percentualMvaBaseDeCalculoST;
            PercentualReducaoBaseDeCalculoST = percentualReducaoBaseDeCalculoST;
            ValorBaseDeCalculoSt = valorBaseDeCalculoSt;
            AliquotaSt = aliquotaSt;
            ValorImpostoSt = valorImpostoSt;
            MotivoDesoneracaoIcms = motivoDesoneracaoIcms;
            ValorBaseDeCalculoStRetido = valorBaseDeCalculoStRetido;
            ValorImpostoStRetido = valorImpostoStRetido;
            PercentualCreditoSimplesNacionalIcms = percentualCreditoSimplesNacionalIcms;
            ValorImpostoCreditoSimplesNacionalIcms = valorImpostoCreditoSimplesNacionalIcms;
            ValorBaseDeCalculoFcp = valorBaseDeCalculoFcp;
            PercentualFcp = percentualFcp;
            ValorImpostoFcp = valorImpostoFcp;
            ValorOperacaoDiferimentoIcms = valorOperacaoDiferimentoIcms;
            PercentualDiferimentoIcms = percentualDiferimentoIcms;
            ValorImpostoDiferimentoIcms = valorImpostoDiferimentoIcms;
            CstIpiSaida = cstIpiSaida;
            ValorBaseDeCalculoIpi = valorBaseDeCalculoIpi;
            AliquotaIpi = aliquotaIpi;
            ValorImpostoDiferimentoIpi = valorImpostoDiferimentoIpi;
            ValorQuantidadeTotalParaTributacaoIpi = valorQuantidadeTotalParaTributacaoIpi;
            ValorPorUnidadeTributavelIpi = valorPorUnidadeTributavelIpi;
            CstPis = cstPis;
            ValorBaseDeCalculoPis = valorBaseDeCalculoPis;
            AliquotaPis = aliquotaPis;
            ValorQuantidadeVendidaProdutoPis = valorQuantidadeVendidaProdutoPis;
            AliquotaPorUnidadeVendidaPis = aliquotaPorUnidadeVendidaPis;
            ValorImpostoDiferimentoPis = valorImpostoDiferimentoPis;
            CstCofins = cstCofins;
            ValorBaseDeCalculoCofins = valorBaseDeCalculoCofins;
            AliquotaCofins = aliquotaCofins;
            ValorQuantidadeVendidaProdutoCofins = valorQuantidadeVendidaProdutoCofins;
            AliquotaPorUnidadeVendidaCofins = aliquotaPorUnidadeVendidaCofins;
            ValorImpostoDiferimentoCofins = valorImpostoDiferimentoCofins;
            ValorBaseDeCalculoFcpSt = valorBaseDeCalculoFcpSt;
            PercentualFcpSt = percentualFcpSt;
            ValorImpostoFcpSt = valorImpostoFcpSt;
            ValorAliquotaIcmsInterna = valorAliquotaIcmsInterna;
            ValorAliquotaIcmsInterestadual = valorAliquotaIcmsInterestadual;
            EnquadramentoIpi = enquadramentoIpi;
            ValorReducaoIpiPercentual = valorReducaoIpiPercentual;
            IpiEmbutido = ipiEmbutido;
            DifalTipoCalculoPorDentro = difalTipoCalculoPorDentro;
            TipoReducaoIpi = tipoReducaoIpi;
            TipoCalculoBaseIcmsSt = tipoCalculoBaseIcmsSt;
            ValorUnitFixadoIcmsSt = valorUnitFixadoIcmsSt;
            ValorBaseDeCalculoDifal = valorBaseDeCalculoDifal;
            ValorImpostoDevidoDifal = valorImpostoDevidoDifal;
            ValorImpostoDevidoRecolherSt = valorImpostoDevidoRecolherSt;
            ValorImpostoDevidoFcp = valorImpostoDevidoFcp;
            ValorIcmsIsento = valorIcmsIsento;
            ValorIcmsOutros = valorIcmsOutros;
            IcmsObservacao = icmsObservacao;
            ValorIpiIsento = valorIpiIsento;
            ValorIpiOutros = valorIpiOutros;
            IpiObservacao = ipiObservacao;
        }

        public long VendaItemId { get; set; }
        public string Origem { get; set; }
        public string? CstIcms { get; set; }
        public string? Csosn { get; set; }
        public int ModalidadeDeterminacaoBaseCalculoIcms { get; set; }
        public decimal ValorBaseDeCalculoIcms { get; set; }
        public decimal PercentualReducaoBaseDeCalculoIcms { get; set; }
        public decimal AliquotaIcms { get; set; }
        public decimal ValorImpostoIcms { get; set; }
        public int ModalidadeBaseDeCalculosST { get; set; }
        public decimal PercentualMvaBaseDeCalculoST { get; set; }
        public decimal PercentualReducaoBaseDeCalculoST { get; set; }
        public decimal ValorBaseDeCalculoSt { get; set; }
        public decimal AliquotaSt { get; set; }
        public decimal ValorImpostoSt { get; set; }
        public int MotivoDesoneracaoIcms { get; set; }
        public decimal ValorBaseDeCalculoStRetido { get; set; }
        public decimal ValorImpostoStRetido { get; set; }
        public decimal PercentualCreditoSimplesNacionalIcms { get; set; }
        public decimal ValorImpostoCreditoSimplesNacionalIcms { get; set; }
        public decimal ValorBaseDeCalculoFcp { get; set; }
        public decimal PercentualFcp { get; set; }
        public decimal ValorImpostoFcp { get; set; }
        public decimal ValorOperacaoDiferimentoIcms { get; set; }
        public decimal PercentualDiferimentoIcms { get; set; }
        public decimal ValorImpostoDiferimentoIcms { get; set; }
        public string? CstIpiSaida { get; set; }
        public decimal ValorBaseDeCalculoIpi { get; set; }
        public decimal AliquotaIpi { get; set; }
        public decimal ValorImpostoDiferimentoIpi { get; set; }
        public decimal ValorQuantidadeTotalParaTributacaoIpi { get; set; }
        public decimal ValorPorUnidadeTributavelIpi { get; set; }
        public string? CstPis { get; set; }
        public decimal ValorBaseDeCalculoPis { get; set; }
        public decimal AliquotaPis { get; set; }
        public decimal ValorQuantidadeVendidaProdutoPis { get; set; }
        public decimal AliquotaPorUnidadeVendidaPis { get; set; }
        public decimal ValorImpostoDiferimentoPis { get; set; }
        public string? CstCofins { get; set; }
        public decimal ValorBaseDeCalculoCofins { get; set; }
        public decimal AliquotaCofins { get; set; }
        public decimal ValorQuantidadeVendidaProdutoCofins { get; set; }
        public decimal AliquotaPorUnidadeVendidaCofins { get; set; }
        public decimal ValorImpostoDiferimentoCofins { get; set; }
        public decimal ValorBaseDeCalculoFcpSt { get; set; }
        public decimal PercentualFcpSt { get; set; }
        public decimal ValorImpostoFcpSt { get; set; }
        public decimal ValorAliquotaIcmsInterna { get; set; }
        public decimal ValorAliquotaIcmsInterestadual { get; set; }
        public int EnquadramentoIpi { get; set; }
        public decimal ValorReducaoIpiPercentual { get; set; }
        public bool IpiEmbutido { get; set; }
        public bool DifalTipoCalculoPorDentro { get; set; }
        public int TipoReducaoIpi { get; set; }
        public int TipoCalculoBaseIcmsSt { get; set; }
        public decimal ValorUnitFixadoIcmsSt { get; set; }
        public decimal ValorBaseDeCalculoDifal { get; set; }
        public decimal ValorImpostoDevidoDifal { get; set; }
        public decimal ValorImpostoDevidoRecolherSt { get; set; }
        public decimal ValorImpostoDevidoFcp { get; set; }
        public decimal ValorIcmsIsento { get; set; }
        public decimal ValorIcmsOutros { get; set; }
        public string? IcmsObservacao { get; set; }
        public decimal ValorIpiIsento { get; set; }
        public decimal ValorIpiOutros { get; set; }
        public string? IpiObservacao { get; set; }
    }

    public class VendaCompletaItemImpostoValorAproximado2CreateDto
    {
        public long VendaItemId { get; set; }
        public decimal AliquotaNacionalFederal { get; set; }
        public decimal AliquotaImportadoFederal { get; set; }
        public decimal AliquotaEstadual { get; set; }
        public decimal AliquotaMunicipal { get; set; }
        public string? Versao { get; set; }
        public string? Fonte { get; set; }
    }

    public class VendaCompletaItemImpostoIbsCbs2CreateDto
    {
        public string Cst { get; set; } = null!;
        public string CClassTrib { get; set; } = string.Empty;

        public decimal AliquotaEstadual { get; set; }
        public decimal AliquotaMunicipal { get; set; }
        public decimal AliquotaCbs { get; set; }

        public decimal AliquotaEstadualReducao { get; set; }
        public decimal AliquotaMunicipalReducao { get; set; }
        public decimal AliquotaCbsReducao { get; set; }

        public decimal AliquotaEstadualDiferimento { get; set; }
        public decimal AliquotaMunicipalDiferimento { get; set; }
        public decimal AliquotaCbsDiferimento { get; set; }

        public decimal AliquotaEfetivaEstadual { get; set; }
        public decimal AliquotaEfetivaMunicipal { get; set; }
        public decimal AliquotaEfetivaCbs { get; set; }

        public decimal ValorBaseDeCalculo { get; set; }
        public decimal ValorImpostoDevidoEstadual { get; set; }
        public decimal ValorImpostoDevidoMunicipal { get; set; }
        public decimal ValorImpostoDevidoCbs { get; set; }

        public VendaCompletaItemImpostoIbsCbsTributacaoRegular2CreateDto? TributacaoRegular { get; set; }
    }

    public class VendaCompletaItemImpostoIbsCbsTributacaoRegular2CreateDto
    {        
        public string Cst { get; set; } = null!;
        public string CClassTrib { get; set; } = string.Empty;
        public decimal AliquotaEfetivaIbsEstadual { get; set; }
        public decimal ValorIbsEstadual { get; set; }
        public decimal AliquotaEfetivaIbsMunicipal { get; set; }
        public decimal ValorIbsMunicipal { get; set; }
        public decimal AliquotaEfetivaCbs { get; set; }
        public decimal ValorCbs { get; set; }
    }

    public class VendaCompletaConfiguracao2CreateDto
    {
        public int TipoAtendimento { get; set; }
        public int IndicadorIntermediador { get; set; }
    }

    public class VendaCompletaIntermediadorCreateDto
    {
        public string Documento { get; set; } = null!;
        public string? IdentificadorIntermediador { get; set; }
    }

    public class VendaExportacaoDto
    {
        public string UfSaidaPais { get; set; } = null!;
        public string LocalExportacao { get; set; } = null!;
        public string? LocalDespacho { get; set; }
    }
}
