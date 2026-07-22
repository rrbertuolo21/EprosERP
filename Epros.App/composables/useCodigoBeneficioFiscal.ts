/**
 * useCodigoBeneficioFiscal — IO de códigos de benefício fiscal (fatia Fiscal).
 *
 * Rotas reais (CodigosBeneficiosFiscaisController): `GET/POST /api/v1/codigos-beneficios-fiscais`,
 * `GET/PUT /api/v1/codigos-beneficios-fiscais/{id}` (sem DELETE exposto pelo controller).
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface CodigoBeneficioFiscal {
  id: string
  codigo: string
  uf: number
  descricao?: string | null
  csts: number[]
  csosns: number[]
}

export interface CodigoBeneficioFiscalCreateDto {
  codigo: string
  uf: number
  descricao?: string | null
  csts: number[]
  csosns: number[]
}

export interface CodigoBeneficioFiscalUpdateDto extends CodigoBeneficioFiscalCreateDto {
  id: string
}

export interface CodigoBeneficioFiscalFiltros {
  localizar?: string
}

export function useCodigoBeneficioFiscal() {
  const codigoBeneficioFiscal = ref<CodigoBeneficioFiscal | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<CodigoBeneficioFiscal, CodigoBeneficioFiscalFiltros>('/codigos-beneficios-fiscais')

  async function buscarPorId(id: string): Promise<CodigoBeneficioFiscal | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<CodigoBeneficioFiscal>>('/codigos-beneficios-fiscais/{id}', { params: { id } })
      const dados = extrairDados<CodigoBeneficioFiscal>(resposta) ?? null
      codigoBeneficioFiscal.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCodigoBeneficioFiscal.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: CodigoBeneficioFiscalCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/codigos-beneficios-fiscais', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCodigoBeneficioFiscal.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: CodigoBeneficioFiscalUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/codigos-beneficios-fiscais/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useCodigoBeneficioFiscal.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    codigoBeneficioFiscal,
    carregando,
    erro,
    itens: lista.itens,
    total: lista.total,
    buscar: lista.buscar,
    filtros: lista.filtros,
    buscarPorId,
    criar,
    atualizar
  }
}
