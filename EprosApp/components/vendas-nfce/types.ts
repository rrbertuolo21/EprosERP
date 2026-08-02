/**
 * Tipos de domínio da emissão de NFC-e (modelo fiscal 65).
 *
 * Portados de `types/nfce.ts` do legado, adaptados ao envelope CommandResult do novo backend.
 * Ficam nesta pasta da fatia (exclusiva) — não são um recurso compartilhado.
 */

/** Modelo fiscal do documento. */
export enum ModeloFiscal {
  NFCe = 65,
  NFe = 55
}

/** Ação de gravação escolhida ao finalizar a venda. */
export enum StatusNfce {
  /** Apenas salva a venda (rascunho), sem transmitir à SEFAZ. */
  SALVAR = 0,
  /** Salva e transmite a NFC-e. */
  SALVAR_TRANSMITIR = 1,
  /** Já transmitida. */
  TRANSMITIDO = 2
}

/** Formas de pagamento aceitas na NFC-e (tabela SEFAZ tPag). */
export enum TipoPagamento {
  DINHEIRO = 1,
  CHEQUE = 2,
  CARTAO_CREDITO = 3,
  CARTAO_DEBITO = 4,
  CREDITO_LOJA = 5,
  VALE_ALIMENTACAO = 10,
  VALE_REFEICAO = 11,
  VALE_PRESENTE = 12,
  VALE_COMBUSTIVEL = 13,
  BOLETO = 15,
  DEPOSITO_BANCARIO = 16,
  PIX = 17,
  TRANSFERENCIA_BANCARIA = 18,
  PROGRAMA_FIDELIDADE = 19,
  SEM_PAGAMENTO = 90,
  OUTROS = 99
}

/** Opções de forma de pagamento para o select. */
export const OPCOES_TIPO_PAGAMENTO: { label: string; value: number }[] = [
  { label: 'Dinheiro', value: TipoPagamento.DINHEIRO },
  { label: 'PIX', value: TipoPagamento.PIX },
  { label: 'Cartão de Crédito', value: TipoPagamento.CARTAO_CREDITO },
  { label: 'Cartão de Débito', value: TipoPagamento.CARTAO_DEBITO },
  { label: 'Cheque', value: TipoPagamento.CHEQUE },
  { label: 'Crédito Loja', value: TipoPagamento.CREDITO_LOJA },
  { label: 'Vale Alimentação', value: TipoPagamento.VALE_ALIMENTACAO },
  { label: 'Vale Refeição', value: TipoPagamento.VALE_REFEICAO },
  { label: 'Boleto Bancário', value: TipoPagamento.BOLETO },
  { label: 'Transferência Bancária', value: TipoPagamento.TRANSFERENCIA_BANCARIA },
  { label: 'Sem Pagamento', value: TipoPagamento.SEM_PAGAMENTO },
  { label: 'Outros', value: TipoPagamento.OUTROS }
]

/** Produto resumido usado na busca de itens. */
export interface ProdutoResumo {
  id: number
  codigo?: string | number | null
  descricao?: string | null
  valorVenda?: number | null
  unidadeComercial?: string | null
}

/** Item da NFC-e. */
export interface NfceItem {
  produtoId: number
  descricao: string
  unidade?: string | null
  quantidadeComercial: number
  valorUnitarioComercial: number
  valorDesconto: number
}

/** Destinatário/consumidor da NFC-e. */
export interface NfceDestinatario {
  pessoaId: number | null
  documentoConsumidor: string
  enviarDestinatarioNaNfce: boolean
  descricao: string
}

/** Pagamento informado. */
export interface NfcePagamento {
  tipoPagamento: number
  valorPagamento: number
  valorTroco: number
}

/** Estrutura completa da NFC-e em edição. */
export interface Nfce {
  id?: number | null
  modeloFiscal: ModeloFiscal
  status: StatusNfce
  statusSefaz?: number | null
  emitente: { empresaId: number }
  destinatario: NfceDestinatario
  itens: NfceItem[]
  pagamentos: NfcePagamento[]
  informacoesComplementares: string
  informacoesAdicionaisFisco: string
}

/** Cria o estado inicial de uma NFC-e (equivalente ao createNfceInitialState do legado). */
export function criarNfceInicial(empresaId: number): Nfce {
  return {
    id: null,
    modeloFiscal: ModeloFiscal.NFCe,
    status: StatusNfce.SALVAR_TRANSMITIR,
    statusSefaz: null,
    emitente: { empresaId },
    destinatario: {
      pessoaId: null,
      documentoConsumidor: '',
      enviarDestinatarioNaNfce: false,
      descricao: ''
    },
    itens: [],
    pagamentos: [],
    informacoesComplementares: '',
    informacoesAdicionaisFisco: ''
  }
}

/** Resultado da transmissão/gravação, extraído da resposta da API. */
export interface NfceResultado {
  vendaId: number | null
  numero: number | null
  chave: string | null
  url: string | null
}
