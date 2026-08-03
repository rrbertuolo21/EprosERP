/**
 * useNaturezaFinanceira — IO de configurações de código de natureza financeira (fatia Financeiro).
 *
 * Rotas reais (ConfiguracoesCodigoNaturezaFinanceiraController):
 * `GET/POST /api/v1/configuracao-codigo-naturezas-financeiras`,
 * `GET/PUT/DELETE /api/v1/configuracao-codigo-naturezas-financeiras/{id}`.
 */
import { ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

export interface ConfiguracaoCodigoNaturezaFinanceira {
  id: string
  empresaId: string
  descricao: string
  tipoConfiguracaoNatureza: number
  itemPlanoDeContasFinanceiroDinheiroId?: string | null
  itemPlanoDeContasFinanceiroCartaoChequeId?: string | null
  itemPlanoDeContasFinanceiroCartaoCreditoId?: string | null
  itemPlanoDeContasFinanceiroCartaoDebitoId?: string | null
  itemPlanoDeContasFinanceiroCartaoDaLojaId?: string | null
  itemPlanoDeContasFinanceiroValeAlimentacaoId?: string | null
  itemPlanoDeContasFinanceiroValeRefeicaoId?: string | null
  itemPlanoDeContasFinanceiroValePresenteId?: string | null
  itemPlanoDeContasFinanceiroValeCombustivelId?: string | null
  itemPlanoDeContasFinanceiroDuplicataMercantilId?: string | null
  itemPlanoDeContasFinanceiroBoletoBancarioId?: string | null
  itemPlanoDeContasFinanceiroDepositoBancarioId?: string | null
  itemPlanoDeContasFinanceiroPagamentoInstantaneoPixDinamicoId?: string | null
  itemPlanoDeContasFinanceiroTransferenciaBancariaId?: string | null
  itemPlanoDeContasFinanceiroProgramaDeFidelidadeId?: string | null
  itemPlanoDeContasFinanceiroPagamentoInstantaneoPixEstaticoId?: string | null
  itemPlanoDeContasFinanceiroCreditoEmLojaId?: string | null
  itemPlanoDeContasFinanceiroPagamentoEletronicoNaoInformadoId?: string | null
  itemPlanoDeContasFinanceiroOutrosId?: string | null
  itemPlanoDeContasFinanceiroDescontoId?: string | null
  itemPlanoDeContasFinanceiroAcrescimoId?: string | null
  itemPlanoDeContasFinanceiroJurosId?: string | null
  itemPlanoDeContasFinanceiroMultaId?: string | null
  itemPlanoDeContasFinanceiroTrocoId?: string | null
}

export type ConfiguracaoCodigoNaturezaFinanceiraCreateDto = Omit<ConfiguracaoCodigoNaturezaFinanceira, 'id'>

export interface ConfiguracaoCodigoNaturezaFinanceiraUpdateDto extends ConfiguracaoCodigoNaturezaFinanceiraCreateDto {
  id: string
}

export interface NaturezaFinanceiraFiltros {
  localizar?: string
}

export function useNaturezaFinanceira() {
  const naturezaFinanceira = ref<ConfiguracaoCodigoNaturezaFinanceira | null>(null)
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  const lista = useApiList<ConfiguracaoCodigoNaturezaFinanceira, NaturezaFinanceiraFiltros>('/configuracao-codigo-naturezas-financeiras')

  async function buscarPorId(id: string): Promise<ConfiguracaoCodigoNaturezaFinanceira | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<ConfiguracaoCodigoNaturezaFinanceira>>('/configuracao-codigo-naturezas-financeiras/{id}', { params: { id } })
      const dados = extrairDados<ConfiguracaoCodigoNaturezaFinanceira>(resposta) ?? null
      naturezaFinanceira.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNaturezaFinanceira.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  async function criar(payload: ConfiguracaoCodigoNaturezaFinanceiraCreateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post('/configuracao-codigo-naturezas-financeiras', payload)
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNaturezaFinanceira.criar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function atualizar(payload: ConfiguracaoCodigoNaturezaFinanceiraUpdateDto): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.put('/configuracao-codigo-naturezas-financeiras/{id}', payload, { params: { id: payload.id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNaturezaFinanceira.atualizar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function excluir(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete('/configuracao-codigo-naturezas-financeiras/{id}', { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useNaturezaFinanceira.excluir]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  return {
    naturezaFinanceira,
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
