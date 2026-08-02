/**
 * useBanco — IO de bancos (fatia Financeiro).
 *
 * Rotas reais (BancosController): `GET/POST /api/v1/bancos`,
 * `GET/PUT/DELETE /api/v1/bancos/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface Banco {
  id: string
  codigo: string
  descricao: string
}

export interface BancoCreateDto {
  codigo: string
  descricao: string
}

export interface BancoUpdateDto extends BancoCreateDto {
  id: string
}

export interface BancoFiltros {
  localizar?: string
}

export function useBanco() {
  const banco = ref<Banco | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<Banco, BancoFiltros>('/bancos')

  async function buscarPorId(id: string): Promise<Banco | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<Banco>>('/bancos/{id}', { params: { id } })
      const dados = extrairDados<Banco>(resposta) ?? null
      banco.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useBanco.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: BancoCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/bancos', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useBanco.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: BancoUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/bancos/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useBanco.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/bancos/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useBanco.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    banco,
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
