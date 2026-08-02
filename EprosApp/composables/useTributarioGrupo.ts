/**
 * useTributarioGrupo — IO de grupos tributários (fatia Fiscal).
 *
 * Rotas reais (TributarioGruposController): `GET/POST /api/v1/fiscal/tributario-grupos`,
 * `GET/PUT/DELETE /api/v1/fiscal/tributario-grupos/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface TributarioGrupo {
  id: string
  descricao: string
}

export interface TributarioGrupoCreateDto {
  descricao: string
}

export interface TributarioGrupoUpdateDto extends TributarioGrupoCreateDto {
  id: string
}

export interface TributarioGrupoFiltros {
  localizar?: string
}

export function useTributarioGrupo() {
  const tributarioGrupo = ref<TributarioGrupo | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<TributarioGrupo, TributarioGrupoFiltros>('/fiscal/tributario-grupos')

  async function buscarPorId(id: string): Promise<TributarioGrupo | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<TributarioGrupo>>('/fiscal/tributario-grupos/{id}', { params: { id } })
      const dados = extrairDados<TributarioGrupo>(resposta) ?? null
      tributarioGrupo.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useTributarioGrupo.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: TributarioGrupoCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/fiscal/tributario-grupos', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useTributarioGrupo.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: TributarioGrupoUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/fiscal/tributario-grupos/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useTributarioGrupo.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/fiscal/tributario-grupos/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useTributarioGrupo.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    tributarioGrupo,
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
