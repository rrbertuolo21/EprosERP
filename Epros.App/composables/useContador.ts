/**
 * useContador — IO de contadores (fatia Fiscal).
 *
 * Rotas reais (ContadoresController): `GET/POST /api/v1/contadores`,
 * `GET/PUT/DELETE /api/v1/contadores/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface Contador {
  id: string
  razaoSocial?: string | null
  nomeContador?: string | null
  cpf?: string | null
  cnpj?: string | null
  numeroCrc: string
  ufCrc: number
  dataVencimentoCrc: string
  qualificacao?: string | null
  funcao?: string | null
  telefone?: string | null
  email?: string | null
  permissaoTransmissao: number
  ativo: boolean
  logradouro: string
  numero: string
  complemento?: string | null
  bairro: string
  cep: string
  municipioId: number
  uf: number
}

export interface ContadorCreateDto {
  razaoSocial?: string | null
  nomeContador?: string | null
  cpf?: string | null
  cnpj?: string | null
  numeroCrc: string
  ufCrc: number
  dataVencimentoCrc: string
  qualificacao?: string | null
  funcao?: string | null
  telefone?: string | null
  email?: string | null
  permissaoTransmissao: number
  ativo: boolean
  logradouro: string
  numero: string
  complemento?: string | null
  bairro: string
  cep: string
  municipioId: number
  uf: number
}

export interface ContadorUpdateDto extends ContadorCreateDto {
  id: string
}

export interface ContadorFiltros {
  localizar?: string
}

export function useContador() {
  const contador = ref<Contador | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<Contador, ContadorFiltros>('/contadores')

  async function buscarPorId(id: string): Promise<Contador | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<Contador>>('/contadores/{id}', { params: { id } })
      const dados = extrairDados<Contador>(resposta) ?? null
      contador.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContador.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: ContadorCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/contadores', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContador.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: ContadorUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/contadores/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContador.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/contadores/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContador.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    contador,
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
