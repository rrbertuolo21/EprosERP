/**
 * Tipos do formulário de Produto (fatia Cadastro Produtos).
 * Exclusivo desta fatia — não é scaffolding compartilhado.
 */

/** Origem de combustível vinculada ao produto específico (combustíveis). */
export interface ProdutoCombustivelOrigem {
  identificadorOrigemImportacao: number | null
  ufOrigem: string
  valorPercentualUf: number
}

/** Dados específicos (combustíveis) do produto — aba "Combustível". */
export interface ProdutoEspecificoForm {
  ufConsumo: string
  valorPartida: number
  valorPercentualGlpDerivadoPetroleo: number
  valorPercentualGasNaturalImportado: number
  valorPercentualGasNaturalNacional: number
  origens: ProdutoCombustivelOrigem[]
}

/** Vínculo de adicional ao produto — aba "Grade/Adicionais". */
export interface ProdutoAdicionalVinculo {
  id?: number
  adicionaisId: number
  produtoId?: number
  descricao?: string
}

/** Estado do formulário de cadastro/edição de produto. */
export interface ProdutoForm {
  id?: number
  codigo: string
  descricao: string
  ean: string
  ativo: boolean
  marcaProdutoId: number | null
  categoriaId: number | null
  unidadeMedidaComercialId: number | null
  produtoGrupoId?: number | null
  valorCompra: number
  valorVenda: number
  valorVendaPrazo: number
  pesoLiquido: number
  pesoBruto: number
  ncmId: number | null
  ncmDescricao: string
  cestId: number | null
  codigoAnpId: number | null
  utilizaBalanca: boolean
  codigoProdutoBalanca: string
  balancaId: number | null
  imagem: string
  adicionaisProduto: ProdutoAdicionalVinculo[]
  produtoEspecifico: ProdutoEspecificoForm
}

/** Estado inicial do formulário — produto novo. */
export function criarProdutoFormInicial(): ProdutoForm {
  return {
    codigo: '',
    descricao: '',
    ean: '',
    ativo: true,
    marcaProdutoId: null,
    categoriaId: null,
    unidadeMedidaComercialId: null,
    valorCompra: 0,
    valorVenda: 0,
    valorVendaPrazo: 0,
    pesoLiquido: 0,
    pesoBruto: 0,
    ncmId: null,
    ncmDescricao: '',
    cestId: null,
    codigoAnpId: null,
    utilizaBalanca: false,
    codigoProdutoBalanca: '',
    balancaId: null,
    imagem: '',
    adicionaisProduto: [],
    produtoEspecifico: {
      ufConsumo: '',
      valorPartida: 0,
      valorPercentualGlpDerivadoPetroleo: 0,
      valorPercentualGasNaturalImportado: 0,
      valorPercentualGasNaturalNacional: 0,
      origens: []
    }
  }
}

/** Rótulo "código - descrição" para autocomplete/exibição. */
export function formatarLabelProduto(p: { codigo?: string | null; descricao?: string | null; id?: number }): string {
  const codigo = p.codigo?.trim()
  const descricao = p.descricao?.trim()
  if (codigo && descricao) return `${codigo} - ${descricao}`
  if (descricao) return descricao
  if (codigo) return codigo
  return p.id != null ? `Produto #${p.id}` : ''
}
