/**
 * useCte — CT-e (Conhecimento de Transporte Eletrônico): emissão, cancelamento e listagem.
 *
 * Porta o comportamento de `CteController` do legado (`api/v1/cte`) para o padrão EprosERP
 * (`useApi`/`useApiList`, envelope CommandResult).
 *
 * Contrato de rota:
 *   GET  api/v1/cte?status=&pagina=&tamanhoPagina=
 *   POST api/v1/cte/emitir
 *   POST api/v1/cte/cancelar/{chave}   (body: string com a justificativa)
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export type TipoAmbienteCte = 1 | 2 // 1=Produção, 2=Homologação

/** Corpo de emissão de CT-e. Fiel a `EmitirCteCommand`. */
export interface EmitirCteDto {
  serie: number
  numero: number
  remetenteDocumento: string
  destinatarioDocumento: string
  valorTotal: number
  valorReceber: number
  ambiente?: TipoAmbienteCte
  tipoCte?: number
  modal?: number
}

/** Registro de CT-e listado no histórico. */
export interface Cte {
  id: string
  status: string
  chave?: string | null
  serie: number
  numero: number
  valorTotal: number
  dataEmissao: string
}

export interface CteFiltros {
  status?: string
}

export function useCte() {
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<Cte, CteFiltros>('/cte')

  /** Emite um CT-e de forma síncrona. */
  async function emitir(payload: EmitirCteDto): Promise<unknown> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi.post<CommandResult>('/cte/emitir', payload)
      return extrairDados(resposta)
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCte.emitir]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Cancela um CT-e previamente autorizado pela chave, com justificativa (mínimo 15 caracteres). */
  async function cancelar(chave: string, justificativa: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/cte/cancelar/${chave}`, justificativa)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCte.cancelar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    carregando,
    erro,
    // listagem server-side (ver useApiList)
    itens: lista.itens,
    total: lista.total,
    buscar: lista.buscar,
    filtros: lista.filtros,
    // ações
    emitir,
    cancelar
  }
}
