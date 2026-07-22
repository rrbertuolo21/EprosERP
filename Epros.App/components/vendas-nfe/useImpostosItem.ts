/**
 * Tipos de tributação por item de NF-e (fatia Vendas/NFe).
 *
 * Porta `ImpostoDto`/`VendaItemDto` de `types/nfe/item.ts` do legado — todos os campos de
 * cálculo tributário por linha de produto (ICMS/ICMS-ST/DIFAL/FCP/PIS/COFINS/IPI/IBS-CBS).
 * No legado esses valores eram calculados no servidor (hub `hubs/venda`, método
 * `AdicionarProduto`/`AtualizarProduto`); aqui ficam como estado editável/exibido pela tela —
 * o back-end novo (REST) é quem calcula e devolve os valores ao gravar/consultar o item.
 *
 * Consumido por `ImpostosTabsDialog.vue` (tela de detalhe de impostos por item) e por
 * `useNfeProdutos` (estado da linha de produto da NF-e).
 */

/** Base de cálculo, alíquotas e valores devidos por tributo, para 1 item da NF-e. */
export interface ImpostoItem {
  // ICMS
  cstIcms: string
  origem: string
  csosnIcms: string
  valorBaseDeCalculoIcms: number
  valorBaseDeCalculoStIcms: number
  valorBaseDeCalculoFcpIcms: number
  valorBaseDeCalculoStFcpIcms: number
  valorBaseDeCalculoDifalIcms: number
  aliquotaIcms: number
  aliquotaStIcms: number
  aliquotaFcpIcms: number
  aliquotaFcpStIcms: number
  aliquotaMvaIcms: number
  aliquotaDifalInternaIcms: number
  aliquotaDifalInterestadualIcms: number
  aliquotaReducaoPercentualIcms: number
  aliquotaReducaoPercentualStIcms: number
  valorUnitFixadoIcmsStIcms: number
  valorImpostoDevidoIcms: number
  valorCreditoIcms: number
  valorImpostoDevidoFcpIcms: number
  valorImpostoDevidoFcpStIcms: number
  valorImpostoDevidoStIcms: number
  valorImpostoDevidoRecolherStIcms: number
  valorImpostoDevidoRecolherFcpStIcms: number
  valorImpostoDevidoDifalIcms: number
  valorImpostoDevidoFcp: number

  // PIS
  cstPis: string
  quantidadeVendidaPis: number
  valorBaseDeCalculoPis: number
  aliquotaPercetualPis: number
  aliquotaRealPis: number
  valorImpostoDevidoPis: number

  // COFINS
  cstCofins: string
  quantidadeVendidaCofins: number
  valorBaseDeCalculoCofins: number
  aliquotaPercetualCofins: number
  aliquotaRealCofins: number
  valorImpostoDevidoCofins: number

  // IPI
  valorBaseDeCalculoIpi: number
  aliquotaIpi: number
  valorImpostoDevidoIpi: number
  reducaoPercentualIpi: number
  cstIpi: string

  // Imposto de Importação (II)
  valorBaseCalculoImpostoImportacao: number
  valorDespesaAduaneira: number
  valorImpostoImportacao: number
  valorIOFImpostoImportacao: number

  // IBS/CBS (reforma tributária)
  cstIbsCbs: string
  cClassTrib: string
  baseDeCalculoIbsCbs: number
  aliquotaEstadualIbsCbs: number
  aliquotaMunicipalIbsCbs: number
  aliquotaCbs: number
  aliquotaEstadualReducaoIbsCbs: number
  aliquotaMunicipalReducaoIbsCbs: number
  aliquotaCbsReducaoIbsCbs: number
  aliquotaEstadualDiferimentoIbsCbs: number
  aliquotaMunicipalDiferimentoIbsCbs: number
  aliquotaCbsDiferimentoIbsCbs: number
  aliquotaEfetivaEstadualIbsCbs: number
  aliquotaEfetivaMunicipalIbsCbs: number
  aliquotaEfetivaCbsIbsCbs: number
  valorImpostoDevidoEstadualIbsCbs: number
  valorImpostoDevidoMunicipalIbsCbs: number
  valorImpostoDevidoCbsIbsCbs: number
}

/** Regras de rateio/composição do item (embutir frete/seguro/acréscimo/outro na base). */
export interface ItemNfeDadosFiscais {
  produtoId: number | null
  codigoProduto: string
  nomeProduto: string
  codigoBarras: string
  ncm: string
  cfop: number | null
  naturezaOperacao: string | null
  unidade: string
  valorUnitario: number
  quantidade: number
  valorDesconto: number
  enquadramentoIpi: string
  cest: string
  ipiEmbutido: boolean
  difalTipoCalculoPorDentro: boolean
  embuteFrete: boolean
  embuteSeguro: boolean
  embuteAcrescimo: boolean
  embuteOutro: boolean
  totalItem: number
  /** Rateios aplicados ao item (frete/seguro/acréscimo distribuídos proporcionalmente). */
  valorFreteRateado: number
  valorSeguroRateado: number
  valorAcrescimoRateado: number
  numeroItemPedidoCompra: number | null
  numeroPedidoCompra: string | null
  fichaConteudoImportacao: string | null
  codigoBeneficioFiscal: string | null
  imposto: ImpostoItem
}

/** Estado inicial (zerado) de `ImpostoItem`. */
export function criarImpostoItemVazio(): ImpostoItem {
  return {
    cstIcms: '',
    origem: '0',
    csosnIcms: '',
    valorBaseDeCalculoIcms: 0,
    valorBaseDeCalculoStIcms: 0,
    valorBaseDeCalculoFcpIcms: 0,
    valorBaseDeCalculoStFcpIcms: 0,
    valorBaseDeCalculoDifalIcms: 0,
    aliquotaIcms: 0,
    aliquotaStIcms: 0,
    aliquotaFcpIcms: 0,
    aliquotaFcpStIcms: 0,
    aliquotaMvaIcms: 0,
    aliquotaDifalInternaIcms: 0,
    aliquotaDifalInterestadualIcms: 0,
    aliquotaReducaoPercentualIcms: 0,
    aliquotaReducaoPercentualStIcms: 0,
    valorUnitFixadoIcmsStIcms: 0,
    valorImpostoDevidoIcms: 0,
    valorCreditoIcms: 0,
    valorImpostoDevidoFcpIcms: 0,
    valorImpostoDevidoFcpStIcms: 0,
    valorImpostoDevidoStIcms: 0,
    valorImpostoDevidoRecolherStIcms: 0,
    valorImpostoDevidoRecolherFcpStIcms: 0,
    valorImpostoDevidoDifalIcms: 0,
    valorImpostoDevidoFcp: 0,
    cstPis: '',
    quantidadeVendidaPis: 0,
    valorBaseDeCalculoPis: 0,
    aliquotaPercetualPis: 0,
    aliquotaRealPis: 0,
    valorImpostoDevidoPis: 0,
    cstCofins: '',
    quantidadeVendidaCofins: 0,
    valorBaseDeCalculoCofins: 0,
    aliquotaPercetualCofins: 0,
    aliquotaRealCofins: 0,
    valorImpostoDevidoCofins: 0,
    valorBaseDeCalculoIpi: 0,
    aliquotaIpi: 0,
    valorImpostoDevidoIpi: 0,
    reducaoPercentualIpi: 0,
    cstIpi: '',
    valorBaseCalculoImpostoImportacao: 0,
    valorDespesaAduaneira: 0,
    valorImpostoImportacao: 0,
    valorIOFImpostoImportacao: 0,
    cstIbsCbs: '',
    cClassTrib: '',
    baseDeCalculoIbsCbs: 0,
    aliquotaEstadualIbsCbs: 0,
    aliquotaMunicipalIbsCbs: 0,
    aliquotaCbs: 0,
    aliquotaEstadualReducaoIbsCbs: 0,
    aliquotaMunicipalReducaoIbsCbs: 0,
    aliquotaCbsReducaoIbsCbs: 0,
    aliquotaEstadualDiferimentoIbsCbs: 0,
    aliquotaMunicipalDiferimentoIbsCbs: 0,
    aliquotaCbsDiferimentoIbsCbs: 0,
    aliquotaEfetivaEstadualIbsCbs: 0,
    aliquotaEfetivaMunicipalIbsCbs: 0,
    aliquotaEfetivaCbsIbsCbs: 0,
    valorImpostoDevidoEstadualIbsCbs: 0,
    valorImpostoDevidoMunicipalIbsCbs: 0,
    valorImpostoDevidoCbsIbsCbs: 0
  }
}

/** Estado inicial (zerado) de `ItemNfeDadosFiscais`. */
export function criarItemNfeDadosFiscaisVazio(): ItemNfeDadosFiscais {
  return {
    produtoId: null,
    codigoProduto: '',
    nomeProduto: '',
    codigoBarras: '',
    ncm: '',
    cfop: null,
    naturezaOperacao: null,
    unidade: 'UN',
    valorUnitario: 0,
    quantidade: 1,
    valorDesconto: 0,
    enquadramentoIpi: '',
    cest: '',
    ipiEmbutido: false,
    difalTipoCalculoPorDentro: false,
    embuteFrete: true,
    embuteSeguro: true,
    embuteAcrescimo: true,
    embuteOutro: true,
    totalItem: 0,
    valorFreteRateado: 0,
    valorSeguroRateado: 0,
    valorAcrescimoRateado: 0,
    numeroItemPedidoCompra: null,
    numeroPedidoCompra: null,
    fichaConteudoImportacao: null,
    codigoBeneficioFiscal: null,
    imposto: criarImpostoItemVazio()
  }
}
