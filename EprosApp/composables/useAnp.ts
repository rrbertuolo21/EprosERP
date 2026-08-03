/**
 * useAnp — IO de códigos ANP (fatia Fiscal).
 *
 * Rotas reais (CodigosAnpController): `GET/POST /api/v1/fiscal/codigos-anp`,
 * `GET/PUT/DELETE /api/v1/fiscal/codigos-anp/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface CodigoAnp {
  id: string
  codigo: string
  descricao: string
  dataInicioVigencia: string
  dataFinalVigencia?: string | null
}

export interface CodigoAnpCreateDto {
  codigo: string
  descricao: string
  dataInicioVigencia: string
  dataFinalVigencia?: string | null
}

export interface CodigoAnpUpdateDto extends CodigoAnpCreateDto {
  id: string
}

export interface CodigoAnpFiltros {
  localizar?: string
}

export function useAnp() {
  const codigoAnp = ref<CodigoAnp | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<CodigoAnp, CodigoAnpFiltros>('/fiscal/codigos-anp')

  async function buscarPorId(id: string): Promise<CodigoAnp | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<CodigoAnp>>('/fiscal/codigos-anp/{id}', { params: { id } })
      const dados = extrairDados<CodigoAnp>(resposta) ?? null
      codigoAnp.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useAnp.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: CodigoAnpCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/fiscal/codigos-anp', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useAnp.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: CodigoAnpUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/fiscal/codigos-anp/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useAnp.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/fiscal/codigos-anp/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useAnp.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    codigoAnp,
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
