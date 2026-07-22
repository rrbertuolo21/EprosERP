/**
 * useContaBancaria — IO de contas bancárias (fatia Financeiro).
 *
 * Rotas reais (ContasBancariasController): `GET/POST /api/v1/contas-bancarias`,
 * `GET/PUT/DELETE /api/v1/contas-bancarias/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface ContaBancaria {
  id: string
  empresaId: string
  bancoId: string
  tipoContaBancaria: number
  apelido: string
  titular: string
  agencia: string
  conta: string
  gerente?: string | null
  foneGerente?: string | null
  detalhe?: string | null
  digitoAgencia?: string | null
  dataEncerramento?: string | null
}

export interface ContaBancariaCreateDto {
  empresaId: string
  bancoId: string
  tipoContaBancaria: number
  apelido: string
  titular: string
  agencia: string
  conta: string
  gerente?: string | null
  foneGerente?: string | null
  detalhe?: string | null
  digitoAgencia?: string | null
  dataEncerramento?: string | null
}

export interface ContaBancariaUpdateDto extends ContaBancariaCreateDto {
  id: string
}

export interface ContaBancariaFiltros {
  localizar?: string
}

export function useContaBancaria() {
  const contaBancaria = ref<ContaBancaria | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<ContaBancaria, ContaBancariaFiltros>('/contas-bancarias')

  async function buscarPorId(id: string): Promise<ContaBancaria | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<ContaBancaria>>('/contas-bancarias/{id}', { params: { id } })
      const dados = extrairDados<ContaBancaria>(resposta) ?? null
      contaBancaria.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContaBancaria.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: ContaBancariaCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/contas-bancarias', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContaBancaria.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: ContaBancariaUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/contas-bancarias/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContaBancaria.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/contas-bancarias/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContaBancaria.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function importarOfx(id: string, ofxConteudo: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`/contas-bancarias/{id}/importar-ofx`, { ofxConteudo }, { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContaBancaria.importarOfx]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    contaBancaria,
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
    importarOfx
  }
}
