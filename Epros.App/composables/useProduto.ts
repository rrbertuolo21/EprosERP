/**
 * useProduto — IO do cadastro de Produto (fatia Estoque).
 *
 * Rotas reais (EstoqueController): `GET /api/v1/estoque/produtos` (listagem paginada,
 * filtros `localizar` + `ativo`), `GET /api/v1/estoque/produtos/{id}`,
 * `POST /api/v1/estoque/produtos`, `PUT/DELETE /api/v1/estoque/produtos/{id}`.
 *
 * Não confundir com `useEstoqueProduto` (posição/saldo de estoque de um produto) nem com
 * `/produtos-especificos` (dados complementares de GLP/GN) — este composable é o CRUD
 * do cadastro principal de Produto.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface Produto {
  id: string
  categoriaId?: string | null
  marcaProdutoId?: string | null
  unidadeMedidaComercialId?: string | null
  codigo: string
  descricao: string
  ean?: string | null
  pesoLiquido: number
  pesoBruto: number
  valorVenda: number
  valorVendaPrazo: number
  valorCompra: number
  tipoProduto?: number | null
  ativo: boolean
  imagem?: string | null
  cestId?: string | null
  codigoAnpId?: string | null
  balancaId?: string | null
  utilizaBalanca: boolean
  codigoProdutoBalanca?: string | null
  ncmId?: string | null
}

export interface ProdutoCreateDto {
  categoriaId?: string | null
  marcaProdutoId?: string | null
  unidadeMedidaComercialId?: string | null
  codigo: string
  descricao: string
  ean?: string | null
  pesoLiquido: number
  pesoBruto: number
  valorVenda: number
  valorVendaPrazo: number
  valorCompra: number
  tipoProduto?: number | null
  ativo: boolean
  imagem: string
  cestId?: string | null
  codigoAnpId?: string | null
  balancaId?: string | null
  utilizaBalanca: boolean
  codigoProdutoBalanca?: string | null
  ncmId?: string | null
}

export interface ProdutoUpdateDto extends Omit<ProdutoCreateDto, 'imagem'> {
  id: string
}

export interface ProdutoFiltros {
  localizar?: string
  ativo?: boolean
}

/** EAN em branco → 'SEM GTIN' (gap L2 — porta `initialProdutoState.ean` do legado). */
function aplicarPadraoEan(ean: string | null | undefined): string {
  const limpo = (ean ?? '').trim()
  return limpo || 'SEM GTIN'
}

export function useProduto() {
  const produto = ref<Produto | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<Produto, ProdutoFiltros>('/estoque/produtos', {
    filtrosIniciais: { ativo: true }
  })

  async function buscarPorId(id: string): Promise<Produto | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<Produto>>('/estoque/produtos/{id}', { params: { id } })
      const dados = extrairDados<Produto>(resposta) ?? null
      produto.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useProduto.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: ProdutoCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/estoque/produtos', { ...payload, ean: aplicarPadraoEan(payload.ean) })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useProduto.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: ProdutoUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/estoque/produtos/{id}', { ...payload, ean: aplicarPadraoEan(payload.ean) }, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useProduto.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/estoque/produtos/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useProduto.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    produto,
    carregando,
    erro,
    itens: lista.itens,
    total: lista.total,
    buscar: lista.buscar,
    filtros: lista.filtros,
    buscarPorId,
    criar,
    atualizar,
    excluir
  }
}
