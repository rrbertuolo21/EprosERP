using System.ComponentModel;

namespace Epros.Modules.Vendas.Domain.Enums
{
    // ============================================================================
    // Porte fiel dos enums do legado Epros.ERP.Shared.Enums usados no módulo Vendas.
    // Mantidos aqui (namespace do módulo) pois o projeto Epros.Modules.Vendas só
    // referencia Epros.Shared e Epros.Infrastructure. Valores e descrições idênticos
    // ao legado. Enums que já existem em Epros.Shared.Domain.Enums (EBandeiraCartao,
    // ECodigoSituacaoOperacaoSimplesNacional, ECodigoSituacaoTributariaIcms,
    // ECodigoSituacaoTributariaPisCofins, EEstado, EFinalidadeEmissao,
    // EModalidadeFrete, ETipoAtendimento, ETipoPagamento) NÃO são recriados.
    // ============================================================================

    /// <summary>Porte fiel de EModeloDocumento.</summary>
    public enum EModeloDocumento
    {
        [Description("NF-e")]
        NFe = 55,
        [Description("CT-e")]
        CTe = 57,
        [Description("MDF-e")]
        MDFe = 58,
        [Description("CF-e")]
        CFe = 59,
        [Description("NFC-e")]
        NFCe = 65,
        [Description("CTe-OS")]
        CTeOS = 67,
        [Description("Documento Auxiliar Interno")]
        DAI = 99
    }

    /// <summary>Porte fiel de EVendaStatus.</summary>
    public enum EVendaStatus
    {
        Salvar = 0,
        SalvarTransmitir = 1,
        Transmitido = 2,
        SalvarImprimirMei = 3,
        Errado = 9
    }

    /// <summary>Situação da sessão de caixa (PDV). Tipado (padrão EVendaStatus), substitui a string legada 'Aberto'/'Fechado'.</summary>
    public enum ECaixaStatus
    {
        [Description("Aberto")]
        Aberto = 0,
        [Description("Fechado")]
        Fechado = 1
    }

    /// <summary>Porte fiel de EVendaOrigem.</summary>
    public enum EVendaOrigem
    {
        Nfce = 0,
        NfeSimplificada = 1,
        NfeCompleta = 2
    }

    /// <summary>Porte fiel de ERegimeTributario.</summary>
    public enum ERegimeTributario
    {
        [Description("Não Utiliza")]
        NaoUtiliza = -1,
        [Description("Simples Nacional")]
        SimplesNacional = 1,
        [Description("Simples Nacional – excesso de sublimite de receita bruta")]
        SimplesNacionalExcessoSublimite = 2,
        [Description("Regime Normal")]
        RegimeNormal = 3,
        [Description("Simples Nacional MEI")]
        SimplesNacionalMei = 4
    }

    /// <summary>Porte fiel de ETipoIndicadorIe.</summary>
    public enum ETipoIndicadorIe
    {
        [Description("Contribuinte ICMS")]
        ContribuinteICMS = 1,
        [Description("Contribuinte isento de inscrição")]
        Isento = 2,
        [Description("Não Contribuinte")]
        NaoContribuinte = 9
    }

    /// <summary>Porte fiel de ETipoEndereco.</summary>
    public enum ETipoEndereco
    {
        Principal = 1, // Comercial e Residencial
        [Description("Cobrança")]
        Cobranca,
        Entrega,
        Obra
    }

    /// <summary>Porte fiel de EOrigemMercadoria.</summary>
    public enum EOrigemMercadoria
    {
        [Description("0 - Nacional exceto as indicadas nos códigos 3, 4, 5 e 8")]
        OmNacional = 0,
        [Description("1 - Estrangeira - Importação direta")]
        OmEstrangeiraImportacaoDireta = 1,
        [Description("2 - Estrangeira - Adquirida no mercado interno")]
        OmEstrangeiraAdquiridaBrasil = 2,
        [Description("3 - Nacional, conteudo superior 40% e inferior ou igual a 70%")]
        OmNacionalConteudoImportacaoSuperior40 = 3,
        [Description("4 - Nacional, processos produtivos básicos")]
        OmNacionalProcessosBasicos = 4,
        [Description("5 - Nacional, conteudo inferior 40%")]
        OmNacionalConteudoImportacaoInferiorIgual40 = 5,
        [Description("6 - Estrangeira - Importação direta, com similar nacional, lista CAMEX")]
        OmEstrangeiraImportacaoDiretaSemSimilar = 6,
        [Description("7 - Estrangeira - mercado interno, sem simular,lista CAMEX")]
        OmEstrangeiraAdquiridaBrasilSemSimilar = 7,
        [Description("8 - Nacional, Conteúdo de Importação superior a 70%")]
        OmNacionalConteudoImportacaoSuperior70 = 8
    }

    /// <summary>Porte fiel de ECodigoSituacaoTributariaIpi.</summary>
    public enum ECodigoSituacaoTributariaIpi
    {
        [Description("00 - Entrada com Recuperação de Crédito")]
        Cst00 = 00,
        [Description("01 - Entrada Tributada com Alíquota Zero")]
        Cst01 = 01,
        [Description("02 - Entrada Isenta")]
        Cst02 = 02,
        [Description("03 - Entrada Não Tributada")]
        Cst03 = 03,
        [Description("04 - Entrada Imune")]
        Cst04 = 04,
        [Description("05 - Entrada com Suspensão")]
        Cst05 = 05,
        [Description("49 - Outras Entradas")]
        Cst49 = 49,
        [Description("50 - Saída Tributada")]
        Cst50 = 50,
        [Description("51 - Saída Tributável com Alíquota Zero")]
        Cst51 = 51,
        [Description("52 - Saída Isenta")]
        Cst52 = 52,
        [Description("53 - Saída Não-Tributada")]
        Cst53 = 53,
        [Description("54 - Saída Imune")]
        Cst54 = 54,
        [Description("55 - Saída com Suspensão")]
        Cst55 = 55,
        [Description("99 - Outras Saídas")]
        Cst99 = 99
    }

    /// <summary>Porte fiel de EModalidadeBaseDeCalculosIcms.</summary>
    public enum EModalidadeBaseDeCalculosIcms
    {
        [Description("Margem Valor Agregado (%)")]
        MargemValorAgregado = 0,
        [Description("Pauta (Valor)")]
        PautaValor = 1,
        [Description("Preço Tabelado Máx. (valor)")]
        PrecoTabelado = 2,
        [Description("Valor da operação")]
        ValorOperacao = 3
    }

    /// <summary>Porte fiel de EModalidadeBaseDeCalculosST.</summary>
    public enum EModalidadeBaseDeCalculosST
    {
        [Description("Preço tabelado ou máximo sugerido")]
        PrecoTabelaOuMaximoSugerido = 0,
        [Description("Lista Negativa (valor)")]
        ListaNegativaValor = 1,
        [Description("Lista Positiva (valor)")]
        ListaPositivaValor = 2,
        [Description("Lista Neutra (valor)")]
        ListaNeutraValor = 3,
        [Description("Margem Valor Agregado (%)")]
        MargemValorAgregado = 4,
        [Description(" Pauta (valor)")]
        PautaValor = 5
    }

    /// <summary>Porte fiel de EMotivoDesoneracaoIcms.</summary>
    public enum EMotivoDesoneracaoIcms
    {
        [Description("Táxi")]
        Taxi = 1,
        [Description("Deficiente Físico (Revogado)")]
        DeficienteFisicoRevogado = 2,
        [Description("Produtor Agropecuário")]
        ProdutorAgropecuario = 3,
        [Description("Frotista/Locadora")]
        FrotistaLocadora = 4,
        [Description("Diplomático/Consular")]
        DiplomaticoConsular = 5,
        [Description("Utilitários e Motocicletas da Amazônia Ocidental e Áreas de Livre Comércio")]
        UtilitariosMotosAmazonia = 6,
        [Description("SUFRAMA")]
        Suframa = 7,
        [Description("Venda a Órgãos Públicos")]
        VendaOrgaosPublicos = 8,
        [Description("Outros")]
        Outros = 9,
        [Description("Deficiente Condutor (Convênio ICMS 38/12)")]
        DeficienteCondutor = 10,
        [Description("Deficiente Não Condutor (Convênio ICMS 38/12)")]
        DeficienteNaoCondutor = 11,
        [Description("Órgão de fomento e desenvolvimento agropecuário")]
        OrgaoFomentoDesenvolvimentoAgropecuario = 12,
        [Description("Olimpíadas Rio 2016")]
        OlimpiadasRio2016 = 16,
        [Description("Solicitado pelo Fisco")]
        SolicitadoPeloFisco = 90
    }

    /// <summary>Porte fiel de ETipoReducaoBaseDeCalculo.</summary>
    public enum ETipoReducaoBaseDeCalculo
    {
        [Description("Não Utiliza")]
        NaoUtiliza = 0,
        [Description("Em = (-) subtração da base calculo")]
        Em = 1,
        [Description("Para = (*) multiplicação da base calculo")]
        Para = 2
    }

    /// <summary>Porte fiel de EDeterminacaoBaseIcmsSt.</summary>
    public enum EDeterminacaoBaseIcmsSt
    {
        [Description("Preço tabelado ou máximo  sugerido")]
        DbisPrecoTabelado,
        [Description("Lista Negativa (valor)")]
        DbisListaNegativa,
        [Description("Lista Positiva (valor)")]
        DbisListaPositiva,
        [Description("Lista Neutra (valor)")]
        DbisListaNeutra,
        [Description("Margem Valor Agregado (%)")]
        DbisMargemValorAgregado,
        [Description("Pauta (valor)")]
        DbisPauta,
        [Description("Valor da Operação")]
        DbisValordaOperacao
    }

    /// <summary>Porte fiel de EIndicadorTotalizador.</summary>
    public enum EIndicadorTotalizador
    {
        [Description("Valor do item não compõe o total da NF-e")]
        ValorDoItemNaoCompoeTotalNF = 0,
        [Description("Valor do item compõe o total da NF-e")]
        ValorDoItemCompoeTotalNF = 1
    }

    /// <summary>Porte fiel de EIndicadorPagamento.</summary>
    public enum EIndicadorPagamento
    {
        [Description("Pagamento à vista")]
        PagamentoAVista = 0,
        [Description("Pagamento à prazo")]
        PagamentoAPrazo = 1,
        Outros = 2
    }

    /// <summary>Porte fiel de ETipoIntegracaoPagamentoCArtao.</summary>
    public enum ETipoIntegracaoPagamentoCArtao
    {
        [Description("Não utiliza")]
        NaoUtiliza = -1,
        [Description("Pagamento integrado com o sistema de automação da empresa (Ex.: equipamento TEF, Comércio Eletrônico)")]
        PagamentoIntegradoComSistemaAutomacao = 1,
        [Description(" Pagamento não integrado com o sistema de automação da empresa (Ex.: equipamento POS)")]
        PagamentoNaoIntegradoComSistemaAutomacao = 2
    }

    /// <summary>Porte fiel de ETipoOperacaoNfe.</summary>
    public enum ETipoOperacaoNfe
    {
        Entrada = 0,
        [Description("Saída")]
        Saida = 1
    }

    /// <summary>Porte fiel de ETipoFormatoImpressaoDanfe.</summary>
    public enum ETipoFormatoImpressaoDanfe
    {
        [Description("Sem geração de DANFE")]
        SemGeracaoDanfe = 0,
        [Description("DANFE normal, Retrato")]
        DanfeNormalRetrato = 1,
        [Description("DANFE normal, Paisagem")]
        DanfeNormalPaisagem = 2,
        [Description("DANFE Simplificado")]
        DanfeSimplificado = 3,
        [Description("DANFE NFC-e")]
        DanfeNfce = 4,
        [Description("DANFE NFC-e em mensagem eletrônica (o envio de mensagem eletrônica pode ser feita de forma simultânea com a impressão do DANFE; usar o tpImp - 5 quando esta for a única forma de disponibilização do DANFE).")]
        DanfeNfceEmMensagemEletronica = 5
    }

    /// <summary>Porte fiel de ETipoEmissao.</summary>
    public enum ETipoEmissao
    {
        [Description("Normal")]
        teNormal = 1,
        [Description("Contingência FS-IA")]
        teFSIA = 2,
        [Description("Contingência SCAN")]
        teSCAN = 3,
        [Description("Contingência DPEC")]
        teEPEC = 4,
        [Description("Contingência FS-DA")]
        teFSDA = 5,
        [Description("Contingência SVC-AN")]
        teSVCAN = 6,
        [Description("Contingência SVC-RS")]
        teSVCRS = 7,
        [Description("Contingência off-line")]
        teOffLine = 9
    }

    /// <summary>Porte fiel de ETipoAmbiente.</summary>
    public enum ETipoAmbiente
    {
        [Description("Produção")]
        Producao = 1,
        [Description("Homologação")]
        Homologacao = 2
    }

    /// <summary>Porte fiel de EIndicadorFinalidadeOperacao.</summary>
    public enum EIndicadorFinalidadeOperacao
    {
        [Description("Operação normal")]
        OperacaoSemConsumidorFinal = 0,
        [Description("Operação com Consumidor Final")]
        OperacaoComConsumidorFinal = 1
    }

    /// <summary>Porte fiel de EIndicadorIntermediadorMarketplace.</summary>
    public enum EIndicadorIntermediadorMarketplace
    {
        [Description("Operação sem intermediador (em site ou plataforma própria")]
        OperacaoSemIntermediador = 0,
        [Description("Operação em site ou plataforma de terceiros(intermediadores/marketplace)")]
        OperacaoEmSiteOuPlataformaTerceiro = 1
    }

    /// <summary>Porte fiel de EDocumentoFiscalStatus.</summary>
    public enum EDocumentoFiscalStatus
    {
        Recebido = 0,
        Autorizado = 1,
        Rejeitado = 2,
        Cancelado = 3
    }
}
