/**
 * useServico — IO de serviços (fatia Fiscal).
 *
 * Rotas reais (ServicosController): `GET/POST /api/v1/servicos`,
 * `GET/PUT/DELETE /api/v1/servicos/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface Servico {
  id: string
  empresaId: string
  unidadeMedidaId: string
  codigoServicoSefazId: string
  codigo: string
  descricao: string
  valor: number
  informacaoAdicional?: string | null
  servicoAtivo: boolean
  cnae: number
  codigoNbs: string
  indicadorIss: boolean
  indicadorIncentivo: boolean
  cstIbsCbs?: string | null
  cClassTrib?: string | null
  aliquotaIss: number
  aliquotaIssRetido: number
  aliquotaIrrfRetido: number
  aliquotaInss: number
  cstPisCofins: number
  aliquotaPis: number
  aliquotaCofins: number
  calcularRetencao: boolean
  anexoSimplesNacional: number
}

export type ServicoCreateDto = Omit<Servico, 'id'>

export interface ServicoUpdateDto extends ServicoCreateDto {
  id: string
}

export interface ServicoFiltros {
  localizar?: string
}

export function useServico() {
  const servico = ref<Servico | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<Servico, ServicoFiltros>('/servicos')

  async function buscarPorId(id: string): Promise<Servico | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<Servico>>('/servicos/{id}', { params: { id } })
      const dados = extrairDados<Servico>(resposta) ?? null
      servico.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useServico.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: ServicoCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/servicos', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useServico.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: ServicoUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/servicos/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useServico.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/servicos/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useServico.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    servico,
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
