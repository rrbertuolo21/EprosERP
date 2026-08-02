/**
 * Enums do módulo Contabilidade — espelham os enums do backend
 * (Epros.Modules.Financeiro/Domain/Enums). A API serializa enums como INTEIRO,
 * portanto os `value` aqui são numéricos e os `label` são a descrição PT-BR.
 *
 * Fica em `contabilidade-contas/` por ser o primeiro consumidor; é reutilizado
 * pelas demais telas do módulo (import direto deste arquivo).
 */
import type { SelectOption } from '~/composables/useEnum'

/** ETipoContaContabil — tipo contábil da conta. */
export const tiposContaContabil: SelectOption[] = [
  { label: 'Ativo', value: 0 },
  { label: 'Passivo', value: 1 },
  { label: 'Patrimônio', value: 2 },
  { label: 'Receita', value: 3 },
  { label: 'Despesa', value: 4 }
]

export function tipoContaLabel(valor: number | null | undefined): string {
  return tiposContaContabil.find((o) => o.value === valor)?.label ?? '—'
}

/** ETipoSaldoContabil — débito/crédito. */
export const tiposSaldoContabil: SelectOption[] = [
  { label: 'Débito', value: 0 },
  { label: 'Crédito', value: 1 }
]

export function tipoSaldoLabel(valor: number | null | undefined): string {
  return tiposSaldoContabil.find((o) => o.value === valor)?.label ?? '—'
}

/** EEstadoPeriodoContabil — estado do período. */
export const estadosPeriodo: SelectOption[] = [
  { label: 'Aberto', value: 0 },
  { label: 'Em Fechamento', value: 1 },
  { label: 'Fechado', value: 2 },
  { label: 'Reaberto', value: 3 }
]

export function estadoPeriodoLabel(valor: number | null | undefined): string {
  return estadosPeriodo.find((o) => o.value === valor)?.label ?? '—'
}

export function estadoPeriodoClasse(valor: number | null | undefined): string {
  switch (valor) {
    case 0: return 'success'
    case 1: return 'warning'
    case 2: return 'danger'
    case 3: return 'info'
    default: return 'secondary'
  }
}

/** EEstadoLancamentoContabil — estado do lançamento. */
export const estadosLancamento: SelectOption[] = [
  { label: 'Rascunho', value: 0 },
  { label: 'Confirmado', value: 1 },
  { label: 'Estornado', value: 2 },
  { label: 'Cancelado', value: 3 }
]

export function estadoLancamentoLabel(valor: number | null | undefined): string {
  return estadosLancamento.find((o) => o.value === valor)?.label ?? '—'
}

export function estadoLancamentoClasse(valor: number | null | undefined): string {
  switch (valor) {
    case 0: return 'secondary'
    case 1: return 'success'
    case 2: return 'warning'
    case 3: return 'danger'
    default: return 'secondary'
  }
}

/** EEstadoCentroCusto — ativo/inativo. */
export const estadosCentroCusto: SelectOption[] = [
  { label: 'Ativo', value: 0 },
  { label: 'Inativo', value: 1 }
]

export function estadoCentroCustoLabel(valor: number | null | undefined): string {
  return estadosCentroCusto.find((o) => o.value === valor)?.label ?? '—'
}

/** ETipoTituloAlocacao — contas a pagar/receber. */
export const tiposTituloAlocacao: SelectOption[] = [
  { label: 'Contas a Pagar', value: 0 },
  { label: 'Contas a Receber', value: 1 }
]

/** EEstadoConsolidacao — provisório/publicado. */
export const estadosConsolidacao: SelectOption[] = [
  { label: 'Provisório', value: 0 },
  { label: 'Publicado', value: 1 }
]

export function estadoConsolidacaoLabel(valor: number | null | undefined): string {
  return estadosConsolidacao.find((o) => o.value === valor)?.label ?? '—'
}

export function estadoConsolidacaoClasse(valor: number | null | undefined): string {
  return valor === 1 ? 'success' : 'secondary'
}
