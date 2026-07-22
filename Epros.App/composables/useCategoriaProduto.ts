/**
 * useCategoriaProduto — IO de categorias de produto (fatia Estoque).
 *
 * Rotas reais (CategoriasProdutosController): `GET/POST /api/v1/categorias-produtos`,
 * `GET/PUT/DELETE /api/v1/categorias-produtos/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface CategoriaProduto {
  id: string
  descricao: string
  produtoGrupoId?: string | null
}

export interface CategoriaProdutoCreateDto {
  descricao: string
  produtoGrupoId?: string | null
}

export interface CategoriaProdutoUpdateDto extends CategoriaProdutoCreateDto {
  id: string
}

export interface CategoriaProdutoFiltros {
  localizar?: string
}

export function useCategoriaProduto() {
  const categoriaProduto = ref<CategoriaProduto | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<CategoriaProduto, CategoriaProdutoFiltros>('/categorias-produtos')

  async function buscarPorId(id: string): Promise<CategoriaProduto | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<CategoriaProduto>>('/categorias-produtos/{id}', { params: { id } })
      const dados = extrairDados<CategoriaProduto>(resposta) ?? null
      categoriaProduto.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCategoriaProduto.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: CategoriaProdutoCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/categorias-produtos', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCategoriaProduto.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: CategoriaProdutoUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/categorias-produtos/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCategoriaProduto.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/categorias-produtos/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCategoriaProduto.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    categoriaProduto,
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
