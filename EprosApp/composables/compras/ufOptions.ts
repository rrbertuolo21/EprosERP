import type { SelectOption } from '~/composables/useEnum'

/**
 * UFs no índice do enum `EEstado` (Shared/Domain/Enums/EEstado.cs). A API não usa
 * JsonStringEnumConverter, então enums trafegam como INTEIRO na ordem de declaração (base 0).
 * Mantido no front do módulo Compras (TMS, Comércio Exterior) para os selects de UF.
 */
export const UF_SIGLAS = [
  'AC', 'AL', 'AP', 'AM', 'BA', 'CE', 'DF', 'ES', 'GO', 'MA', 'MT', 'MS', 'MG',
  'PA', 'PB', 'PR', 'PE', 'PI', 'RJ', 'RN', 'RS', 'RO', 'RR', 'SC', 'SP', 'SE', 'TO'
] as const

export const UF_OPTIONS: SelectOption[] = UF_SIGLAS.map((sigla, indice) => ({ label: sigla, value: indice }))

export function ufSigla(indice: number | null | undefined): string {
  if (indice == null) return '-'
  return UF_SIGLAS[indice] ?? String(indice)
}
