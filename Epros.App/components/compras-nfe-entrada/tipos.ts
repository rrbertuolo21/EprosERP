/**
 * Tipos e estado inicial das telas de NF-e de Entrada (Compras) — fatia 13.
 *
 * Porta o modelo de dados das telas legadas `compras/emissao/nfe-entrada/[[id]]` e
 * `compras/emissao/devolucao-retorno/nfe-entrada/[[id]]` (que no legado usavam
 * `useNfeEmissaoPage(nfeEntradaConfig)`), enxugado para o que o design novo consome.
 *
 * Diferenças em relação à emissão de saída (fatia 7 — vendas-nfe):
 *  - O parceiro da nota é o FORNECEDOR (não o destinatário/cliente).
 *  - Os impostos são exibidos em "modo entrada" (compra de mercadoria).
 *  - A devolução/retorno referencia a(s) NF-e de origem (chaves referenciadas).
 *
 * O cálculo de totais é feito no cliente (o backend novo expõe apenas REST — `compras`),
 * espelhando a abordagem de `useNfeEmissao` da fatia 7.
 */

/** Modalidade de frete (mesmos códigos do padrão SEFAZ). */
export const MODALIDADES_FRETE: { label: string; value: number }[] = [
  { label: '0 - Contratação por conta do Remetente (CIF)', value: 0 },
  { label: '1 - Contratação por conta do Destinatário (FOB)', value: 1 },
  { label: '2 - Contratação por conta de Terceiros', value: 2 },
  { label: '3 - Transporte Próprio por conta do Remetente', value: 3 },
  { label: '4 - Transporte Próprio por conta do Destinatário', value: 4 },
  { label: '9 - Sem Ocorrência de Transporte', value: 9 }
]

/** Modalidades de pagamento (mesmas opções do módulo de vendas). */
export const MODALIDADES_PAGAMENTO: { label: string; value: number }[] = [
  { label: 'Dinheiro', value: 1 },
  { label: 'Cheque', value: 2 },
  { label: 'Cartão de Crédito', value: 3 },
  { label: 'Cartão de Débito', value: 4 },
  { label: 'Crédito Loja', value: 5 },
  { label: 'Vale Alimentação', value: 10 },
  { label: 'Vale Refeição', value: 11 },
  { label: 'Vale Presente', value: 12 },
  { label: 'Vale Combustível', value: 13 },
  { label: 'Boleto Bancário', value: 15 },
  { label: 'Depósito Bancário', value: 16 },
  { label: 'PIX', value: 17 },
  { label: 'Transferência bancária', value: 18 },
  { label: 'Sem pagamento', value: 90 },
  { label: 'Outros', value: 99 }
]

/** Fornecedor da NF-e de entrada (o "destinatário" da tela de saída vira o fornecedor). */
export interface EntradaFornecedor {
  pessoaId: number | null
  nome: string
  documento: string
  inscricaoEstadual: string
  enderecoFormatado: string
}

/** Linha de produto da NF-e de entrada. */
export interface EntradaItem {
  /** Identificador local (linha) — usado só na UI. */
  _uid: string
  produtoId: number | null
  /** Código/SKU do produto (usado no payload `compras/lancar`). */
  codigoProduto: string
  nomeProduto: string
  ncm: string
  cfop: number | null
  csosnCst: string
  unidade: string
  quantidade: number
  valorUnitario: number
  descontoValor: number
  aliquotaIcms: number
  aliquotaIpi: number
  /** Total da linha (quantidade * unitário - desconto). Calculado no cliente. */
  total: number
  informacoesAdicionais: string
}

/** Duplicata (parcela) do pagamento ao fornecedor — gera contas a pagar. */
export interface EntradaDuplicata {
  _uid: string
  numero: string
  dataVencimento: string
  valor: number
}

/** Forma de pagamento ao fornecedor. */
export interface EntradaPagamento {
  _uid: string
  tipoPagamento: number
  valorPagamento: number
  valorTroco: number
}

export interface EntradaTotais {
  valorProduto: number
  valorDesconto: number
  valorFrete: number
  valorSeguro: number
  valorOutro: number
  valorIcms: number
  valorIpi: number
  valorNotaFiscal: number
}

export interface EntradaForm {
  /** Guid da compra (rota compras/{id}). Nulo enquanto rascunho não persistido/lançado. */
  id: string | null
  numero: string | null
  serie: string | null
  chave: string | null
  situacao: string | null
  naturezaOperacao: string
  modalidadeFrete: number
  dataEmissao: string
  dataEntrada: string
  informacoesComplementares: string
  informacoesAdicionaisFisco: string
  fornecedor: EntradaFornecedor
  transportadora: { pessoaId: number | null; nome: string; documento: string }
  itens: EntradaItem[]
  /** Duplicatas (parcelas) do pagamento ao fornecedor. */
  duplicatas: EntradaDuplicata[]
  /** Formas de pagamento ao fornecedor. */
  pagamentos: EntradaPagamento[]
  /** Chaves de NF-e referenciadas (obrigatório na devolução/retorno). */
  chavesReferenciadas: string[]
  /** Ajustes manuais dos totais. */
  descontoManual: number
  freteManual: number
  seguroManual: number
  outroManual: number
}

/** Cria uma duplicata vazia. */
export function criarDuplicataVazia(): EntradaDuplicata {
  return { _uid: gerarUid(), numero: '', dataVencimento: '', valor: 0 }
}

/** Cria um pagamento vazio (padrão dinheiro). */
export function criarPagamentoVazio(): EntradaPagamento {
  return { _uid: gerarUid(), tipoPagamento: 1, valorPagamento: 0, valorTroco: 0 }
}

/** Cria uma linha de produto vazia. */
export function criarItemVazio(): EntradaItem {
  return {
    _uid: gerarUid(),
    produtoId: null,
    codigoProduto: '',
    nomeProduto: '',
    ncm: '',
    cfop: null,
    csosnCst: '',
    unidade: 'UN',
    quantidade: 1,
    valorUnitario: 0,
    descontoValor: 0,
    aliquotaIcms: 0,
    aliquotaIpi: 0,
    total: 0,
    informacoesAdicionais: ''
  }
}

/**
 * Estado inicial do formulário de NF-e de entrada.
 *
 * @param modo 'entrada' (padrão) ou 'devolucao' — o modo devolução ajusta a natureza padrão.
 */
export function criarEntradaFormInicial(modo: 'entrada' | 'devolucao' = 'entrada'): EntradaForm {
  const agora = new Date().toISOString()
  return {
    id: null,
    numero: null,
    serie: null,
    chave: null,
    situacao: null,
    naturezaOperacao: modo === 'devolucao' ? 'Devolução/Retorno de mercadoria' : 'Compra para comercialização',
    modalidadeFrete: 9,
    dataEmissao: agora,
    dataEntrada: agora,
    informacoesComplementares: '',
    informacoesAdicionaisFisco: '',
    fornecedor: {
      pessoaId: null,
      nome: '',
      documento: '',
      inscricaoEstadual: '',
      enderecoFormatado: ''
    },
    transportadora: { pessoaId: null, nome: '', documento: '' },
    itens: [],
    duplicatas: [],
    pagamentos: [],
    chavesReferenciadas: [],
    descontoManual: 0,
    freteManual: 0,
    seguroManual: 0,
    outroManual: 0
  }
}

/** Gera um identificador local curto para chaves de linha. */
export function gerarUid(): string {
  return `l-${Math.random().toString(36).slice(2, 10)}`
}
