/**
 * Tipos locais da fatia Compras — entrada de mercadorias.
 * Espelham o contrato de `LancarCompraCommand` / `ItemCompraInput` do backend
 * (Epros.Modules.Estoque.Application.Commands.LancarCompraCommand).
 */

/** Item de uma nota de entrada, no formato usado pela tela e pelos diálogos. */
export interface ItemEntrada {
  /** Índice na lista de itens da nota; -1 quando ainda não incluído. */
  index: number
  /** Id do produto no cadastro (estoque-produtos), quando selecionado da base. */
  produtoId?: number
  /** Código do produto (SKU) — obrigatório no payload. */
  sku: string
  /** Descrição do produto — obrigatória no payload. */
  nomeProduto: string
  ncm?: string
  unidade?: string
  codigoEan?: string
  cfop?: string
  quantidade: number
  precoUnitario: number
  valorDesconto?: number
  /** Valor de ICMS do item (mapeado para `ValorIms` no comando). */
  valorIcms?: number
  /** Valor de IPI do item. */
  valorIpi?: number
  /** Total do item (quantidade × unitário − desconto) — apenas exibição/somatório. */
  totalItem: number
}

/** Cabeçalho + itens da nota de entrada (estado da tela). */
export interface EntradaMercadorias {
  id?: string
  fornecedorCnpj: string
  fornecedorNome: string
  fornecedorId?: number
  numeroNota: string
  chaveAcesso: string
  dataEmissao: string
  valorTotal: number
  itens: ItemEntrada[]
}

/** Payload enviado ao endpoint `compras/lancar` (record LancarCompraCommand). */
export interface LancarCompraPayload {
  fornecedorCnpj: string
  fornecedorNome: string
  numeroNota: string
  chaveAcesso: string
  valorTotal: number
  dataEmissao: string
  itens: Array<{
    sku: string
    nomeProduto: string
    quantidade: number
    precoUnitario: number
    valorIms: number
    valorIpi: number
  }>
}
