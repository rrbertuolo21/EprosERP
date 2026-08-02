/**
 * useUnidadeMedida — IO de unidades de medida comercial (fatia Estoque).
 *
 * Rotas reais (UnidadesMedidaComercialController): `GET/POST /api/v1/unidades-de-medidas-comercial`,
 * `GET/PUT/DELETE /api/v1/unidades-de-medidas-comercial/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'
import { useRules } from './useRules'

export interface UnidadeMedidaComercial {
  id: string
  unidadeMedida: string
  descricao: string
  fator: number
  produtoGrupoId?: string | null
}

export interface UnidadeMedidaComercialCreateDto {
  unidadeMedida: string
  descricao: string
  fator: number
  produtoGrupoId?: string | null
}

export interface UnidadeMedidaComercialUpdateDto extends UnidadeMedidaComercialCreateDto {
  id: string
}

export interface UnidadeMedidaComercialFiltros {
  localizar?: string
}

export function useUnidadeMedida() {
  const unidadeMedida = ref<UnidadeMedidaComercial | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<UnidadeMedidaComercial, UnidadeMedidaComercialFiltros>('/unidades-de-medidas-comercial')

  async function buscarPorId(id: string): Promise<UnidadeMedidaComercial | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<UnidadeMedidaComercial>>('/unidades-de-medidas-comercial/{id}', { params: { id } })
      const dados = extrairDados<UnidadeMedidaComercial>(resposta) ?? null
      unidadeMedida.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useUnidadeMedida.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: UnidadeMedidaComercialCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    const erroFator = useRules().fatorMaiorQueZero(payload.fator)
    if (erroFator !== true) {
      erro.value = erroFator
      carregando.value = false
      return false
    }
    try {
      await useApi.post('/unidades-de-medidas-comercial', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useUnidadeMedida.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: UnidadeMedidaComercialUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    const erroFator = useRules().fatorMaiorQueZero(payload.fator)
    if (erroFator !== true) {
      erro.value = erroFator
      carregando.value = false
      return false
    }
    try {
      await useApi.put('/unidades-de-medidas-comercial/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useUnidadeMedida.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/unidades-de-medidas-comercial/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useUnidadeMedida.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    unidadeMedida,
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
