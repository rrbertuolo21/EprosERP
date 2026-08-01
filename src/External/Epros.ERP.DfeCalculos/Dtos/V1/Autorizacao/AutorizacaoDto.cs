namespace Epros.ERP.DfeCalculos.Dtos.V1.Autorizacao
{
    public class AutorizacaoNfceDto
    {
        public string? LocalizadorExternoId { get; set; }
        public DocumentoNfceDto Documento { get; set; } = null!;
        public string NaturezaOperacao { get; set; } = null!;
        public decimal ValorDesconto { get; set; }
        public decimal ValorAcrescimo { get; set; }
        public decimal ValorSeguro { get; set; }
        public decimal ValorOutro { get; set; }
        public decimal ValorTotal { get; set; }
        public int ModalidadeFrete { get; set; }
        public decimal ValorFrete { get; set; }
        public string? InformacoesComplementares { get; set; }
        public string? InformacoesAdicionaisFisco { get; set; }
        public string[]? DocumentoAutorizacaoXml { get; set; }
        public EmitenteDto Emitente { get; set; } = null!;
        public DestinatarioDto? Destinatario { get; set; }
        public ItenDto[] Itens { get; set; } = null!;
        public CobrancaDto? Cobranca { get; set; }
        public PagamentoDto[] Pagamentos { get; set; } = null!;
        public TransporteNfceDto? Transporte { get; set; }
        //public AutorizacaoXmlDto[]? AutorizacoesXml { get; set; }
    }

    public class AutorizacaoNfeDto
    {
        public string? LocalizadorExternoId { get; set; }
        public DocumentoNfeDto Documento { get; set; } = null!;
        public string NaturezaOperacao { get; set; } = null!;
        public decimal ValorDesconto { get; set; }
        public decimal ValorAcrescimo { get; set; }
        public decimal ValorSeguro { get; set; }
        public decimal ValorOutro { get; set; }
        public decimal ValorTotal { get; set; }
        public int ModalidadeFrete { get; set; }
        public decimal ValorFrete { get; set; }
        public string? InformacoesComplementares { get; set; }
        public string? InformacoesAdicionaisFisco { get; set; }
        public string[]? DocumentoAutorizacaoXml { get; set; }
        public EmitenteDto Emitente { get; set; } = null!;
        public DestinatarioDto Destinatario { get; set; } = null!;
        public ItenDto[] Itens { get; set; } = null!;
        public CobrancaDto? Cobranca { get; set; }
        public PagamentoDto[] Pagamentos { get; set; } = null!;
        public TransporteNfeDto? Transporte { get; set; }
        public AutorizacaoNfeImpostoDto? Imposto { get; set; }
        //public AutorizacaoXmlDto[]? AutorizacoesXml { get; set; }
    }

    //NFe Completa
    public class AutorizacaoNfeCompletaDto
    {
        public string? LocalizadorExternoId { get; set; }
        public DocumentoNfeDto Documento { get; set; } = null!;
        public string NaturezaOperacao { get; set; } = null!;
        public int ModalidadeFrete { get; set; }
        public string? InformacoesComplementares { get; set; }
        public string? InformacoesAdicionaisFisco { get; set; }
        public string[]? DocumentoAutorizacaoXml { get; set; }
        public int IndicadorPresenca { get; set; }
        public int IndicadorIntermediador { get; set; }
        public EmitenteDto Emitente { get; set; } = null!;
        public DestinatarioDto Destinatario { get; set; } = null!;
        public EntregaDto? Entrega { get; set; }
        public ItenCompletoDto[] Itens { get; set; } = null!;
        public CobrancaDto? Cobranca { get; set; }
        public PagamentoDto[] Pagamentos { get; set; } = null!;
        public TransporteNfeDto? Transporte { get; set; }
        public AutorizacaoNfeImpostoDto? Imposto { get; set; }
        public AutorizacaoNfeCompletaTotalDto Total { get; set; } = null!;
        public AutorizacaoNfeCompletaTotalIbsCbsDto? TotalIbsCbs { get; set; }
        public IntermediadorDto? Intermediador { get; set; }
        public ExportacaoDto? Exportacao { get; set; }
    }

    public class SalvarNfeCompletaComXmlDto
    {
        public string? LogoMarca { get; set; }
        public string? LocalizadorExternoId { get; set; }
        public string Xml { get; set; } = string.Empty;
    }

    public class AutorizacaoNfeCompletaTotalDto
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
        public decimal ValorAcrescimo { get; set; }
        public decimal ValorImpostoImportacao { get; set; }
        public decimal ValorIpi { get; set; }
        public decimal ValorIpiDevolucao { get; set; }
        public decimal ValorPis { get; set; }
        public decimal ValorCofins { get; set; }
        public decimal ValorOutro { get; set; }
        public decimal ValorNotaFiscal { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class AutorizacaoNfeCompletaTotalIbsCbsDto
    {
        public decimal ValorBaseDeCalculo { get; set; }
        public decimal ValorImpostoDevidoEstadual { get; set; }
        public decimal ValorImpostoDevidoMunicipal { get; set; }
        public decimal ValorImpostoDevidoCbs { get; set; }

    }
}
