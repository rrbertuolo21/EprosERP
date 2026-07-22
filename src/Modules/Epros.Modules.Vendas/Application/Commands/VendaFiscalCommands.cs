using System;
using System.Collections.Generic;
using Epros.Modules.Vendas.Domain.Enums;
using Epros.Shared.Application.Contracts;
using Epros.Shared.Domain.Enums;

namespace Epros.Modules.Vendas.Application.Commands
{
    // ============================================================================
    // Commands do agregado fiscal Venda (porte dos métodos de negócio do legado).
    // Cada Command tem um Handler correspondente (CQRS/MediatR).
    // ============================================================================

    // ---------- Venda (cabeçalho fiscal) ----------

    /// <summary>Cria uma venda fiscal (cabeçalho). Porte do ctor de Venda do legado.</summary>
    public record CriarVendaFiscalCommand(
        string CaixaId,
        EModeloDocumento ModeloFiscal,
        string NaturezaOperacao,
        DateTime DataVenda,
        string? InformacoesComplementares,
        string? InformacoesAdicionaisFisco,
        string Status,
        EModalidadeFrete ModalidadeFrete,
        EVendaOrigem VendaOrigem,
        bool IncluirFreteNoTotal,
        Guid? ClienteId,
        decimal Total,
        decimal ValorDesconto,
        decimal ValorFrete,
        string? FormaPagamento
    ) : ICommand;

    /// <summary>Atualiza o cabeçalho de uma venda. Porte de Venda.Alterar.</summary>
    public record AtualizarVendaFiscalCommand(
        Guid Id,
        EModeloDocumento ModeloFiscal,
        string NaturezaOperacao,
        DateTime DataVenda,
        string? InformacoesComplementares,
        string? InformacoesAdicionaisFisco,
        EModalidadeFrete ModalidadeFrete,
        decimal ValorDesconto,
        decimal ValorFrete
    ) : ICommand;

    /// <summary>Exclui (soft delete em cascata) uma venda. Porte de Venda.Deletar.</summary>
    public record DeletarVendaFiscalCommand(Guid Id) : ICommand;

    // ---------- Itens ----------

    public record ItemImpostoInput(
        EOrigemMercadoria Origem,
        ECodigoSituacaoTributariaIcms CstIcms,
        ECodigoSituacaoOperacaoSimplesNacional Csosn,
        EModalidadeBaseDeCalculosIcms ModalidadeDeterminacaoBaseCalculoIcms,
        decimal ValorBaseDeCalculoIcms,
        decimal PercentualReducaoBaseDeCalculoIcms,
        decimal AliquotaIcms,
        decimal ValorImpostoIcms,
        EModalidadeBaseDeCalculosST ModalidadeBaseDeCalculosST,
        decimal PercentualMvaBaseDeCalculoST,
        decimal PercentualReducaoBaseDeCalculoST,
        decimal ValorBaseDeCalculoSt,
        decimal AliquotaSt,
        decimal ValorImpostoSt,
        EMotivoDesoneracaoIcms MotivoDesoneracaoIcms,
        decimal ValorBaseDeCalculoStRetido,
        decimal ValorImpostoStRetido,
        decimal PercentualCreditoSimplesNacionalIcms,
        decimal ValorImpostoCreditoSimplesNacionalIcms,
        decimal ValorBaseDeCalculoFcp,
        decimal PercentualFcp,
        decimal ValorImpostoFcp,
        decimal ValorOperacaoDiferimentoIcms,
        decimal PercentualDiferimentoIcms,
        decimal ValorImpostoDiferimentoIcms,
        ECodigoSituacaoTributariaIpi CstIpiSaida,
        decimal ValorBaseDeCalculoIpi,
        decimal AliquotaIpi,
        decimal ValorImpostoDiferimentoIpi,
        decimal ValorQuantidadeTotalParaTributacaoIpi,
        decimal ValorPorUnidadeTributavelIpi,
        ECodigoSituacaoTributariaPisCofins CstPis,
        decimal ValorBaseDeCalculoPis,
        decimal AliquotaPis,
        decimal ValorQuantidadeVendidaProdutoPis,
        decimal AliquotaPorUnidadeVendidaPis,
        decimal ValorImpostoDiferimentoPis,
        ECodigoSituacaoTributariaPisCofins CstCofins,
        decimal ValorBaseDeCalculoCofins,
        decimal AliquotaCofins,
        decimal ValorQuantidadeVendidaProdutoCofins,
        decimal AliquotaPorUnidadeVendidaCofins,
        decimal ValorImpostoDiferimentoCofins,
        ETipoReducaoBaseDeCalculo TipoReducaoIcms,
        ETipoReducaoBaseDeCalculo TipoReducaoIcmsSt,
        decimal ValorBaseDeCalculoFcpSt,
        decimal PercentualFcpSt,
        decimal ValorImpostoFcpSt,
        decimal ValorIcmsProprioSubistituto,
        decimal ValorAliquotaIcmsInterna,
        decimal ValorAliquotaIcmsInternaEstadual,
        int EnquadramentoIpi,
        decimal ValorReducaoIpiPercentual,
        bool IpiEmbutido,
        bool DifalTipoCalculoPorDentro,
        ETipoReducaoBaseDeCalculo TipoReducaoIpi,
        EDeterminacaoBaseIcmsSt TipoCalculoBaseIcmsSt,
        decimal ValorUnitFixadoIcmsSt,
        decimal ValorBaseDeCalculoDifal,
        decimal ValorImpostoDevidoDifal,
        decimal ValorImpostoDevidoRecolherSt,
        decimal ValorImpostoDevidoFcp,
        decimal ValorIcmsIsento,
        decimal ValorIcmsOutros,
        string? IcmsObservacao,
        decimal ValorIpiIsento,
        decimal ValorIpiOutros,
        string? IpiObservacao
    );

    public record ItemImpostoIbsCbsInput(
        string Cst,
        string CClassTrib,
        decimal AliquotaEstadual,
        decimal AliquotaMunicipal,
        decimal AliquotaCbs,
        decimal AliquotaEstadualReducao,
        decimal AliquotaMunicipalReducao,
        decimal AliquotaCbsReducao,
        decimal AliquotaEstadualDiferimento,
        decimal AliquotaMunicipalDiferimento,
        decimal AliquotaCbsDiferimento,
        decimal AliquotaEfetivaEstadual,
        decimal AliquotaEfetivaMunicipal,
        decimal AliquotaEfetivaCbs,
        decimal ValorBaseDeCalculo,
        decimal ValorImpostoDevidoEstadual,
        decimal ValorImpostoDevidoMunicipal,
        decimal ValorImpostoDevidoCbs,
        ItemImpostoIbsCbsTributacaoRegularInput? TributacaoRegular
    );

    public record ItemImpostoIbsCbsTributacaoRegularInput(
        string Cst,
        string CClassTrib,
        decimal AliquotaEfetivaIbsEstadual,
        decimal ValorIbsEstadual,
        decimal AliquotaEfetivaIbsMunicipal,
        decimal ValorIbsMunicipal,
        decimal AliquotaEfetivaCbs,
        decimal ValorCbs
    );

    /// <summary>Adiciona um item fiscal completo (com imposto ICMS/ST/IPI/PIS/COFINS e IBS/CBS). Porte de Venda.AdicionarItem.</summary>
    public record AdicionarItemFiscalCommand(
        Guid VendaId,
        Guid ProdutoId,
        string CodigoProduto,
        string? CodigoEan,
        string DescricaoProduto,
        string Ncm,
        string? ExcecaoNcmTipi,
        Guid? CestId,
        string? Cest,
        Guid? CodigoAnpId,
        string? CodigoAnp,
        int Cfop,
        string UnidadeComercial,
        decimal QuantidadeComercial,
        decimal ValorUnitarioComercial,
        decimal ValorTotalBrutoProdutos,
        string? CodigoEanTributavel,
        string UnidadeTributavel,
        decimal QuantidadeTributavel,
        decimal ValorUnitarioTributavel,
        decimal ValorDesconto,
        decimal ValorDescontoRateado,
        decimal ValorFreteRateado,
        decimal ValorSeguroRateado,
        decimal ValorOutrasDepesasAcessoriasRateado,
        EIndicadorTotalizador CompoeValorTotal,
        string? InformacoesAdicionaisDoProduto,
        int CfopCorrelacao,
        bool IntegraFaturamento,
        int NumeroItemPedidoCompra,
        string? NumeroPedidoCompra,
        string? FichaConteudoImportacao,
        string? CodigoBeneficioFiscal,
        decimal ValorCusto,
        ItemImpostoInput Imposto,
        ItemImpostoIbsCbsInput? ImpostoIbsCbs
    ) : ICommand;

    /// <summary>Exclui (soft delete) um item da venda. Porte de Venda.DeletarItem.</summary>
    public record DeletarItemFiscalCommand(Guid VendaId, Guid ItemId) : ICommand;

    // ---------- Emitente / Destinatário ----------

    public record EnderecoInput(
        EEstado Uf,
        string Logradouro,
        string Numero,
        string Complemento,
        string Bairro,
        int MunicipioId,
        string MunicipioNome,
        string Cep,
        int PaisId,
        string PaisNome
    );

    /// <summary>Inclui/atualiza o emitente da venda. Porte de Venda.IncluirEmitente/AlterarEmitente.</summary>
    public record DefinirEmitenteCommand(
        Guid VendaId,
        Guid EmpresaId,
        string? Cnpj,
        string? Cpf,
        string RazaoSocial,
        string? NomeFantasia,
        string? Telefone,
        string InscricaoEstadual,
        string? InscricaoEstadualST,
        string? InscricaoMunicipal,
        int Cnae,
        ERegimeTributario RegimeTributario,
        EnderecoInput Endereco
    ) : ICommand;

    /// <summary>Inclui/atualiza o destinatário da venda. Porte de Venda.IncluirDestinatario/AlterarDestinatario.</summary>
    public record DefinirDestinatarioCommand(
        Guid VendaId,
        Guid? PessoaId,
        string? Cnpj,
        string? Cpf,
        string? RazaoSocial,
        string? Telefone,
        string? InscricaoEstadual,
        string? IdentificadorEstrangeiro,
        ETipoIndicadorIe IndicadorIE,
        string? Email,
        bool EhConsumidorFinal,
        string? DocumentoConsumidor,
        bool EnviarDestinatatioNaNfce
    ) : ICommand;

    // ---------- Totais / Configuração / Imposto ----------

    /// <summary>Inclui/atualiza os totais da venda. Porte de Venda.IncluirTotal/AlterarTotal.</summary>
    public record DefinirTotalCommand(
        Guid VendaId,
        decimal ValorBaseDeCalculoIcms,
        decimal ValorIcms,
        decimal ValorIcmsDesonerado,
        decimal ValorFcp,
        decimal ValorBaseDeCalculoSt,
        decimal ValorSt,
        decimal ValorFcpSt,
        decimal ValorFcpRet,
        decimal ValorProduto,
        decimal ValorFrete,
        decimal ValorSeguro,
        decimal ValorDesconto,
        decimal ValorImpostoImportacao,
        decimal ValorIpi,
        decimal ValorIpiDevolucao,
        decimal ValorPis,
        decimal ValorCofins,
        decimal ValorOutro,
        decimal ValorNotaFiscal
    ) : ICommand;

    /// <summary>Inclui/atualiza os totais IBS/CBS da venda. Porte de Venda.IncluirTotalIbsCbs/AlterarTotalIbsCbs.</summary>
    public record DefinirTotalIbsCbsCommand(
        Guid VendaId,
        decimal ValorBaseDeCalculo,
        decimal ValorImpostoDevidoEstadual,
        decimal ValorImpostoDevidoMunicipal,
        decimal ValorImpostoDevidoCbs
    ) : ICommand;

    /// <summary>Inclui/atualiza a configuração de emissão. Porte de Venda.IncluirConfiguracao/AlterarConfiguracao.</summary>
    public record DefinirConfiguracaoCommand(
        Guid VendaId,
        ETipoOperacaoNfe TipoOperacao,
        ETipoFormatoImpressaoDanfe TipoFormatoImpressaoDanfe,
        ETipoEmissao TipoEmissao,
        ETipoAmbiente TipoAmbiente,
        EFinalidadeEmissao FinalidadeEmissao,
        EIndicadorFinalidadeOperacao IndicadorFinalidadeOperacao,
        ETipoAtendimento TipoAtendimento,
        EIndicadorIntermediadorMarketplace IndicadorIntermediadorMarketplace
    ) : ICommand;

    /// <summary>Inclui/atualiza o crédito de ICMS da venda. Porte de Venda.AdicionarImposto/AlterarImposto.</summary>
    public record DefinirVendaImpostoCommand(Guid VendaId, decimal ValorAliquotaCreditoIcms) : ICommand;

    // ---------- Pagamentos ----------

    /// <summary>Adiciona um pagamento. Porte de Venda.AdicionarPagamento.</summary>
    public record AdicionarPagamentoCommand(
        Guid VendaId,
        decimal ValorTroco,
        ETipoPagamento TipoPagamento,
        decimal ValorPagamento,
        ETipoIntegracaoPagamentoCArtao CartaoTipoIntegracao,
        string? CartaoCnpjIntermediadorFinanceira,
        EBandeiraCartao CartaoBandeira,
        string? CartaoCodigoAutorizacaoOperacao
    ) : ICommand;

    /// <summary>Exclui (soft delete) um pagamento. Porte de Venda.DeletarPagamento.</summary>
    public record DeletarPagamentoCommand(Guid VendaId, Guid PagamentoId) : ICommand;

    // ---------- Fatura ----------

    /// <summary>Inclui/atualiza a fatura da venda. Porte de Venda.AdicionarFatura/AlterarFatura.</summary>
    public record DefinirFaturaCommand(
        Guid VendaId,
        string? NumeroFatura,
        decimal ValorOriginal,
        decimal ValorDesconto,
        decimal ValorLiquido
    ) : ICommand;

    /// <summary>Exclui (soft delete em cascata) a fatura da venda. Porte de Venda.DeletarFatura.</summary>
    public record DeletarFaturaCommand(Guid VendaId) : ICommand;

    /// <summary>Inclui uma duplicata na fatura. Porte de VendaFatura.IncluirDuplicata.</summary>
    public record IncluirDuplicataCommand(
        Guid VendaId,
        string NumeroDuplicata,
        DateTime DataVencimento,
        decimal ValorDuplicata
    ) : ICommand;

    // ---------- Transporte ----------

    public record VolumeInput(int QuantidadeVolumes, string? Especie, string? NumeroVolumes, decimal PesoLiquido, decimal PesoBruto, string? Marca);
    public record ReboqueInput(Guid? VeiculoId, string Placa, EEstado Uf, string? Rntrc);

    /// <summary>Inclui/atualiza o transporte da venda. Porte de Venda.IncluirTransporte.</summary>
    public record DefinirTransporteCommand(
        Guid VendaId,
        Guid? TransportadoraPessoaId,
        string? TransportadoraCnpj,
        string? TransportadoraCpf,
        string? TransportadoraRazaoSocial,
        string? TransportadoraInscricaoEstadual,
        string? TransportadoraLogradouro,
        string? TransportadoraNumero,
        string? TransportadoraComplemento,
        string? TransportadoraBairro,
        string? TransportadoraMunicipio,
        EEstado? TransportadoraUf,
        Guid? VeiculoId,
        string? VeiculoPlaca,
        EEstado? VeiculoUf,
        string? VeiculoRntrc,
        List<VolumeInput>? Volumes,
        List<ReboqueInput>? Reboques
    ) : ICommand;

    // ---------- NF-e / NFC-e ----------

    /// <summary>Inclui os dados da NFC-e. Porte de Venda.IncluirNfce.</summary>
    public record IncluirNfceCommand(Guid VendaId, int Serie, long Numero, string IdCsc, string Csc) : ICommand;

    /// <summary>Registra a transmissão autorizada da NFC-e. Porte de Venda.TransmitirNfce.</summary>
    public record TransmitirNfceCommand(Guid VendaId, string Chave, DateTime DataHoraEmissao, int StatusSefaz, string Protocolo) : ICommand;

    /// <summary>Cancela a NFC-e. Porte de Venda.CancelarNfce.</summary>
    public record CancelarNfceCommand(Guid VendaId, DateTime DataHoraCancelamento, string ProtocoloCancelamento, int StatusSefazCancelamento, string MotivoCancelamento) : ICommand;

    /// <summary>Inclui os dados da NF-e. Porte de Venda.IncluirNfe.</summary>
    public record IncluirNfeCommand(Guid VendaId, int Serie, long Numero, DateTime? DataHoraSaida, bool EmbuteFrete = true, bool EmbuteSeguro = true, bool EmbuteAcrescimo = true, bool EmbuteOutro = true) : ICommand;

    /// <summary>Registra a transmissão autorizada da NF-e. Porte de Venda.TransmitirNfe.</summary>
    public record TransmitirNfeCommand(Guid VendaId, string Chave, string NaturezaOperacao, DateTime DataHoraEmissao, int StatusSefaz, string Protocolo) : ICommand;

    /// <summary>Cancela a NF-e. Porte de Venda.CancelarNfe.</summary>
    public record CancelarNfeCommand(Guid VendaId, DateTime DataHoraCancelamento, string ProtocoloCancelamento, int StatusSefazCancelamento, string MotivoCancelamento) : ICommand;

    /// <summary>Adiciona uma NF-e referenciada. Porte de Venda.AdicionarNfeReferenciada.</summary>
    public record AdicionarNfeReferenciadaCommand(Guid VendaId, string ChaveAcesso) : ICommand;

    /// <summary>Adiciona uma carta de correção à NF-e da venda. Porte de VendaNfe.AdicionarCartaCorrecao.</summary>
    public record AdicionarCartaCorrecaoCommand(Guid VendaId, ETipoAmbiente Ambiente, string TextoCorrecao) : ICommand;

    /// <summary>Vincula o documento fiscal transmitido (módulo Fiscal) à venda por Guid FK.</summary>
    public record VincularDocumentoFiscalCommand(Guid VendaId, Guid DocumentoFiscalId) : ICommand;
}
