/**
 * Formatadores de exibição para as colunas das listas do módulo Concessionárias.
 * Usados em `DataTableColumn.formatter`. Puros, sem IO.
 */

/** Valor monetário BRL. */
export function formatMoeda(v: unknown): string {
  if (v === null || v === undefined || v === '') return ''
  const n = Number(v)
  if (Number.isNaN(n)) return ''
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(n)
}

/** Número com casas decimais (quantidade/quilometragem). */
export function formatNumero(v: unknown): string {
  if (v === null || v === undefined || v === '') return ''
  const n = Number(v)
  if (Number.isNaN(n)) return ''
  return new Intl.NumberFormat('pt-BR', { maximumFractionDigits: 3 }).format(n)
}

/** Data (sem hora). */
export function formatData(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  if (Number.isNaN(d.getTime())) return ''
  return d.toLocaleDateString('pt-BR')
}

/** Data e hora. */
export function formatDataHora(v: unknown): string {
  if (!v) return ''
  const d = new Date(String(v))
  if (Number.isNaN(d.getTime())) return ''
  return d.toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
}
