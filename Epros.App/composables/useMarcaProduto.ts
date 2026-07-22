/**
 * useMarcaProduto — IO de marcas de produto (fatia Estoque).
 *
 * Rotas reais (MarcasProdutosController): `GET/POST /api/v1/marcas-produtos`,
 * `GET/PUT/DELETE /api/v1/marcas-produtos/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface MarcaProduto {
  id: string
  descricao: string
  produtoGrupoId?: string | null
}

export interface MarcaProdutoCreateDto {
  descricao: string
  produtoGrupoId?: string | null
}

export interface MarcaProdutoUpdateDto extends MarcaProdutoCreateDto {
  id: string
}

export interface MarcaProdutoFiltros {
  localizar?: string
}

export function useMarcaProduto() {
  const marcaProduto = ref<MarcaProduto | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<MarcaProduto, MarcaProdutoFiltros>('/marcas-produtos')

  async function buscarPorId(id: string): Promise<MarcaProduto | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<MarcaProduto>>('/marcas-produtos/{id}', { params: { id } })
      const dados = extrairDados<MarcaProduto>(resposta) ?? null
      marcaProduto.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useMarcaProduto.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: MarcaProdutoCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/marcas-produtos', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useMarcaProduto.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: MarcaProdutoUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/marcas-produtos/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useMarcaProduto.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/marcas-produtos/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useMarcaProduto.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    marcaProduto,
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
