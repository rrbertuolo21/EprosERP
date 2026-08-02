/**
 * useEstoqueProduto — IO do saldo/posição de estoque por produto (fatia Estoque).
 *
 * Porta `composables/estoque/useEstoqueProduto.ts` do legado: listagem paginada, busca por
 * id e CRUD do registro de estoque de um produto (saldo, mínimo, máximo, reservado, tipo de
 * custeio). Usa exclusivamente `useApi`/`useApiList` (padrão CommandResult do EprosERP),
 * substituindo o `useApiFetch`/`$apiFetch` gerado por OpenAPI do legado.
 *
 * Contrato de rota (igual ao backend): `GET api/v1/estoque/produtos` para o catálogo/posição
 * de estoque — listagem no formato `{ total, pagina, itens }`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export type TipoCusteioEstoque = 0 | 1 | 2 // 0=CustoMedio, 1=PEPS, 2=UEPS

export const TIPO_CUSTEIO_ESTOQUE_OPTIONS: { label: string; value: TipoCusteioEstoque }[] = [
  { label: 'Custo médio', value: 0 },
  { label: 'PEPS (Primeiro que entra primeiro que sai)', value: 1 },
  { label: 'UEPS (Último que entra primeiro que sai)', value: 2 }
]

export function labelTipoCusteioEstoque(valor?: number | null): string {
  if (valor == null) return '-'
  return TIPO_CUSTEIO_ESTOQUE_OPTIONS.find((o) => o.value === valor)?.label ?? String(valor)
}

/** Registro de posição de estoque de um produto, conforme retornado pela API. */
export interface EstoqueProduto {
  id: string
  empresaId: string
  produtoId: string
  quantidadeSaldoEstoque: number
  quantidadeEstoqueMinimo: number
  quantidadeEstoqueMaximo: number
  quantidadeEstoqueReservado: number
  valorSaldo: number
  valorCustoMedio: number | null
  tipoCusteioEstoque: TipoCusteioEstoque
}

/** Corpo de criação (produtoId + parâmetros iniciais de estoque). */
export interface EstoqueProdutoCreateDto {
  produtoId: string
  quantidadeSaldoEstoque: number
  quantidadeEstoqueMinimo: number
  quantidadeEstoqueMaximo: number
  quantidadeEstoqueReservado: number
  valorSaldo: number
  tipoCusteioEstoque: TipoCusteioEstoque
}

export interface EstoqueProdutoUpdateDto extends EstoqueProdutoCreateDto {
  id: string
}

export interface EstoqueProdutoFiltros {
  produtoId?: string
  busca?: string
}

export function useEstoqueProduto() {
  const estoqueProduto = ref<EstoqueProduto | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<EstoqueProduto, EstoqueProdutoFiltros>('/estoque/produtos')

  async function buscarPorId(id: string): Promise<EstoqueProduto | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<EstoqueProduto>>('/estoque/produtos/{id}', { params: { id } })
      const dados = extrairDados<EstoqueProduto>(resposta) ?? null
      estoqueProduto.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useEstoqueProduto.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: EstoqueProdutoCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/estoque/produtos', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useEstoqueProduto.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: EstoqueProdutoUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put(`/estoque/produtos/{id}`, payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useEstoqueProduto.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function remover(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete(`/estoque/produtos/{id}`, { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useEstoqueProduto.remover]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    estoqueProduto,
    carregando,
    erro,
    // listagem server-side (ver useApiList)
    itens: lista.itens,
    total: lista.total,
    buscar: lista.buscar,
    filtros: lista.filtros,
    // CRUD
    buscarPorId,
    criar,
    atualizar,
    remover
  }
}
