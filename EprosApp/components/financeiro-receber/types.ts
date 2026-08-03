/**
 * Tipos compartilhados entre as telas/dialogs de Contas a Receber.
 */

export type SituacaoTitulo = 1 | 2 | 3 | 4 // 1 Aberta, 2 Paga, 3 Paga Parcial, 4 Cancelada

export interface SituacaoInfo {
  texto: string
  cor: 'warning' | 'success' | 'info' | 'error' | 'grey'
}

export interface ContasAReceberItem {
  id?: number
  dataRecebimento: string | null
  tipoPagamento?: number | null
  formaPagamento?: string
  contaBancariaId?: number | null
  contaBancaria?: string | null
  planoDeContasFinanceiroItemId?: number | null
  valorDesconto: number
  valorJuros: number
  valorMulta: number
  valorAcrescimo: number
  valorParcela: number
  valorPago: number
}

export interface ContasAReceber {
  id?: number
  documento: string
  numeroParcela: number
  situacao: SituacaoTitulo
  dataEmissao: string | null
  dataVencimento: string | null
  dataBaixa: string | null
  detalhamento: string | null
  pessoaId: number | null
  nomePessoa?: string | null
  planoDeContasFinanceiroItemId: number | null
  valorTitulo: number
  valorInicialDesconto: number
  valorInicialMulta: number
  valorInicialJuros: number
  valorInicialAcrescimo: number
  valorTotalDesconto: number
  valorTotalMulta: number
  valorTotalJuros: number
  valorTotalAcrescimo: number
  valorTituloLiquido: number
  valorTotalRecebido: number
  justificativaCancelamento?: string | null
  origem?: number | null
  fatoGeradorFinanceiro?: {
    origem: number
    descricao: string
  } | null
  contasAReceberItens: ContasAReceberItem[]
  situacaoInfo?: SituacaoInfo
}

export interface PessoaBusca {
  id: number
  nome: string
  razaoSocial?: string | null
  nomeFantasia?: string | null
  cpf?: string | null
  cnpj?: string | null
}

export interface PlanoDeContasItem {
  id: number
  descricao?: string
  descricaoFormatada?: string
}

export const SITUACAO_LABEL: Record<number, SituacaoInfo> = {
  1: { texto: 'Aberto', cor: 'warning' },
  2: { texto: 'Pago', cor: 'success' },
  3: { texto: 'Pago Parcial', cor: 'info' },
  4: { texto: 'Cancelado', cor: 'error' }
}

export function situacaoInfo(situacao: number | undefined | null): SituacaoInfo {
  return SITUACAO_LABEL[situacao ?? 0] ?? { texto: 'Desconhecido', cor: 'grey' }
}
