/**
 * useCstIbsCbs — IO de CST IBS/CBS e suas classificações tributárias (fatia Fiscal).
 *
 * Rotas reais (CstsIbsCbsController): `GET/POST /api/v1/fiscal/csts-ibs-cbs`,
 * `GET/PUT/DELETE /api/v1/fiscal/csts-ibs-cbs/{id}`,
 * `POST /api/v1/fiscal/csts-ibs-cbs/{id}/classificacoes-tributarias`,
 * `DELETE /api/v1/fiscal/csts-ibs-cbs/{id}/classificacoes-tributarias/{classificacaoTributariaId}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface ClassificacaoTributariaAnexo {
  nroAnexo: number
  codigo: string
  dataInicioVigencia: string
  dataFimVigencia?: string | null
}

export interface ClassificacaoTributaria {
  id?: string
  codigo: string
  descricao: string
  dataInicioVigencia: string
  dataFimVigencia?: string | null
  indNfe: boolean
  indNfce: boolean
  indCte: boolean
  indCteos: boolean
  indNfse: boolean
  indTribRegular: boolean
  anexos?: ClassificacaoTributariaAnexo[] | null
}

export interface CstIbsCbs {
  id: string
  cst: string
  descricao: string
  dataInicioVigencia: string
  dataFimVigencia?: string | null
  classificacoesTributarias?: ClassificacaoTributaria[] | null
}

export interface CstIbsCbsCreateDto {
  cst: string
  descricao: string
  dataInicioVigencia: string
  dataFimVigencia?: string | null
  classesTributarias?: ClassificacaoTributaria[] | null
}

export interface CstIbsCbsUpdateDto {
  id: string
  cst: string
  descricao: string
  dataInicioVigencia: string
  dataFimVigencia?: string | null
}

export interface CstIbsCbsFiltros {
  localizar?: string
}

export function useCstIbsCbs() {
  const cstIbsCbs = ref<CstIbsCbs | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<CstIbsCbs, CstIbsCbsFiltros>('/fiscal/csts-ibs-cbs')

  async function buscarPorId(id: string): Promise<CstIbsCbs | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<CstIbsCbs>>('/fiscal/csts-ibs-cbs/{id}', { params: { id } })
      const dados = extrairDados<CstIbsCbs>(resposta) ?? null
      cstIbsCbs.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCstIbsCbs.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: CstIbsCbsCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/fiscal/csts-ibs-cbs', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCstIbsCbs.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: CstIbsCbsUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/fiscal/csts-ibs-cbs/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCstIbsCbs.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/fiscal/csts-ibs-cbs/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCstIbsCbs.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function adicionarClassificacaoTributaria(id: string, classe: ClassificacaoTributaria): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/fiscal/csts-ibs-cbs/{id}/classificacoes-tributarias', classe, { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCstIbsCbs.adicionarClassificacaoTributaria]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function removerClassificacaoTributaria(id: string, classificacaoTributariaId: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/fiscal/csts-ibs-cbs/{id}/classificacoes-tributarias/{classificacaoTributariaId}', {
        params: { id, classificacaoTributariaId }
      })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCstIbsCbs.removerClassificacaoTributaria]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    cstIbsCbs,
    carregando,
    erro,
    itens: lista.itens,
    total: lista.total,
    buscar: lista.buscar,
    filtros: lista.filtros,
    buscarPorId,
    criar,
    atualizar,
    excluir,
    adicionarClassificacaoTributaria,
    removerClassificacaoTributaria
  }
}
