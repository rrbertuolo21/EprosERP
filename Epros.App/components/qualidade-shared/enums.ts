/**
 * Enums de domínio do módulo QUALIDADE (QLD).
 *
 * O backend (System.Text.Json sem JsonStringEnumConverter) serializa enums como
 * INTEIROS e desserializa os bodies de POST a partir de inteiros — por isso as
 * opções de formulário usam `value` numérico. Já o filtro de status na listagem
 * é parseado por NOME no backend (`Enum.TryParse`), então lá usamos o nome.
 *
 * Espelha `Domain/Enums/QualidadeEnums.cs`. Sem endpoint de enums para estes
 * submódulos novos, montamos as opções localmente a partir do digest.
 */
import type { SelectOption } from '~/composables/useEnum'

/** Converte um mapa { valorNumérico: rótulo } em opções de select (value numérico). */
function paraOpcoesNum(mapa: Record<number, string>): SelectOption[] {
  return Object.entries(mapa).map(([value, label]) => ({ value: Number(value), label }))
}

/** Resolve o rótulo PT-BR de um valor de enum (aceita número ou string). */
export function rotuloEnum(mapa: Record<number, string>, valor: unknown): string {
  if (valor === null || valor === undefined || valor === '') return '—'
  const n = Number(valor)
  if (!Number.isNaN(n) && mapa[n] !== undefined) return mapa[n]
  return String(valor)
}

// ============================================================
// Ciclo de vida do registro (comum a NCR, INS, ACR, ADM, ATR)
// ============================================================
export const STATUS_REGISTRO: Record<number, string> = {
  0: 'Rascunho',
  1: 'Em análise',
  2: 'Ativo',
  3: 'Suspenso',
  4: 'Encerrado',
  5: 'Inativo'
}
/** Nomes do enum (para o filtro `?status=` que o backend parseia por nome). */
export const STATUS_REGISTRO_OPCOES_FILTRO: SelectOption[] = [
  { value: 'Rascunho', label: 'Rascunho' },
  { value: 'EmAnalise', label: 'Em análise' },
  { value: 'Ativo', label: 'Ativo' },
  { value: 'Suspenso', label: 'Suspenso' },
  { value: 'Encerrado', label: 'Encerrado' },
  { value: 'Inativo', label: 'Inativo' }
]

// ============================================================
// QLD-NCR — Não Conformidades
// ============================================================
export const NCR_ORIGEM: Record<number, string> = {
  0: 'Rejeição de lote',
  1: 'Reclamação de cliente',
  2: 'Auditoria',
  3: 'Garantia',
  4: 'Inspeção',
  5: 'Produção',
  6: 'Estoque',
  7: 'Manual',
  8: 'Outro'
}
export const NCR_ORIGEM_OPCOES = paraOpcoesNum(NCR_ORIGEM)

export const NCR_PRIORIDADE: Record<number, string> = {
  0: 'Baixa',
  1: 'Média',
  2: 'Alta',
  3: 'Urgente'
}
export const NCR_PRIORIDADE_OPCOES = paraOpcoesNum(NCR_PRIORIDADE)

export const NCR_ETAPA: Record<number, string> = {
  0: 'Rascunho',
  1: 'Triagem',
  2: 'Investigação',
  3: 'CAPA',
  4: 'Verificação',
  5: 'Encerrada',
  6: 'Cancelada'
}

// ============================================================
// QLD-INS — Planos de Inspeção
// ============================================================
export const CONTEXTO_PLANO: Record<number, string> = {
  0: 'Produto',
  1: 'Processo',
  2: 'Recebimento',
  3: 'Lote',
  4: 'Ordem',
  5: 'Etapa',
  6: 'Manual',
  7: 'Outro'
}
export const CONTEXTO_PLANO_OPCOES = paraOpcoesNum(CONTEXTO_PLANO)

export const TIPO_CARACTERISTICA: Record<number, string> = {
  0: 'Dimensional',
  1: 'Visual',
  2: 'Funcional',
  3: 'Documental',
  4: 'Regulatória',
  5: 'Outro'
}
export const TIPO_CARACTERISTICA_OPCOES = paraOpcoesNum(TIPO_CARACTERISTICA)

// ============================================================
// QLD-ACR — Análise de Aceitação e Rejeição
// ============================================================
export const TIPO_ANALISE_ACR: Record<number, string> = {
  0: 'Recebimento',
  1: 'Processo',
  2: 'Devolução',
  3: 'Manual'
}
export const TIPO_ANALISE_ACR_OPCOES = paraOpcoesNum(TIPO_ANALISE_ACR)

// ============================================================
// QLD-ATR — Gestão de Atributos
// ============================================================
export const TIPO_ATRIBUTO: Record<number, string> = {
  0: 'Comercial',
  1: 'Qualidade',
  2: 'Regulatório'
}
export const TIPO_ATRIBUTO_OPCOES = paraOpcoesNum(TIPO_ATRIBUTO)

export const TIPO_DADO_ATRIBUTO: Record<number, string> = {
  0: 'Texto',
  1: 'Número',
  2: 'Decimal',
  3: 'Data',
  4: 'Lista',
  5: 'Booleano'
}
export const TIPO_DADO_ATRIBUTO_OPCOES = paraOpcoesNum(TIPO_DADO_ATRIBUTO)

export const ESCOPO_ATRIBUTO: Record<number, string> = {
  0: 'Produto',
  1: 'Item',
  2: 'Família',
  3: 'Processo',
  4: 'Plano'
}
export const ESCOPO_ATRIBUTO_OPCOES = paraOpcoesNum(ESCOPO_ATRIBUTO)
