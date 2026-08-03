/**
 * usePlanoContas — IO do Plano de Contas Financeiro e seus itens (fatia Financeiro).
 *
 * Rotas reais (PlanosDeContasFinanceiroController / PlanosDeContasFinanceiroItensController):
 * `GET/POST /api/v1/plano-de-contas-financeiro`, `GET/PUT/DELETE /api/v1/plano-de-contas-financeiro/{id}`,
 * `GET/POST /api/v1/plano-de-contas-financeiro-itens`, `GET/PUT/DELETE /api/v1/plano-de-contas-financeiro-itens/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface PlanoDeContasFinanceiro {
  id: string
  configuracaoCodigoNaturezaFinanceiraRecebimentoId?: string | null
  configuracaoCodigoNaturezaFinanceiraPagamentoId?: string | null
  descricao: string
  mascara: string
  empresaIds?: string[] | null
}

export interface PlanoDeContasFinanceiroCreateDto {
  configuracaoCodigoNaturezaFinanceiraRecebimentoId?: string | null
  configuracaoCodigoNaturezaFinanceiraPagamentoId?: string | null
  descricao: string
  mascara: string
  empresaIds?: string[] | null
}

export interface PlanoDeContasFinanceiroUpdateDto extends PlanoDeContasFinanceiroCreateDto {
  id: string
}

export interface PlanoDeContasFinanceiroFiltros {
  localizar?: string
}

export interface PlanoDeContasFinanceiroItem {
  id: string
  planoDeContasFinanceiroId: string
  codigo: string
  descricao: string
  tipoDetalhamento: number
  movimentaCaixa: boolean
}

export interface PlanoDeContasFinanceiroItemCreateDto {
  planoDeContasFinanceiroId: string
  codigo: string
  descricao: string
  tipoDetalhamento: number
  movimentaCaixa: boolean
}

export interface PlanoDeContasFinanceiroItemUpdateDto {
  id: string
  codigo: string
  descricao: string
  tipoDetalhamento: number
  movimentaCaixa: boolean
}

export interface PlanoDeContasFinanceiroItemFiltros {
  planoDeContasId?: string
  tipoDetalhamento?: number
}

export function usePlanoContas() {
  const planoContas = ref<PlanoDeContasFinanceiro | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<PlanoDeContasFinanceiro, PlanoDeContasFinanceiroFiltros>('/plano-de-contas-financeiro')

  async function buscarPorId(id: string): Promise<PlanoDeContasFinanceiro | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<PlanoDeContasFinanceiro>>('/plano-de-contas-financeiro/{id}', { params: { id } })
      const dados = extrairDados<PlanoDeContasFinanceiro>(resposta) ?? null
      planoContas.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePlanoContas.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: PlanoDeContasFinanceiroCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/plano-de-contas-financeiro', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePlanoContas.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: PlanoDeContasFinanceiroUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/plano-de-contas-financeiro/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePlanoContas.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/plano-de-contas-financeiro/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePlanoContas.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  // ----- Itens do Plano de Contas -----
  const itemAtual = ref<PlanoDeContasFinanceiroItem | null>(null)
  const listaItens = useApiList<PlanoDeContasFinanceiroItem, PlanoDeContasFinanceiroItemFiltros>('/plano-de-contas-financeiro-itens')

  async function buscarItemPorId(id: string): Promise<PlanoDeContasFinanceiroItem | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<PlanoDeContasFinanceiroItem>>('/plano-de-contas-financeiro-itens/{id}', { params: { id } })
      const dados = extrairDados<PlanoDeContasFinanceiroItem>(resposta) ?? null
      itemAtual.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePlanoContas.buscarItemPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criarItem(payload: PlanoDeContasFinanceiroItemCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/plano-de-contas-financeiro-itens', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePlanoContas.criarItem]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizarItem(payload: PlanoDeContasFinanceiroItemUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/plano-de-contas-financeiro-itens/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePlanoContas.atualizarItem]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluirItem(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/plano-de-contas-financeiro-itens/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[usePlanoContas.excluirItem]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    planoContas,
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
    // itens do plano
    itemAtual,
    itensDoPlano: listaItens.itens,
    totalItens: listaItens.total,
    buscarItens: listaItens.buscar,
    filtrosItens: listaItens.filtros,
    buscarItemPorId,
    criarItem,
    atualizarItem,
    excluirItem
  }
}
