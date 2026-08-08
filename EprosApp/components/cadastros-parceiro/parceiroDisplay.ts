/**
 * Helpers de exibição para a listagem/form de parceiros (sem IO).
 */
import type { PessoaListaItem } from './types'

/**
 * Extrai o valor textual de um documento que a API pode devolver como string crua
 * OU como value-object serializado `{ valor }` (OwnsOne CPF/CNPJ). Tolera ambos.
 */
function valorDocumento(v: unknown): string {
  if (typeof v === 'string') return v
  if (v && typeof v === 'object' && 'valor' in v) {
    const val = (v as { valor?: unknown }).valor
    return val == null ? '' : String(val)
  }
  return ''
}

/**
 * Nome de exibição da pessoa. Detecta o tipo pela sub-entidade presente
 * (imune à representação do enum `tipoPessoa` — número no back, string no contrato do front).
 */
export function nomeExibicao(item: PessoaListaItem): string {
  if (item.pessoaFisica) {
    return [item.pessoaFisica.nome, item.pessoaFisica.sobrenome].filter(Boolean).join(' ')
  }
  if (item.pessoaJuridica) {
    return item.pessoaJuridica.razaoSocial || item.pessoaJuridica.nomeFantasia || ''
  }
  if (item.pessoaEstrangeiro) {
    return item.pessoaEstrangeiro.nome || ''
  }
  return ''
}

/** CPF/CNPJ/identificação de exibição — detecta pela sub-entidade e tolera string ou `{ valor }`. */
export function documentoExibicao(item: PessoaListaItem, maskCpfCnpj: (v: string) => string): string {
  if (item.pessoaFisica) {
    const cpf = valorDocumento(item.pessoaFisica.cpf)
    return cpf ? maskCpfCnpj(cpf) : ''
  }
  if (item.pessoaJuridica) {
    const cnpj = valorDocumento(item.pessoaJuridica.cnpj)
    return cnpj ? maskCpfCnpj(cnpj) : ''
  }
  if (item.pessoaEstrangeiro) {
    return item.pessoaEstrangeiro.identificacaoEstrangeiro || ''
  }
  return ''
}

/** Endereço principal da pessoa (tipoEndereco === 'Principal'), quando existir. */
export function enderecoPrincipal(item: PessoaListaItem) {
  return (item.enderecos ?? []).find((e) => e.tipoEndereco === 'Principal')
}

/** Contato marcado como principal, quando existir. */
export function contatoPrincipal(item: PessoaListaItem) {
  return (item.contatos ?? []).find((c) => c.ehPrincipal)
}

/** Rótulo curto do tipo de pessoa para exibição em selects. */
export const OPCOES_TIPO_PESSOA = [
  { label: 'Pessoa Física', value: 'PessoaFisica' },
  { label: 'Pessoa Jurídica', value: 'PessoaJuridica' },
  { label: 'Pessoa Estrangeira', value: 'PessoaEstrangeira' }
]

export const OPCOES_TIPO_INDICADOR_IE = [
  { label: 'Contribuinte ICMS', value: 'ContribuinteICMS' },
  { label: 'Isento', value: 'Isento' },
  { label: 'Não Contribuinte', value: 'NaoContribuinte' }
]

export const OPCOES_TIPO_ENDERECO = [
  { label: 'Principal', value: 'Principal' },
  { label: 'Cobrança', value: 'Cobranca' },
  { label: 'Entrega', value: 'Entrega' }
]
