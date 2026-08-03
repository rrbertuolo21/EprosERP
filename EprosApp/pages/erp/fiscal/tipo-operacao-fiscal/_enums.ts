/**
 * Espelha os enums do backend consumidos pelo Tipo de Operação Fiscal
 * (Epros.Shared.Domain.Enums: EFinalidadeEmissao, ETipoAtendimento, EModalidadeFrete, ETipoMovimento).
 * Não há endpoint de enum dedicado para essas listas — se um `fiscais-enums/*` for exposto no
 * backend, trocar por `useEnum().carregarOpcoes(...)`.
 *
 * Arquivo auxiliar local desta fatia (Fiscal — CFOP + Tipo Operação + NCM); não é uma rota.
 */
import type { SelectOption } from '~/composables/useEnum'

export const opcoesFinalidade: SelectOption[] = [
  { label: 'Normal', value: 1 },
  { label: 'Complementar', value: 2 },
  { label: 'Ajuste', value: 3 },
  { label: 'Devolução', value: 4 },
  { label: 'Nota de Crédito', value: 5 },
  { label: 'Nota de Débito', value: 6 }
]

export const opcoesAtendimento: SelectOption[] = [
  { label: 'Não se aplica', value: 0 },
  { label: 'Operação presencial', value: 1 },
  { label: 'Operação não presencial, pela Internet', value: 2 },
  { label: 'Operação não presencial, Teleatendimento', value: 3 },
  { label: 'NFC-e em operação com entrega a domicílio', value: 4 },
  { label: 'Operação presencial, fora do estabelecimento', value: 5 },
  { label: 'Operação não presencial, outros', value: 9 }
]

export const opcoesTipoFrete: SelectOption[] = [
  { label: 'Contratação do Frete por conta do Remetente (CIF)', value: 0 },
  { label: 'Contratação do Frete por conta do Destinatário (FOB)', value: 1 },
  { label: 'Contratação do Frete por conta de Terceiros', value: 2 },
  { label: 'Transporte Próprio por conta do Remetente', value: 3 },
  { label: 'Transporte Próprio por conta do Destinatário', value: 4 },
  { label: 'Sem Ocorrência de Transporte', value: 9 }
]

export const opcoesTipoMovimento: SelectOption[] = [
  { label: 'Entrada', value: 1 },
  { label: 'Saída', value: 2 }
]

export const FINALIDADE_DEVOLUCAO = 4
export const TIPO_MOVIMENTO_ENTRADA = 1
export const TIPO_MOVIMENTO_SAIDA = 2

function descricao(opcoes: SelectOption[], valor: number | null | undefined): string {
  if (valor === null || valor === undefined) return ''
  return opcoes.find((o) => o.value === valor)?.label ?? String(valor)
}

export const descricaoFinalidade = (v: number | null | undefined) => descricao(opcoesFinalidade, v)
export const descricaoAtendimento = (v: number | null | undefined) => descricao(opcoesAtendimento, v)
export const descricaoTipoFrete = (v: number | null | undefined) => descricao(opcoesTipoFrete, v)
export const descricaoTipoMovimento = (v: number | null | undefined) => descricao(opcoesTipoMovimento, v)
