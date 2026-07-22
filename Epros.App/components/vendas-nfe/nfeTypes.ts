/**
 * Tipos e estado inicial da tela de Emissão de NF-e (fatia 7).
 *
 * Porta o modelo de dados de `types/nfe/nfe.ts` e `types/nfe/item.ts` do legado, enxugado
 * para o que a tela do design novo consome. O cálculo de impostos server-side (que no legado
 * era feito pelo hub SignalR `hubs/venda`) foi substituído por um cálculo de totais no cliente
 * (ver `calcularTotais` em `useNfeEmissao.ts`), já que o backend novo expõe apenas endpoints REST
 * (`vendas`, `vendas-fiscal/{id}/nfe/...`).
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

/** Tipo de atendimento (indicador de presença do comprador). */
export const TIPOS_ATENDIMENTO: { label: string; value: number }[] = [
  { label: '0 - Não se aplica', value: 0 },
  { label: '1 - Operação presencial', value: 1 },
  { label: '2 - Operação não presencial, pela internet', value: 2 },
  { label: '3 - Operação não presencial, Teleatendimento', value: 3 },
  { label: '5 - Operação não presencial, fora do estabelecimento', value: 5 },
  { label: '9 - Operação não presencial, outros', value: 9 }
]

/** Modalidades de pagamento (formas de pagamento da NF-e/NFC-e). */
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

export interface NfeDestinatario {
  pessoaId: number | null
  nome: string
  documento: string
  enderecoEntregaId: number | null
  enderecoCobrancaId: number | null
  enderecoFormatado: string
}

export interface NfeItem {
  /** Identificador local (linha) — usado só na UI. */
  _uid: string
  produtoId: number | null
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

export interface NfeDuplicata {
  _uid: string
  numero: string
  dataVencimento: string
  valor: number
}

export interface NfePagamento {
  _uid: string
  tipoPagamento: number
  valorPagamento: number
  valorTroco: number
}

export interface NfeTotais {
  valorProduto: number
  valorDesconto: number
  valorFrete: number
  valorSeguro: number
  valorOutro: number
  valorIcms: number
  valorIpi: number
  valorNotaFiscal: number
  valorRecebimento: number
}

export interface NfeForm {
  /** Guid da venda fiscal (rota vendas-fiscal/{id}). Nulo enquanto rascunho não persistido. */
  id: string | null
  /** Número/série exibidos após emissão (retornados pela API). */
  numero: string | null
  serie: string | null
  chave: string | null
  situacao: string | null
  naturezaOperacao: string
  tipoOperacaoFiscalId: number | null
  modalidadeFrete: number
  tipoAtendimento: number
  dataEmissao: string
  dataHoraSaida: string
  informacoesComplementares: string
  informacoesAdicionaisFisco: string
  destinatario: NfeDestinatario
  transportadora: { pessoaId: number | null; nome: string; documento: string }
  itens: NfeItem[]
  duplicatas: NfeDuplicata[]
  pagamentos: NfePagamento[]
  chavesReferenciadas: string[]
  cartasCorrecao: { sequencia: number; texto: string; dataRegistro: string }[]
  /** Ajustes manuais dos totais. */
  descontoManual: number
  freteManual: number
  seguroManual: number
  outroManual: number
}

/** Cria uma linha de produto vazia. */
export function criarItemVazio(): NfeItem {
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

/** Cria uma duplicata vazia. */
export function criarDuplicataVazia(): NfeDuplicata {
  return { _uid: gerarUid(), numero: '', dataVencimento: '', valor: 0 }
}

/** Cria um pagamento vazio (padrão dinheiro). */
export function criarPagamentoVazio(): NfePagamento {
  return { _uid: gerarUid(), tipoPagamento: 1, valorPagamento: 0, valorTroco: 0 }
}

/** Estado inicial do formulário de NF-e. */
export function criarNfeFormInicial(): NfeForm {
  const agora = new Date().toISOString()
  return {
    id: null,
    numero: null,
    serie: null,
    chave: null,
    situacao: null,
    naturezaOperacao: '',
    tipoOperacaoFiscalId: null,
    modalidadeFrete: 9,
    tipoAtendimento: 0,
    dataEmissao: agora,
    dataHoraSaida: agora,
    informacoesComplementares: '',
    informacoesAdicionaisFisco: '',
    destinatario: {
      pessoaId: null,
      nome: '',
      documento: '',
      enderecoEntregaId: null,
      enderecoCobrancaId: null,
      enderecoFormatado: ''
    },
    transportadora: { pessoaId: null, nome: '', documento: '' },
    itens: [],
    duplicatas: [],
    pagamentos: [],
    chavesReferenciadas: [],
    cartasCorrecao: [],
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
