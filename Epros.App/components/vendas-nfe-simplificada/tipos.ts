/**
 * Tipos locais da fatia "Emissão NF-e Simplificada".
 *
 * Portados do comportamento do legado (`types/nfce.ts` + composables de venda),
 * porém reduzidos ao que a emissão simplificada realmente usa. Como o projeto novo
 * ainda não gera tipos OpenAPI, tipamos manualmente o contrato consumido pelas telas.
 */

/** Modelo fiscal do documento. A NF-e simplificada é sempre modelo 55 (NF-e). */
export const MODELO_FISCAL_NFE = 55 as const

/** Modalidade de frete "sem transporte" (não incide no total). */
export const MODALIDADE_FRETE_SEM = 9 as const

/** Produto minimamente descrito para exibição no item da venda. */
export interface ProdutoResumo {
  id: number
  codigo?: string
  descricao?: string
  unidadeComercial?: string
  valorUnitario?: number
}

/** Item da venda na tela (linha da lista). */
export interface ItemVenda {
  produtoId: number
  produto?: ProdutoResumo
  descricao: string
  quantidadeComercial: number
  valorUnitarioComercial: number
  valorDesconto: number
}

/** Pagamento informado na venda. */
export interface PagamentoVenda {
  id?: number
  tipoPagamento: number
  valorPagamento: number
  valorTroco?: number
}

/** Destinatário/consumidor da NF-e. */
export interface DestinatarioVenda {
  pessoaId: number
  documentoConsumidor: string
  descricao: string
  enviarDestinatarioNaNfe: boolean
}

/** Estado reativo completo da NF-e simplificada em edição. */
export interface NfeSimplificada {
  id?: number | null
  modeloFiscal: number
  statusSefaz?: number
  destinatario: DestinatarioVenda
  itens: ItemVenda[]
  pagamentos: PagamentoVenda[]
  informacoesComplementares: string
  informacoesAdicionaisFisco: string
}

/** Corpo enviado para gravar a venda (POST/PUT `vendas`). */
export interface VendaGravarBody {
  id?: number
  modeloFiscal: number
  emitente: { empresaId: number }
  destinatario: {
    pessoaId: number
    documentoConsumidor: string
    enviarDestinatarioNaNfe: boolean
  }
  itens: Array<{
    produtoId: number
    quantidadeComercial: number
    valorUnitarioComercial: number
    valorDesconto: number
  }>
  pagamentos: PagamentoVenda[]
  total: {
    valorFrete: number
    valorDesconto: number
    valorOutro: number
  }
  informacoesComplementares?: string
  informacoesAdicionaisFisco?: string
}

/** Estado inicial de uma NF-e simplificada em branco. */
export function novaNfeSimplificada(): NfeSimplificada {
  return {
    id: null,
    modeloFiscal: MODELO_FISCAL_NFE,
    statusSefaz: undefined,
    destinatario: {
      pessoaId: 0,
      documentoConsumidor: '',
      descricao: '',
      enviarDestinatarioNaNfe: true
    },
    itens: [],
    pagamentos: [],
    informacoesComplementares: '',
    informacoesAdicionaisFisco: ''
  }
}

/** Opções fixas de forma de pagamento (espelham o legado da emissão simplificada). */
export const TIPOS_PAGAMENTO: Array<{ label: string; value: number }> = [
  { label: 'Dinheiro', value: 1 },
  { label: 'Cheque', value: 2 },
  { label: 'Cartão de Crédito', value: 3 },
  { label: 'Cartão de Débito', value: 4 },
  { label: 'Crédito Loja', value: 5 },
  { label: 'PIX', value: 17 },
  { label: 'Sem Pagamento', value: 90 },
  { label: 'Outros', value: 99 }
]
