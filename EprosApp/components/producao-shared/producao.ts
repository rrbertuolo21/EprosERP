/**
 * Helpers locais do módulo PRODUÇÃO.
 *
 * Centraliza os enums de status/situação (do backend Epros.Modules.Producao) e utilitários
 * de rótulo/badge usados pelas telas de produção. NÃO é um recurso compartilhado global —
 * vive dentro do espaço do módulo (components/producao-shared) e é importado pelas páginas
 * de `pages/erp/producao/*`.
 *
 * Os enums do backend são serializados ora como número, ora como string; por isso os
 * helpers aceitam ambos (`number | string | null`).
 */
import type { SelectOption } from '~/composables/useEnum'

/** EStatusWorkflowProducao — workflow padrão (estimativa, custo, BOM, planejamento, MRP, ESC). */
export const STATUS_WORKFLOW: Array<{ id: number; nome: string }> = [
  { id: 0, nome: 'Rascunho' },
  { id: 1, nome: 'Em Análise' },
  { id: 2, nome: 'Ativo' },
  { id: 3, nome: 'Inativo' },
  { id: 4, nome: 'Encerrado' }
]

/** EStatusOrdemMes — status da ordem de manufatura (MES). */
export const STATUS_MES: Array<{ id: number; nome: string }> = [
  { id: 0, nome: 'Rascunho' },
  { id: 1, nome: 'Em Análise' },
  { id: 2, nome: 'Ativo' },
  { id: 3, nome: 'Pendente' },
  { id: 4, nome: 'Finalizado' },
  { id: 5, nome: 'Inativo' },
  { id: 6, nome: 'Encerrado' }
]

/** ESituacaoFichaProducao — situação da ficha de produção (ordem de serviço). */
export const SITUACAO_FICHA: Array<{ id: number; nome: string }> = [
  { id: 1, nome: 'Aguardando Pagamento' },
  { id: 2, nome: 'Em Produção' },
  { id: 3, nome: 'Concluído' }
]

/** ELogomarcaFichaProducao. */
export const LOGOMARCA_FICHA: Array<{ id: number; nome: string }> = [
  { id: 1, nome: 'Sem Logo' },
  { id: 2, nome: 'Bordada' },
  { id: 3, nome: 'Carimbada' }
]

/** Status textual (string) da OrdemProducao simples. */
export const STATUS_ORDEM_SIMPLES: string[] = ['Pendente', 'EmProducao', 'Encerrada', 'Cancelada']

function paraOpcoes(itens: Array<{ id: number; nome: string }>): SelectOption[] {
  return itens.map((i) => ({ label: i.nome, value: i.id }))
}

/**
 * Opções de filtro por status (o backend faz Enum.TryParse por NOME do identificador).
 * value = identificador do enum no C# (sem acento/espaço); label = rótulo PT-BR.
 */
export const filtroStatusWorkflow: SelectOption[] = [
  { label: 'Rascunho', value: 'Rascunho' },
  { label: 'Em Análise', value: 'EmAnalise' },
  { label: 'Ativo', value: 'Ativo' },
  { label: 'Inativo', value: 'Inativo' },
  { label: 'Encerrado', value: 'Encerrado' }
]

export const filtroStatusMes: SelectOption[] = [
  { label: 'Rascunho', value: 'Rascunho' },
  { label: 'Em Análise', value: 'EmAnalise' },
  { label: 'Ativo', value: 'Ativo' },
  { label: 'Pendente', value: 'Pendente' },
  { label: 'Finalizado', value: 'Finalizado' },
  { label: 'Inativo', value: 'Inativo' },
  { label: 'Encerrado', value: 'Encerrado' }
]

export const opcoesStatusWorkflow: SelectOption[] = paraOpcoes(STATUS_WORKFLOW)
export const opcoesStatusMes: SelectOption[] = paraOpcoes(STATUS_MES)
export const opcoesSituacaoFicha: SelectOption[] = paraOpcoes(SITUACAO_FICHA)
export const opcoesLogomarca: SelectOption[] = paraOpcoes(LOGOMARCA_FICHA)
export const opcoesStatusOrdemSimples: SelectOption[] = STATUS_ORDEM_SIMPLES.map((s) => ({ label: rotularOrdemSimples(s), value: s }))

/** Converte um valor de enum (número OU nome) no rótulo PT-BR, usando a tabela informada. */
function rotular(valor: number | string | null | undefined, tabela: Array<{ id: number; nome: string }>): string {
  if (valor === null || valor === undefined || valor === '') return '—'
  // Numérico (ou string numérica): casa pelo id.
  const comoNumero = typeof valor === 'number' ? valor : Number(valor)
  if (!Number.isNaN(comoNumero) && String(comoNumero) === String(valor)) {
    return tabela.find((t) => t.id === comoNumero)?.nome ?? String(valor)
  }
  // String com nome do enum (ex.: "EmAnalise"): casa por nome sem espaços.
  const alvo = String(valor).replace(/\s/g, '').toLowerCase()
  return tabela.find((t) => t.nome.replace(/\s/g, '').toLowerCase() === alvo)?.nome ?? String(valor)
}

export function rotuloStatusWorkflow(v: number | string | null | undefined): string {
  return rotular(v, STATUS_WORKFLOW)
}
export function rotuloStatusMes(v: number | string | null | undefined): string {
  return rotular(v, STATUS_MES)
}
export function rotuloSituacaoFicha(v: number | string | null | undefined): string {
  return rotular(v, SITUACAO_FICHA)
}
export function rotuloLogomarca(v: number | string | null | undefined): string {
  return rotular(v, LOGOMARCA_FICHA)
}
export function rotularOrdemSimples(v: string | null | undefined): string {
  const mapa: Record<string, string> = {
    Pendente: 'Pendente',
    EmProducao: 'Em Produção',
    Encerrada: 'Encerrada',
    Cancelada: 'Cancelada'
  }
  return v ? mapa[v] ?? v : '—'
}

/**
 * Classe de badge a partir do rótulo de status.
 * Usa as variantes de badge REAIS do tema (assets/css/main.css):
 * badge-paga (verde), badge-pendente (âmbar), badge-atrasada (vermelho), badge-cancelada (cinza).
 */
export function classeBadgeStatus(rotulo: string): string {
  const r = rotulo.toLowerCase()
  if (r.includes('inativo') || r.includes('rejeit') || r.includes('cancel')) return 'badge-atrasada'
  if (r.includes('ativo')) return 'badge-paga'
  if (r.includes('concluí') || r.includes('finaliz') || r.includes('produzido') || r.includes('entregue') || r.includes('encerrad')) return 'badge-paga'
  if (r.includes('análise') || r.includes('pendente') || r.includes('aguardando') || r.includes('produção') || r.includes('rascunho')) return 'badge-pendente'
  return 'badge-cancelada'
}

/**
 * Formata data-hora ISO para exibição PT-BR curta (dd/mm/aaaa). Vazio → '—'.
 */
export function formatarData(iso: string | null | undefined, comHora = false): string {
  if (!iso) return '—'
  const d = new Date(iso)
  if (Number.isNaN(d.getTime())) return String(iso)
  return comHora ? d.toLocaleString('pt-BR') : d.toLocaleDateString('pt-BR')
}

/** Formata número decimal (moeda BRL) para exibição. */
export function formatarMoeda(v: number | null | undefined): string {
  if (v === null || v === undefined) return '—'
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(v)
}

/** Formata quantidade decimal. */
export function formatarQuantidade(v: number | null | undefined, decimais = 3): string {
  if (v === null || v === undefined) return '—'
  return new Intl.NumberFormat('pt-BR', { minimumFractionDigits: 0, maximumFractionDigits: decimais }).format(v)
}

/** Ações de workflow disponíveis conforme o status atual (EStatusWorkflowProducao). */
export interface AcaoWorkflow {
  chave: string
  rotulo: string
  danger?: boolean
  /** Se true, exige informar um motivo (rejeitar). */
  pedeMotivo?: boolean
}

/**
 * Deriva as ações de workflow ofertadas para um dado status.
 * O backend valida a transição; aqui só orientamos o operador escondendo o que é claramente inválido.
 */
export function acoesWorkflow(status: number | string | null | undefined): AcaoWorkflow[] {
  const rotulo = rotuloStatusWorkflow(status)
  const acoes: AcaoWorkflow[] = []
  switch (rotulo) {
    case 'Rascunho':
      acoes.push({ chave: 'submeter', rotulo: 'Submeter' })
      acoes.push({ chave: 'inativar', rotulo: 'Inativar', danger: true })
      break
    case 'Em Análise':
      acoes.push({ chave: 'aprovar', rotulo: 'Aprovar' })
      acoes.push({ chave: 'rejeitar', rotulo: 'Rejeitar', danger: true, pedeMotivo: true })
      break
    case 'Ativo':
      acoes.push({ chave: 'encerrar', rotulo: 'Encerrar' })
      acoes.push({ chave: 'inativar', rotulo: 'Inativar', danger: true })
      break
    case 'Inativo':
      acoes.push({ chave: 'reativar', rotulo: 'Reativar' })
      break
    default:
      break
  }
  return acoes
}
