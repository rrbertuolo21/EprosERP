/**
 * Tipos do domínio Contas a Pagar (financeiro/contas-pagar).
 * Espelha `types/financeiro/recebimento.ts` do legado (par Contas a Receber, que é o único
 * lado do módulo Financeiro com implementação de composables completa) invertendo o sentido
 * para "a pagar": pessoa é o Fornecedor, os itens representam baixas (pagamentos) da parcela.
 */

/** Item de baixa (pagamento) de uma parcela de contas a pagar. */
export interface ContasAPagarItem {
  id?: number
  contasAPagarId?: number
  contaBancariaId: number | null
  dataPagamento: string
  planoDeContasFinanceiroItemId: number | null
  tipoPagamento: number | null
  valorAPagar: number
  valorAcrescimo: number
  valorDesconto: number
  valorJuros: number
  valorMulta: number
  valorPago: number
  valorParcela: number
}

/** Registro de conta a pagar (título), no formato retornado pela API. */
export interface ContasAPagar {
  id: number
  pessoaId: number
  nomeFornecedor: string
  planoDeContasFinanceiroItemId: number | null
  situacao: number
  contasAPagarItens: ContasAPagarItem[]
  dataBaixa: string | null
  dataEmissao: string
  dataVencimento: string
  detalhamento: string
  documento: string
  valorTitulo: number
  valorInicialDesconto: number
  valorInicialMulta: number
  valorInicialJuros: number
  valorInicialAcrescimo: number
  valorTotalAPagarTitulo: number
  valorTotalAcrescimo: number
  valorTotalDesconto: number
  valorTotalJuros: number
  valorTotalMulta: number
  valorTotalPago: number
  numeroParcela: number
  justificativaCancelamento: string | null
  fatoGeradorFinanceiro?: {
    compraId: number | null
    descricao: string
    id: number
    origem: number
  } | null
}

/** Filtros de listagem — usados por `useApiList`. */
export interface ContasAPagarFiltros extends Record<string, unknown> {
  documento: string
  nomeFornecedor: string
  situacao: string
  dataVencimento: string
}

/** Totais de resumo (cards do topo da listagem). */
export interface ContasAPagarTotais {
  valorVencendoHoje: number
  valorVencido: number
  valorAVencer: number
}

/** Situações possíveis do título — mesmos códigos usados em Contas a Receber. */
export enum SITUACAO_CONTAS_A_PAGAR {
  ABERTA = 1,
  PAGO = 2,
  PAGO_PARCIAL = 3,
  CANCELADO = 4
}

/** Resolve texto + classe de badge (`.badge-<classe>`, ver assets/css/main.css) para a situação. */
export function situacaoContasAPagarInfo(situacao: number): { texto: string; classe: string } {
  switch (situacao) {
    case SITUACAO_CONTAS_A_PAGAR.ABERTA:
      return { texto: 'Aberta', classe: 'pendente' }
    case SITUACAO_CONTAS_A_PAGAR.PAGO:
      return { texto: 'Paga', classe: 'paga' }
    case SITUACAO_CONTAS_A_PAGAR.PAGO_PARCIAL:
      return { texto: 'Paga Parcial', classe: 'pendente' }
    case SITUACAO_CONTAS_A_PAGAR.CANCELADO:
      return { texto: 'Cancelada', classe: 'cancelada' }
    default:
      return { texto: 'Desconhecido', classe: 'cancelada' }
  }
}
