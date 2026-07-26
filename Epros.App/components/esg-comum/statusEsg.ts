/**
 * Utilitário local do módulo ESG (não compartilhado).
 *
 * Mapeia o enum de workflow `EStatusWorkflowEsg` do backend (Rascunho→Inativo) e o
 * escopo GHG (1/2/3) para rótulos/opções de select. Usado apenas pelas telas ESG.
 * A API serializa o enum como número; alguns endpoints podem devolver o nome — o
 * formatador aceita ambos.
 */
import type { SelectOption } from '~/composables/useEnum'

/** Opções de status de workflow ESG (somente leitura nas telas — não vai no POST). */
export const statusEsgOptions: SelectOption[] = [
  { label: 'Rascunho', value: 0 },
  { label: 'Em Análise', value: 1 },
  { label: 'Ativo', value: 2 },
  { label: 'Suspenso', value: 3 },
  { label: 'Encerrado', value: 4 },
  { label: 'Inativo', value: 5 }
]

const rotulosStatus: Record<string, string> = {
  '0': 'Rascunho',
  '1': 'Em Análise',
  '2': 'Ativo',
  '3': 'Suspenso',
  '4': 'Encerrado',
  '5': 'Inativo',
  Rascunho: 'Rascunho',
  EmAnalise: 'Em Análise',
  Ativo: 'Ativo',
  Suspenso: 'Suspenso',
  Encerrado: 'Encerrado',
  Inativo: 'Inativo'
}

/** Converte o valor bruto do status (número ou string) em rótulo PT-BR. */
export function formatarStatusEsg(valor: unknown): string {
  if (valor == null || valor === '') return ''
  return rotulosStatus[String(valor)] ?? String(valor)
}

/** Escopo do inventário/emissão GHG (Protocolo GHG). */
export const escopoGhgOptions: SelectOption[] = [
  { label: 'Escopo 1 — Emissões diretas', value: 1 },
  { label: 'Escopo 2 — Energia adquirida', value: 2 },
  { label: 'Escopo 3 — Cadeia de valor', value: 3 }
]

/** Origem do dado de atividade (EOrigemDadoGhg). */
export const origemDadoGhgOptions: SelectOption[] = [
  { label: 'Manual', value: 'Manual' },
  { label: 'Lote', value: 'Lote' },
  { label: 'Integração', value: 'Integracao' }
]
