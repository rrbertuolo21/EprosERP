/**
 * useContasAReceber — IO e estado de Contas a Receber (fatia Financeiro).
 *
 * Porta o comportamento espalhado em vários composables do legado
 * (`composables/financeiro/useContasAReceberFetch/FetchId/Post/Put/Delete/TotaisFetch.ts`)
 * para um único composable de domínio, seguindo o padrão do EprosERP (`useApi`/`useApiList`,
 * envelope `CommandResult`).
 *
 * Contrato de rota (fixado no plano de equalização):
 *   GET  api/v1/financeiro/contas-receber/totais   → cards de resumo (vencendo hoje/vencido/a vencer)
 *   GET  api/v1/financeiro/contas-receber          → listagem paginada
 *   GET  api/v1/financeiro/contas-receber/{id}     → detalhe
 *   POST api/v1/financeiro/contas-receber          → criação
 *   PUT  api/v1/financeiro/contas-receber/{id}     → atualização
 *   DELETE api/v1/financeiro/contas-receber/{id}   → exclusão
 *   POST api/v1/financeiro/contas-receber/{id}/baixar → baixa (padrão de baixa do contrato)
 *
 * Fidelidade ao legado: mantém os campos de título (valor, desconto/multa/juros/acréscimo
 * iniciais e totais, situação, parcela, fato gerador) e a mecânica de itens de recebimento
 * (baixa parcial) somando `valorTotalRecebido`.
 */
import { computed, ref } from 'vue'
import { useApi, extrairDados, type CommandResult } from './useApi'
import { useApiList, obterMensagemErro } from './useApiList'

/** Situação do título (mesmos códigos do legado `SITUACAO_CONTAS_A_RECEBER`). */
export enum SituacaoContaReceber {
  Aberta = 1,
  Pago = 2,
  PagoParcial = 3,
  Cancelado = 4
}

export const SITUACAO_CONTAS_A_RECEBER_LABEL: Record<SituacaoContaReceber, { texto: string; cor: string }> = {
  [SituacaoContaReceber.Aberta]: { texto: 'Aberta', cor: 'warning' },
  [SituacaoContaReceber.Pago]: { texto: 'Pago', cor: 'success' },
  [SituacaoContaReceber.PagoParcial]: { texto: 'Pago Parcial', cor: 'info' },
  [SituacaoContaReceber.Cancelado]: { texto: 'Cancelado', cor: 'error' }
}

/** Item de baixa/recebimento de uma conta a receber. */
export interface ContasAReceberItem {
  id: string | null
  contasAReceberId: string
  contaBancariaId: string | null
  planoDeContasFinanceiroItemId: string | null
  dataRecebimento: string
  tipoPagamento: number
  valorParcela: number
  valorPago: number
  valorDesconto: number
  valorAcrescimo: number
  valorJuros: number
  valorMulta: number
  valorTroco: number
  valorAReceber: number
}

export interface ContasAReceber {
  id: string | null
  pessoaId: string | null
  planoDeContasFinanceiroItemId: string | null
  nomePessoa: string
  documento: string
  detalhamento: string
  situacao: SituacaoContaReceber
  dataEmissao: string
  dataVencimento: string
  dataBaixa: string | null
  valorTitulo: number
  valorInicialDesconto: number
  valorInicialMulta: number
  valorInicialJuros: number
  valorInicialAcrescimo: number
  valorTotalAReceberTitulo: number
  valorTotalAcrescimo: number
  valorTotalDesconto: number
  valorTotalJuros: number
  valorTotalMulta: number
  valorTotalRecebido: number
  valorTotalTroco: number
  numeroParcela: number
  justificativaCancelamento: string | null
  contasAReceberItens: ContasAReceberItem[]
  fatoGeradorFinanceiro?: {
    id: string | null
    descricao: string
    origem: number
    vendaId: string | null
    compraId: string | null
  }
}

export interface ContasAReceberTotais {
  valorVencendoHoje: number
  valorVencido: number
  valorAVencer: number
}

export interface ContasAReceberFiltros {
  statusVencimento?: number
  dataVencimento?: string
  documento?: string
  natureza?: string
  nomeCliente?: string
  origem?: number
  situacao?: SituacaoContaReceber
  valorTitulo?: number
}

/** Estado inicial de um título novo (mesmos defaults do legado `useContasAReceberFetchId`). */
export function contasAReceberFormInicial(): ContasAReceber {
  return {
    id: null,
    pessoaId: null,
    planoDeContasFinanceiroItemId: null,
    nomePessoa: '',
    documento: '',
    detalhamento: '',
    situacao: SituacaoContaReceber.Aberta,
    dataEmissao: new Date().toISOString(),
    dataVencimento: new Date().toISOString(),
    dataBaixa: null,
    valorTitulo: 0,
    valorInicialDesconto: 0,
    valorInicialMulta: 0,
    valorInicialJuros: 0,
    valorInicialAcrescimo: 0,
    valorTotalAReceberTitulo: 0,
    valorTotalAcrescimo: 0,
    valorTotalDesconto: 0,
    valorTotalJuros: 0,
    valorTotalMulta: 0,
    valorTotalRecebido: 0,
    valorTotalTroco: 0,
    numeroParcela: 1,
    justificativaCancelamento: null,
    contasAReceberItens: []
  }
}

const ROTA_BASE = '/financeiro/contas-receber'

export function useContasAReceber() {
  const registro = ref<ContasAReceber>(contasAReceberFormInicial())
  const carregando = ref(false)
  const erro = ref<string | null>(null)
  const totais = ref<ContasAReceberTotais | null>(null)

  const lista = useApiList<ContasAReceber, ContasAReceberFiltros>(ROTA_BASE)

  const valorTituloLiquido = computed(() => {
    const t = registro.value
    return arred(t.valorTitulo - t.valorInicialDesconto + t.valorInicialMulta + t.valorInicialJuros + t.valorInicialAcrescimo)
  })

  const situacaoInfo = computed(() => SITUACAO_CONTAS_A_RECEBER_LABEL[registro.value.situacao] ?? { texto: 'Aberta', cor: 'warning' })

  /** Busca os totais de resumo (cards) — rota fixa do contrato de equalização. */
  async function buscarTotais(): Promise<ContasAReceberTotais | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<ContasAReceberTotais>>(`${ROTA_BASE}/totais`)
      const dados = extrairDados<ContasAReceberTotais>(resposta) ?? null
      totais.value = dados
      return dados
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContasAReceber.buscarTotais]', e)
      return null
    } finally {
      carregando.value = false
    }
  }

  async function buscarPorId(id: string): Promise<ContasAReceber | null> {
    carregando.value = true
    erro.value = null
    try {
      const resposta = await useApi<CommandResult<ContasAReceber>>(`${ROTA_BASE}/{id}`, { params: { id } })
      const dados = extrairDados<ContasAReceber>(resposta)
      registro.value = dados ? { ...contasAReceberFormInicial(), ...dados } : contasAReceberFormInicial()
      return dados ?? null
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContasAReceber.buscarPorId]', e)
      throw e
    } finally {
      carregando.value = false
    }
  }

  /** Cria ou atualiza (PUT quando já há id) o título atual em `registro`. */
  async function salvar(): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      if (registro.value.id) {
        await useApi.put(`${ROTA_BASE}/{id}`, registro.value, { params: { id: registro.value.id } })
      } else {
        await useApi.post(ROTA_BASE, registro.value)
      }
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContasAReceber.salvar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  async function remover(id: string): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.delete(`${ROTA_BASE}/{id}`, { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContasAReceber.remover]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Baixa (recebimento, total ou parcial) — padrão de baixa do contrato de rotas. */
  async function baixar(id: string, item: Omit<ContasAReceberItem, 'id' | 'contasAReceberId'>): Promise<boolean> {
    carregando.value = true
    erro.value = null
    try {
      await useApi.post(`${ROTA_BASE}/{id}/baixar`, item, { params: { id } })
      return true
    } catch (e) {
      erro.value = obterMensagemErro(e)
      console.error('[useContasAReceber.baixar]', e)
      return false
    } finally {
      carregando.value = false
    }
  }

  /** Adiciona/atualiza um item de recebimento no registro em edição e refaz o total recebido (client-side, antes de persistir). */
  function definirItemRecebimento(item: ContasAReceberItem): void {
    const idx = registro.value.contasAReceberItens.findIndex((i) => i.id === item.id)
    if (idx !== -1) {
      registro.value.contasAReceberItens.splice(idx, 1, item)
    } else {
      registro.value.contasAReceberItens.push(item)
    }
    registro.value.valorTotalRecebido = arred(
      registro.value.contasAReceberItens.reduce((acc, i) => acc + (i.valorPago || 0), 0)
    )
  }

  function removerItemRecebimento(index: number): void {
    registro.value.contasAReceberItens.splice(index, 1)
    registro.value.valorTotalRecebido = arred(
      registro.value.contasAReceberItens.reduce((acc, i) => acc + (i.valorPago || 0), 0)
    )
  }

  function novoRegistro(): void {
    registro.value = contasAReceberFormInicial()
  }

  return {
    // detalhe
    registro,
    carregando,
    erro,
    valorTituloLiquido,
    situacaoInfo,
    buscarPorId,
    salvar,
    remover,
    baixar,
    definirItemRecebimento,
    removerItemRecebimento,
    novoRegistro,
    // totais (cards)
    totais,
    buscarTotais,
    // listagem server-side (ver useApiList)
    itens: lista.itens,
    total: lista.total,
    pagina: lista.pagina,
    tamanhoPagina: lista.tamanhoPagina,
    filtros: lista.filtros,
    buscar: lista.buscar,
    aplicarFiltros: lista.aplicarFiltros
  }
}

/** Arredonda para 2 casas decimais evitando ruído de ponto flutuante. */
function arred(v: number): number {
  return Math.round((v + Number.EPSILON) * 100) / 100
}
