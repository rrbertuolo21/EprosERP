/**
 * useAtivoFixo — IO do módulo Ativos Fixos / Imobilizado (Financeiro).
 *
 * Contrato real (AtivosFixosController, rota `api/v1/ativos-fixos`):
 *   GET    /ativos-fixos?status&grupoBemId&pagina&tamanhoPagina   (lista paginada)
 *   GET    /ativos-fixos/{id}
 *   POST   /ativos-fixos                         (CriarAtivoFixoCommand)
 *   PUT    /ativos-fixos/{id}                    (AtualizarAtivoFixoCommand)
 *   POST   /ativos-fixos/{id}/baixar             (BaixarAtivoFixoCommand)
 *   GET/POST /ativos-fixos/{id}/depreciacoes
 *   GET/POST /ativos-fixos/{id}/movimentacoes
 *   GET/POST /ativos-fixos/grupos  · PUT /ativos-fixos/grupos/{id}
 */
import { ref } from 'vue'
import { useApi, extrairDados, extrairLista, type CommandResult } from './useApi'
import { obterMensagemErro } from './useApiList'

export interface AtivoFixo {
  id: string
  descricao: string
  nome?: string | null
  valorCompra: number
  valorOriginal?: number | null
  dataAquisicao: string
  grupoBemId?: string | null
  grupoNome?: string | null
  numeroSerie?: string | null
  numeroNotaFiscal?: string | null
  chaveNfe?: string | null
  deprecia: boolean
  tipoDepreciacao?: number | null
  taxaAnual?: number | null
  taxaMensal?: number | null
  valorDepreciadoAcumulado?: number | null
  valorContabil?: number | null
  status: number
}

export interface AtivoFixoPayload {
  descricao: string
  valorCompra: number
  dataAquisicao: string
  grupoBemId?: string | null
  numeroNb?: string | null
  nome?: string | null
  numeroSerie?: string | null
  funcao?: string | null
  numeroNotaFiscal?: string | null
  chaveNfe?: string | null
  valorOriginal?: number | null
  deprecia: boolean
  tipoDepreciacao?: number | null
  taxaAnual?: number | null
  taxaMensal?: number | null
}

export interface GrupoBem {
  id: string
  codigo?: string | null
  nome: string
  contaAtivoId?: string | null
  contaDepreciacaoId?: string | null
  contaBaixaId?: string | null
}

export interface DepreciacaoAtivo {
  id: string
  competencia: string
  valor: number
  metodoDepreciacao?: string | null
  taxaAplicada?: number | null
  criadoEm?: string | null
}

export interface MovimentacaoAtivo {
  id: string
  tipoMovimentacao: number
  dataMovimentacao: string
  valor?: number | null
  observacao?: string | null
}

export function useAtivoFixo() {
  const carregando = ref(false)
  const erro = ref<string | null>(null)

  async function listar(query: Record<string, unknown> = {}): Promise<{ itens: AtivoFixo[]; total: number }> {
    const resp = await useApi<CommandResult<unknown>>('/ativos-fixos', { query })
    const d = extrairDados<{ itens?: AtivoFixo[]; total?: number }>(resp)
    const itens = Array.isArray(d) ? (d as AtivoFixo[]) : d?.itens ?? []
    const total = (d && !Array.isArray(d) ? d.total : undefined) ?? (resp as { totalRegistros?: number } | null)?.totalRegistros ?? itens.length
    return { itens, total }
  }

  async function obter(id: string): Promise<AtivoFixo | null> {
    const resp = await useApi<CommandResult<AtivoFixo>>('/ativos-fixos/{id}', { params: { id } })
    return extrairDados<AtivoFixo>(resp) ?? null
  }

  async function criar(payload: AtivoFixoPayload): Promise<boolean> {
    carregando.value = true; erro.value = null
    try { await useApi.post('/ativos-fixos', payload); return true }
    catch (e) { erro.value = obterMensagemErro(e); return false }
    finally { carregando.value = false }
  }

  async function atualizar(id: string, payload: AtivoFixoPayload): Promise<boolean> {
    carregando.value = true; erro.value = null
    try { await useApi.put('/ativos-fixos/{id}', { id, ...payload }, { params: { id } }); return true }
    catch (e) { erro.value = obterMensagemErro(e); return false }
    finally { carregando.value = false }
  }

  async function baixar(id: string, dados: { dataBaixa: string; valorBaixa?: number | null; observacao?: string | null }): Promise<boolean> {
    carregando.value = true; erro.value = null
    try { await useApi.post('/ativos-fixos/{id}/baixar', { id, ...dados }, { params: { id } }); return true }
    catch (e) { erro.value = obterMensagemErro(e); return false }
    finally { carregando.value = false }
  }

  async function listarGrupos(): Promise<GrupoBem[]> {
    const resp = await useApi<CommandResult<unknown>>('/ativos-fixos/grupos', { query: { tamanhoPagina: 200 } })
    return extrairLista<GrupoBem>(resp)
  }

  async function criarGrupo(g: { codigo?: string | null; nome: string }): Promise<boolean> {
    try { await useApi.post('/ativos-fixos/grupos', g); return true }
    catch (e) { erro.value = obterMensagemErro(e); return false }
  }

  async function listarDepreciacoes(id: string): Promise<DepreciacaoAtivo[]> {
    const resp = await useApi<CommandResult<unknown>>('/ativos-fixos/{id}/depreciacoes', { params: { id } })
    return extrairLista<DepreciacaoAtivo>(resp)
  }

  async function registrarDepreciacao(id: string, dados: { competencia: string; valor: number; metodoDepreciacao?: string | null; taxaAplicada?: number | null }): Promise<boolean> {
    try { await useApi.post('/ativos-fixos/{id}/depreciacoes', { ativoId: id, ...dados }, { params: { id } }); return true }
    catch (e) { erro.value = obterMensagemErro(e); return false }
  }

  async function listarMovimentacoes(id: string): Promise<MovimentacaoAtivo[]> {
    const resp = await useApi<CommandResult<unknown>>('/ativos-fixos/{id}/movimentacoes', { params: { id } })
    return extrairLista<MovimentacaoAtivo>(resp)
  }

  return {
    carregando, erro,
    listar, obter, criar, atualizar, baixar,
    listarGrupos, criarGrupo,
    listarDepreciacoes, registrarDepreciacao, listarMovimentacoes
  }
}

export const STATUS_ATIVO_FIXO: Record<number, string> = {
  0: 'Ativo', 1: 'Baixado', 2: 'Totalmente Depreciado', 3: 'Excluído'
}
export const TIPO_DEPRECIACAO_ATIVO: { label: string; value: number }[] = [
  { label: 'Linear (cotas constantes)', value: 0 },
  { label: 'Acelerada', value: 1 },
  { label: 'Saldos decrescentes', value: 2 },
  { label: 'Soma dos dígitos (SYD)', value: 3 }
]
