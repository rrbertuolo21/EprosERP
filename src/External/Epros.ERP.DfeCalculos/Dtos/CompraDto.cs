using Epros.ERP.Shared.Enums;
using Epros.ERP.Shared.ValueObjects.Documentos;

namespace Epros.ERP.DfeCalculos.Dtos
{
    public class CompraDto
    {
        public int ModeloFiscal { get; set; }
        public int ModalidadeFrete { get; set; }
        public string? InformacoesComplementares { get; set; }
        public string? InformacoesAdicionaisFisco { get; set; }
        public int Status { get; set; }
        public string NaturezaOperacao { get; set; } = null!;
        public CompraDocumentoDto? Documento { get; set; }
        public CompraEmitenteDto Emitente { get; set; } = null!;
        public CompraDestinatarioDto? Destinatario { get; set; }
        public CompraTransporteDto? Transporte { get; set; }
        public CompraTotalDto Total { get; set; } = null!;
        public CompraTotalIbsCbsDto? TotalIbsCbs { get; set; }
        public CompraFaturaDto? Fatura { get; set; }
        public ICollection<CompraPagamentoDto> Pagamentos { get; set; } = null!;
        public ICollection<CompraItemDto> Itens { get; set; } = null!;
        public CompraConfiguracaoDto Configuracao { get; set; } = null!;
        public CompraIntermediadorDto? Intermediador { get; set; }
        public string[]? DocumentosAutorizacaoXml { get; set; }
    }

    public class CompraDocumentoDto
    {
        public string? ChaveAcesso { get; set; }
        public int? Serie { get; set; }
        public long? Numero { get; set; }
        public int SituacaoFiscal { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime? DataHoraSaida { get; set; }
        public string[]? ChavesReferenciadasNFe { get; set; }
    }

    public class CompraEmitenteDto
    {
        public long? EmpresaId { get; set; }
        public long? PessoaId { get; set; } //quando for fornecedor (Nfe entrada)
    }

    public class CompraDestinatarioDto
    {
        public long? PessoaId { get; set; }
        public string? DocumentoConsumidor { get; set; }
        //public bool EnviarDestinatatioNaNfce { get; set; }
        public long EnderecoEntregaId { get; set; }
        public long EnderecoCobrancaId { get; set; }
    }

    public class CompraTransporteDto
    {
        public CompraTransporteTransportadoraDto? Transportadora { get; set; }
        public CompraTransporteVeiculoDto? Veiculo { get; set; }
        public ICollection<CompraTransporteReboqueDto>? Reboques { get; set; }
        public ICollection<CompraTransporteVolumeDto>? Volumes { get; set; }
    }

    public class CompraTransporteTransportadoraDto
    {
        public long? PessoaId { get; set; }
        public string? Cnpj { get; set; }
        public string? Cpf { get; set; }
        public string? RazaoSocial { get; set; }
        public string? InscricaoEstadual { get; set; }
        public CompraTransporteTransportadoraEnderecoDto? Endereco { get; set; }
    }

    public class CompraTransporteTransportadoraEnderecoDto
    {
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
    }

    public class CompraTransporteVeiculoDto
    {
        public long? VeiculoId { get; set; }
        public string? Placa { get; set; }
        public string? Uf { get; set; }
        public string? Rntc { get; set; }
    }

    public class CompraTransporteReboqueDto
    {
        public long? VeiculoId { get; set; }
        public string? Placa { get; set; }
        public string? Uf { get; set; }
        public string? Rntc { get; set; }
    }

    public class CompraTransporteVolumeDto
    {
        public int QuantidadeVolumes { get; set; }
        public string? Especie { get; set; }
        public string? NumeroVolumes { get; set; }
        public decimal PesoLiquido { get; set; }
        public decimal PesoBruto { get; set; }
        public string? Marca { get; set; }
    }

    public class CompraTotalDto
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
        public decimal ValorImpostoImportacao { get; set; } //Revisar
        public decimal ValorIpi { get; set; }
        public decimal ValorIpiDevolucao { get; set; }
        public decimal ValorPis { get; set; }
        public decimal ValorCofins { get; set; }
        public decimal ValorOutro { get; set; }
        public decimal ValorNotaFiscal { get; set; }
    }

    public class CompraTotalIbsCbsDto
    {
        public decimal ValorBaseDeCalculo { get; set; }
        public decimal ValorImpostoDevidoEstadual { get; set; }
        public decimal ValorImpostoDevidoMunicipal { get; set; }
        public decimal ValorImpostoDevidoCbs { get; set; }
    }

    public class CompraFaturaDto
    {
        public decimal ValorOriginal { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorLiquido { get; set; }
        public ICollection<CompraFaturaDuplicataDto> Duplicatas { get; set; } = null!;
    }

    public class CompraFaturaDuplicataDto
    {
        public string NumeroDuplicata { get; set; } = null!;
        public DateTime DataVencimento { get; set; }
        public decimal ValorDuplicata { get; set; }
    }

    public class CompraPagamentoDto
    {
        public decimal ValorTroco { get; set; }
        public int TipoPagamento { get; set; }
        public decimal ValorPagamento { get; set; }
    }

    public class CompraItemDto
    {
        public long ProdutoId { get; set; }
        public string? ComplementoDescricao { get; set; }
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
        public string? FichaConteudoImportacao { get; set; } //Revisar
        public CompraItemImpostoDto? Imposto { get; set; }
        public CompraItemImpostoValorAproximadoDto? ImpostoValorAproximado { get; set; }
        public CompraItemImpostoIbsCbsDto? ImpostoIbsCbs { get; set; }
        public string? CodigoBeneficioFiscal { get; set; }
        public ICollection<CompraItemImportacaoDto> Importacoes { get; set; } = null!;
    }


    public class CompraItemImpostoDto
    {
        public CompraItemImpostoDto(long compraItemId, string origem, string? cstIcms, string? csosn, int modalidadeDeterminacaoBaseCalculoIcms, decimal valorBaseDeCalculoIcms, decimal percentualReducaoBaseDeCalculoIcms, decimal aliquotaIcms, decimal valorImpostoIcms, int modalidadeBaseDeCalculosST, decimal percentualMvaBaseDeCalculoST, decimal percentualReducaoBaseDeCalculoST, decimal valorBaseDeCalculoSt, decimal aliquotaSt, decimal valorImpostoSt, int motivoDesoneracaoIcms, decimal valorBaseDeCalculoStRetido, decimal valorImpostoStRetido, decimal percentualCreditoSimplesNacionalIcms, decimal valorImpostoCreditoSimplesNacionalIcms, decimal valorBaseDeCalculoFcp, decimal percentualFcp, decimal valorImpostoFcp, decimal valorOperacaoDiferimentoIcms, decimal percentualDiferimentoIcms, decimal valorImpostoDiferimentoIcms, string? cstIpiSaida, decimal valorBaseDeCalculoIpi, decimal aliquotaIpi, decimal valorImpostoDiferimentoIpi, decimal valorQuantidadeTotalParaTributacaoIpi, decimal valorPorUnidadeTributavelIpi, string? cstPis, decimal valorBaseDeCalculoPis, decimal aliquotaPis, decimal valorQuantidadeVendidaProdutoPis, decimal aliquotaPorUnidadeVendidaPis, decimal valorImpostoDiferimentoPis, string? cstCofins, decimal valorBaseDeCalculoCofins, decimal aliquotaCofins, decimal valorQuantidadeVendidaProdutoCofins, decimal aliquotaPorUnidadeVendidaCofins, decimal valorImpostoDiferimentoCofins, decimal valorBaseDeCalculoFcpSt, decimal percentualFcpSt, decimal valorImpostoFcpSt, decimal valorAliquotaIcmsInterna, decimal valorAliquotaIcmsInterestadual, int enquadramentoIpi, decimal valorReducaoIpiPercentual, bool ipiEmbutido, bool difalTipoCalculoPorDentro, int tipoReducaoIpi, int tipoCalculoBaseIcmsSt, decimal valorUnitFixadoIcmsSt, decimal valorBaseDeCalculoDifal, decimal valorImpostoDevidoDifal, decimal valorImpostoDevidoRecolherSt, decimal valorImpostoDevidoFcp, decimal valorIcmsIsento, decimal valorIcmsOutros, string? icmsObservacao, decimal valorIpiIsento, decimal valorIpiOutros, string? ipiObservacao)
        {
            CompraItemId = compraItemId;
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

        public long CompraItemId { get; set; }
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

    public class CompraItemImpostoValorAproximadoDto
    {
        public long CompraItemId { get; set; }
        public decimal AliquotaNacionalFederal { get; set; }
        public decimal AliquotaImportadoFederal { get; set; }
        public decimal AliquotaEstadual { get; set; }
        public decimal AliquotaMunicipal { get; set; }
        public string? Versao { get; set; }
        public string? Fonte { get; set; }
    }

    public class CompraItemImpostoIbsCbsDto
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
    }

    public class CompraItemImportacaoDto
    {
        public string NumeroDeclaracaoImportacao { get; set; } = null!;
        public DateTime DataDeclaracaoImportacao { get; set; }
        public string LocalDesembaraco { get; set; } = null!;
        public string UfDesembaraco { get; set; } = null!;
        public DateTime DataDesembaraco { get; set; }
        public int TipoViaTransporte { get; set; }
        public decimal ValorAFRMM { get; set; }
        public int TipoIntermedio { get; set; }
        public string? Cnpj { get; set; }
        public string? Cpf { get; set; }
        public string? UfTerceiro { get; set; }
        public string CodigoExportador { get; set; } = null!;
        public ICollection<CompraItemImportacaoAdicaoDto> Adicoes { get; set; } = null!;
    }

    public class CompraItemImportacaoAdicaoDto
    {
        public int NumeroAdicao { get; set; }
        public int NumeroSequencialAdicao { get; set; }
        public string CodigoFabricante { get; set; } = null!;
        public decimal ValorDesconto { get; set; }
        public string? NumeroAtoConcessorio { get; set; }
    }

    public class CompraConfiguracaoDto
    {
        public int TipoAtendimento { get; set; }
        public int IndicadorIntermediador { get; set; }
        public int FinalidadeEmissao { get; set; }
    }

    public class CompraIntermediadorDto
    {
        public string Documento { get; set; } = null!;
        public string? IdentificadorIntermediador { get; set; }
    }

    public record CompraCancelamentoDto(long compraId, string motivo = "");

    public record CompraCartaCorrecaoCreateDto(long compraId, string textoCorrecao);
}
