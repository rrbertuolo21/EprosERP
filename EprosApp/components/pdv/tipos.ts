/**
 * Tipos locais da fatia PDV (frente de caixa / NFC-e em modo caixa).
 *
 * Portados do legado (`types/nfce.ts` + componentes `components/pos/*`), adaptados ao
 * novo backend (IDs em Guid/string, envelope CommandResult). Ficam nesta pasta por serem
 * exclusivos da fatia — nenhuma outra tela os consome.
 */

/** Modelo fiscal do documento. No PDV é sempre NFC-e (65). */
export enum ModeloFiscal {
  NFCe = 65,
  NFe = 55
}

/** Situação de gravação escolhida pelo operador ao finalizar. */
export enum StatusVenda {
  /** Só grava a venda (sem transmitir). */
  Salvar = 0,
  /** Grava e transmite a NFC-e à SEFAZ. */
  SalvarTransmitir = 1,
  /** Já transmitida. */
  Transmitida = 2,
  /** MEI: grava e imprime cupom não fiscal. */
  SalvarImprimirMei = 3
}

/**
 * Tipos de pagamento aceitos (códigos oficiais da NFC-e).
 * Mesmos valores do legado (`TipoPagamento`).
 */
export enum TipoPagamento {
  Dinheiro = 1,
  Cheque = 2,
  CartaoCredito = 3,
  CartaoDebito = 4,
  CreditoLoja = 5,
  ValeAlimentacao = 10,
  ValeRefeicao = 11,
  ValePresente = 12,
  ValeCombustivel = 13,
  Boleto = 15,
  DepositoBancario = 16,
  Pix = 17,
  TransferenciaBancaria = 18,
  ProgramaFidelidade = 19,
  SemPagamento = 90,
  Outros = 99
}

/** Rótulos amigáveis para exibição das formas de pagamento. */
export const ROTULO_PAGAMENTO: Record<number, string> = {
  [TipoPagamento.Dinheiro]: 'Dinheiro',
  [TipoPagamento.Cheque]: 'Cheque',
  [TipoPagamento.CartaoCredito]: 'Cartão de Crédito',
  [TipoPagamento.CartaoDebito]: 'Cartão de Débito',
  [TipoPagamento.CreditoLoja]: 'Crédito Loja',
  [TipoPagamento.ValeAlimentacao]: 'Vale Alimentação',
  [TipoPagamento.ValeRefeicao]: 'Vale Refeição',
  [TipoPagamento.ValePresente]: 'Vale Presente',
  [TipoPagamento.ValeCombustivel]: 'Vale Combustível',
  [TipoPagamento.Boleto]: 'Parcelado',
  [TipoPagamento.DepositoBancario]: 'Depósito Bancário',
  [TipoPagamento.Pix]: 'PIX',
  [TipoPagamento.TransferenciaBancaria]: 'Transferência',
  [TipoPagamento.ProgramaFidelidade]: 'Fidelidade',
  [TipoPagamento.SemPagamento]: 'Sem Pagamento',
  [TipoPagamento.Outros]: 'Outros'
}

/** Produto retornado por `estoque-produtos` (campos usados no caixa). */
export interface ProdutoPdv {
  id: string
  codigo?: string | null
  descricao?: string | null
  ean?: string | null
  valorVenda?: number | null
  unidadeComercial?: string | null
  balancaId?: string | null
}

/** Balança cadastrada (`balancas`) — usada para decodificar códigos de barra pesados. */
export interface BalancaPdv {
  id: string
  descricao?: string | null
  /** Prefixo do código de barras que identifica a balança (ex.: '20', '2'). */
  prefixo?: string | null
  /** 1 = peso/quantidade embutido no código; 2 = valor embutido. */
  tipoValor?: number | null
}

/** Item na comanda do caixa. */
export interface ItemPdv {
  produtoId: string
  produto: ProdutoPdv | null
  quantidadeComercial: number
  valorUnitarioComercial: number
  valorDesconto: number
}

/** Pagamento lançado na venda. */
export interface PagamentoPdv {
  tipoPagamento: TipoPagamento
  valorPagamento: number
  valorTroco: number
}

/** Destinatário/consumidor (opcional na NFC-e). */
export interface DestinatarioPdv {
  pessoaId: string | null
  documentoConsumidor: string
  descricao: string
  enviarNaNfce: boolean
}

/** Documento resultante da emissão, para o cupom final. */
export interface DocumentoEmitido {
  vendaId: string | null
  numero: number | null
  chave: string | null
  status: StatusVenda | null
}
