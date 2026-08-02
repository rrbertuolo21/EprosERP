/**
 * Helpers de exibição para a listagem/form de parceiros (sem IO).
 */
import type { PessoaListaItem } from './types'

/** Nome de exibição da pessoa conforme o tipo. */
export function nomeExibicao(item: PessoaListaItem): string {
  switch (item.tipoPessoa) {
    case 'PessoaFisica':
      return [item.pessoaFisica?.nome, item.pessoaFisica?.sobrenome].filter(Boolean).join(' ')
    case 'PessoaJuridica':
      return item.pessoaJuridica?.razaoSocial || item.pessoaJuridica?.nomeFantasia || ''
    case 'PessoaEstrangeira':
      return item.pessoaEstrangeiro?.nome || ''
    default:
      return ''
  }
}

/** CPF/CNPJ/identificação de exibição conforme o tipo. */
export function documentoExibicao(item: PessoaListaItem, maskCpfCnpj: (v: string) => string): string {
  switch (item.tipoPessoa) {
    case 'PessoaFisica':
      return item.pessoaFisica?.cpf ? maskCpfCnpj(item.pessoaFisica.cpf) : ''
    case 'PessoaJuridica':
      return item.pessoaJuridica?.cnpj ? maskCpfCnpj(item.pessoaJuridica.cnpj) : ''
    case 'PessoaEstrangeira':
      return item.pessoaEstrangeiro?.identificacaoEstrangeiro || ''
    default:
      return ''
  }
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
